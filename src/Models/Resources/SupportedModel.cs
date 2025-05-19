//------------------------------------------------------------------
// <copyright file="SupportedModel.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//------------------------------------------------------------------

namespace Common.Models.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    [Serializable]
    public class SupportedModel
    {
        /// <summary>
        /// The model name for which an update is supported.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The optional list of SKUs for which this update is supported.
        /// </summary>
        public IEnumerable<string> SupportedSkus { get; set; } = new List<string>();

        /// <summary>
        /// THe optional list of SKUs for which this update is NOT supported.
        /// </summary>
        public IEnumerable<string> NotSupportedSkus { get; set; } = new List<string>();

        public SupportedModel() => new SupportedModel(null);

        public SupportedModel(string name)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            var toString = this.Name;
            if (this.SupportedSkus.Any())
            {
                toString += $". SupportedSKUs: {string.Join(", ", this.SupportedSkus)}";
            }
            if (this.NotSupportedSkus.Any())
            {
                toString += $". NotSupportedSKUs: {string.Join(", ", this.NotSupportedSkus)}";
            }

            return toString;
        }

        public static SupportedModel FromXml(XElement xml)
        {
            return new SupportedModel()
            {
                Name = xml.Value,
                SupportedSkus = SupportedModel.ParseSkuValues(xml.Attribute("SupportedSKUs")?.Value),
                NotSupportedSkus = SupportedModel.ParseSkuValues(xml.Attribute("NotSupportedSKUs")?.Value)
            };
        }

        public XElement ToXElement()
        {
            var element = new XElement("SupportedModel", this.Name);
            if (this.SupportedSkus.Any())
            {
                element.Add(new XAttribute("SupportedSKUs", string.Join(";", this.SupportedSkus)));
            }

            if (this.NotSupportedSkus.Any())
            {
                element.Add(new XAttribute("NotSupportedSKUs", string.Join(";", this.NotSupportedSkus)));
            }

            return element;
        }

        private static IEnumerable<string> ParseSkuValues(string skuValues)
        {
            if (string.IsNullOrEmpty(skuValues))
            {
                return Enumerable.Empty<string>();
            }

            return skuValues.Split(';')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeString)
                .ToList();
        }

        public static string NormalizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return Regex.Replace(input, "[^a-zA-Z0-9]", "");
        }
    }
}
