using System;
using System.ComponentModel;

public partial class Purchases
{
    /// <summary>
    /// Enum of available log levels.
    /// </summary>
    public enum LogLevel
    {
        [Description("VERBOSE")]
        Verbose,
        [Description("DEBUG")]
        Debug,
        [Description("INFO")]
        Info,
        [Description("WARN")]
        Warn,
        [Description("ERROR")]
        Error
    }
}

public static class Extensions
{
    public static string Name(this Purchases.LogLevel level)
    {
        var type = level.GetType();
        var memInfo = type.GetMember(level.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
        var stringValue = ((DescriptionAttribute)attributes[0]).Description;
        return stringValue;
    }
}
