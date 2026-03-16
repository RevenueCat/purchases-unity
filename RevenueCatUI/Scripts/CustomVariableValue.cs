using System;
using System.Collections.Generic;
using RevenueCat.SimpleJSON;

namespace RevenueCatUI
{
    /// <summary>
    /// A value type for custom paywall variables that can be passed to paywalls at runtime.
    ///
    /// Custom variables allow developers to personalize paywall text with dynamic values.
    /// Variables are defined in the RevenueCat dashboard and can be overridden at runtime.
    ///
    /// <example>
    /// <code>
    /// var options = new PaywallOptions(
    ///     customVariables: new Dictionary&lt;string, CustomVariableValue&gt;
    ///     {
    ///         { "player_name", CustomVariableValue.String("John") },
    ///         { "level", CustomVariableValue.Number(42) },
    ///         { "is_premium", CustomVariableValue.Boolean(true) },
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
        /// Creates a numeric custom variable value.
        /// </summary>
        /// <param name="value">The numeric value for the custom variable.</param>
        /// <returns>A CustomVariableValue containing the number.</returns>
        public static CustomVariableValue Number(double value) => new NumberCustomVariableValue(value);

        /// <summary>
        /// Creates a boolean custom variable value.
        /// </summary>
        /// <param name="value">The boolean value for the custom variable.</param>
        /// <returns>A CustomVariableValue containing the boolean.</returns>
        public static CustomVariableValue Boolean(bool value) => new BooleanCustomVariableValue(value);

        /// <summary>
        /// Writes this value to a JSON object with the appropriate native type.
        /// </summary>
        internal abstract void WriteToJson(JSONObject dict, string key);
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

        internal override void WriteToJson(JSONObject dict, string key)
        {
            dict[key] = _value;
        }

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
    /// A numeric custom variable value.
    /// </summary>
    internal sealed class NumberCustomVariableValue : CustomVariableValue
    {
        private readonly double _value;

        internal NumberCustomVariableValue(double value)
        {
            _value = value;
        }

        internal override void WriteToJson(JSONObject dict, string key)
        {
            dict[key] = _value;
        }

        public override bool Equals(object obj)
        {
            if (obj is NumberCustomVariableValue other)
                return _value.Equals(other._value);
            return false;
        }

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => $"CustomVariableValue.Number({_value})";
    }

    /// <summary>
    /// A boolean custom variable value.
    /// </summary>
    internal sealed class BooleanCustomVariableValue : CustomVariableValue
    {
        private readonly bool _value;

        internal BooleanCustomVariableValue(bool value)
        {
            _value = value;
        }

        internal override void WriteToJson(JSONObject dict, string key)
        {
            dict[key] = _value;
        }

        public override bool Equals(object obj)
        {
            if (obj is BooleanCustomVariableValue other)
                return _value == other._value;
            return false;
        }

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => $"CustomVariableValue.Boolean({_value})";
    }
}
