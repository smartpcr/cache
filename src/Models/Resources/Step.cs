//-------------------------------------------------------
// <copyright file="Step.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-------------------------------------------------------

namespace Common.Models.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Step of an update. This object represents steps in the summary.xml provided by ECE.
    /// </summary>
    [Serializable]
    public class Step
    {
        private const string EndTimeUtcXmlAttribute = "EndTimeUtc";

        private const string StartTimeUtcXmlAttribute = "StartTimeUtc";

        private const string ExpectedExecutionTimeXmlAttribute = "ExpectedExecutionTime";

        private const string NameXmlAttribute = "Name";

        private const string TypeXmlAttribute = "Type";

        private const string DescriptionXmlAttribute = "Description";

        /// <summary>
        /// Initializes a new instance of the <see cref="Step"/> class.
        /// </summary>
        public Step()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Step"/> class.
        /// </summary>
        /// <param name="xml">The XML representation of the update action plan.</param>
        public Step(XElement xml)
            : this(xml, int.MaxValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Step"/> class.
        /// </summary>
        /// <param name="xml">The XML representation of the update action plan.</param>
        /// <param name="maxDepth">The max depth in the tree to parse.</param>
        public Step(XElement xml, int maxDepth)
            : this(xml, 0, maxDepth)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Step"/> class.
        /// </summary>
        /// <param name="xml">The XML representation of the update action plan.</param>
        /// <param name="currentLevel">The level of the step in the xml</param>
        private Step(XElement xml, int currentLevel, int maxDepth)
        {
            if (xml != null)
            {
                var isAction = xml.Name.LocalName.Equals("Action", StringComparison.OrdinalIgnoreCase);

                // Initial case : the top level element of the summary is an action.
                // Default case : the element of the summary is a step.
                this.Name = xml.Attribute(isAction ? Step.TypeXmlAttribute : Step.NameXmlAttribute)?.Value ?? "Unnamed step";
                this.Description = xml.Attribute(Step.DescriptionXmlAttribute)?.Value ?? string.Empty;
                this.Status = Step.GetAdjustedStatus(xml);

                if (xml.Attribute(Step.StartTimeUtcXmlAttribute)?.Value != null)
                {
                    this.StartTimeUtc = DateTime.Parse(xml.Attribute(Step.StartTimeUtcXmlAttribute)?.Value).ToUniversalTime();
                }

                if (xml.Attribute(Step.EndTimeUtcXmlAttribute)?.Value != null)
                {
                    this.EndTimeUtc = DateTime.Parse(xml.Attribute(Step.EndTimeUtcXmlAttribute)?.Value).ToUniversalTime();
                }

                if (xml.Attribute(Step.ExpectedExecutionTimeXmlAttribute)?.Value != null)
                {
                    this.ExpectedExecutionTime = TimeSpan.Parse(xml.Attribute(Step.ExpectedExecutionTimeXmlAttribute)?.Value);
                }

                var innerSteps = xml.Descendants("Step").Where(x => x.Ancestors().FirstOrDefault(y => y.Name.LocalName.Equals("Step")) == (isAction ? null : xml));

                this.Steps = currentLevel >= maxDepth ? new Step[0] : innerSteps.Select(x => new Step(x, currentLevel + 1, maxDepth)).ToArray();

                this.LastUpdatedTimeUtc = this.EndTimeUtc ?? this.Steps.Select(x => x.LastUpdatedTimeUtc).Where(x => x != null).Max();

                // We gather the error message only for error leafs.
                this.ErrorMessage = this.Status.Equals(Constants.Error, StringComparison.OrdinalIgnoreCase) && (!this.Steps.Any() || currentLevel == maxDepth)
                    ? xml.Descendants("Exception").Elements("Message").FirstOrDefault()?.Value ?? string.Empty
                    : string.Empty;

                // Aggregate progress and errors for steps that are cut off due to the max depth but have child steps.
                if (currentLevel == maxDepth && xml.Descendants("Step").Any())
                {
                    if (string.Equals(this.Status, Constants.InProgress, StringComparison.OrdinalIgnoreCase))
                    {
                        var childStepEndTimes = xml.Descendants("Step")
                            .Where(s => !string.IsNullOrEmpty(s.Attribute(Step.EndTimeUtcXmlAttribute)?.Value))
                            .Select(x => DateTime.Parse(x.Attribute(Step.EndTimeUtcXmlAttribute).Value).ToUniversalTime());

                        this.LastUpdatedTimeUtc = childStepEndTimes.Any() ? childStepEndTimes.Max() : null;
                    }
                    else if (string.Equals(this.Status, Constants.Error, StringComparison.OrdinalIgnoreCase))
                    {
                        this.AggregatedErrorMessages = xml.Descendants("Exception")
                            .Elements("Message")
                            .Select(x => x.Value)
                            .ToList();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the steps.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the step.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the aggregated error messages of all child steps for leaf nodes.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<string> AggregatedErrorMessages { get; set; }

        /// <summary>
        /// Gets or sets the status of the step.
        /// </summary>
        /// <remarks>
        /// No need to use an enum here as this is never used in code. Leaving it a string makes the code more flexible.
        /// </remarks>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the date at which the step started executing.
        /// It will be null if the step has not yet started.
        /// </summary>
        public DateTime? StartTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the date at which the step stopped executing.
        /// It will be null if the step is still executing.
        /// </summary>
        public DateTime? EndTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the expected execution time. This is used to
        /// determine overall update progress. If null, we will take
        /// an equal weighting with the other sibling steps.
        /// </summary>
        public TimeSpan? ExpectedExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the completion time of this step or the last completed substep.
        /// </summary>
        public DateTime? LastUpdatedTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the inner steps composing this step.
        /// </summary>
        public Step[] Steps { get; set; }

        /// <summary>
        /// Returns the status of the specified step. If the overall action status is InProgress,
        /// the status of all underlying steps will also be InProgress to avoid customer confusion.
        /// </summary>
        /// <param name="element">The XML element for which the status will be analyzed.</param>
        /// <returns>A string representing the status of the step.</returns>
        private static string GetAdjustedStatus(XElement element)
        {
            var stepStatus = element.Attribute("Status")?.Value ?? "Unknown status";
            var overallActionStatus = element.AncestorsAndSelf().Last().Attribute("Status")?.Value;
            if (string.Equals(overallActionStatus, Constants.InProgress, StringComparison.OrdinalIgnoreCase)
                && string.Equals(stepStatus, Constants.Error, StringComparison.OrdinalIgnoreCase))
            {
                return Constants.InProgress;
            }

            return stepStatus;
        }
    }
}