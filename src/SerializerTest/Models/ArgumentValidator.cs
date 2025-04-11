//-----------------------------------------------------------------------
// <copyright file="ArgumentValidator.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.AzureStack.Services.Fabric.Common.Resource.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides common argument validation routines that methods can use to more easily
    /// validate the parameters passed to them.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ArgumentValidator
    {
        private const string LowerBoundMessageTemplate = "Parameter {0} cannot be strictly lower than {1}";
        private const string UpperBoundMessageTemplate = "Parameter {0} cannot be strictly higher than {1}";

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> is <paramref name="toValidate"/> is not within the range [<paramref name="lowerBound"/>, <paramref name="upperBound"/>] <c>false</c>.
        /// </summary>
        /// <param name="toValidate">The integer to be validated.</param>
        /// <param name="lowerBound">The lower bound of the valid range. It is inclusive. If null, the range is considered open on the lower end.</param>
        /// <param name="upperBound">The upper bound of the valid range. It is inclusive. If null, the range is considered open on the upper end.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="lowerBoundMessage">A custom exception message if the lower bound condition is not met.</param>
        /// <param name="upperBoundMessage">A custom exception message if the upper bound condition is not met.</param>
        public static void IsInRange(int toValidate, int? lowerBound, int? upperBound, string paramName, string lowerBoundMessage = null, string upperBoundMessage = null)
        {
            if (lowerBound.HasValue && toValidate < lowerBound.Value)
            {
                throw new ArgumentException(lowerBoundMessage ?? string.Format(ArgumentValidator.LowerBoundMessageTemplate, paramName, lowerBound), paramName);
            }

            if (upperBound.HasValue && toValidate < upperBound.Value)
            {
                throw new ArgumentException(upperBoundMessage ?? string.Format(ArgumentValidator.UpperBoundMessageTemplate, paramName, upperBound), paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> is <paramref name="condition"/> is <c>false</c>.
        /// </summary>
        /// <param name="condition">The condition of the parameter to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="message">The exception message.</param>
        public static void IsTrue(bool condition, string paramName, string message)
        {
            if (!condition)
            {
                throw new ArgumentException(message, paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if <paramref name="obj"/> is <c>null</c>.
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <param name="paramName">The name of the parameter that we are checking, so that we can
        /// include this information in the exception.</param>
        /// <param name="message">If provided, this is going to be the message in the exception thrown.</param>
        public static void NotNull(object obj, string paramName, string message = null)
        {
            if (obj == null)
            {
                throw message == null
                    ? new ArgumentNullException(paramName)
                    : new ArgumentNullException(paramName, message);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if <paramref name="str"/> is <c>null</c> and an
        /// <see cref="ArgumentException"/> if it is empty.
        /// </summary>
        /// <param name="str">The string to check</param>
        /// <param name="paramName">The name of the parameter that we are checking, so that we can
        /// include this information in the exception.</param>
        /// <param name="message">If provided, this is going to be the message in the exception thrown.</param>
        public static void NotNullOrEmpty(string str, string paramName, string message = null)
        {
            ArgumentValidator.NotNull(str, paramName, message);
            if (str == string.Empty)
            {
                throw message == null
                        ? new ArgumentException(paramName)
                        : new ArgumentException(paramName, message);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if <paramref name="str"/> is  not a well formed Uri of the kind
        /// specified by <paramref name="kind"/>.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="paramName">The name of the parameter that we are checking, so that we can
        /// include this information in the exception.</param>
        /// <param name="kind">The Uri kind to check.</param>
        /// <param name="message">If provided, this is going to be the message in the exception thrown.</param>
        public static void IsWellFormedUri(string str, string paramName, UriKind kind, string message = null)
        {
            if (!Uri.IsWellFormedUriString(str, kind))
            {
                throw message == null
                    ? new ArgumentException(paramName)
                    : new ArgumentException(paramName, message);
            }
        }
    }
}
