using System;
using System.Collections.Generic;

namespace RevenueCatUI
{
    /// <summary>
    /// A value type for custom paywall variables that can be passed to paywalls at runtime.
    ///
    /// Custom variables allow developers to personalize paywall text with dynamic values.
    /// Variables are defined in the RevenueCat dashboard and can be overridden at runtime.
    ///
    /// Currently only string values are supported. Additional types may be added in the future.
    ///
    /// <example>
    /// <code>
    /// var options = new PaywallOptions(
    ///     customVariables: new Dictionary&lt;string, CustomVariableValue&gt;
    ///     {
    ///         { "player_name", CustomVariableValue.String("John") },
    ///         { "level", CustomVariableValue.String("42") },
    ///     }
    /// );
    /// await PaywallsPresenter.Present(options);
    /// </code>
    /// </example>
    ///
    /// In the paywall text (configured in the dashboard), use the <c>custom.</c> prefix:
    /// <code>
    /// Hello {{ custom.player_name }}!
    /// </code>
    /// </summary>
    public abstract class CustomVariableValue
    {
        internal CustomVariableValue() { }

        /// <summary>
        /// Creates a string custom variable value.
        /// </summary>
        /// <param name="value">The string value for the custom variable.</param>
        /// <returns>A CustomVariableValue containing the string.</returns>
        public static CustomVariableValue String(string value) => new StringCustomVariableValue(value);

        /// <summary>
        /// Returns the string representation of this value for native platform consumption.
        /// </summary>
        internal abstract string StringValue { get; }
    }

    /// <summary>
    /// A string custom variable value.
    /// </summary>
    internal sealed class StringCustomVariableValue : CustomVariableValue
    {
        private readonly string _value;

        internal StringCustomVariableValue(string value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal override string StringValue => _value;

        public override bool Equals(object obj)
        {
            if (obj is StringCustomVariableValue other)
                return _value == other._value;
            return false;
        }

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => $"CustomVariableValue.String({_value})";
    }

    /// <summary>
    /// Internal utilities for custom variable conversion.
    /// </summary>
    internal static class CustomVariableValueExtensions
    {
        /// <summary>
        /// Converts a map of custom variables to a map of strings for native platform consumption.
        /// </summary>
        internal static Dictionary<string, string> ToStringDictionary(
            this Dictionary<string, CustomVariableValue> customVariables)
        {
            if (customVariables == null) return null;

            var result = new Dictionary<string, string>();
            foreach (var kvp in customVariables)
            {
                result[kvp.Key] = kvp.Value.StringValue;
            }
            return result;
        }
    }
}
