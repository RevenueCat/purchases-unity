﻿using System;
using System.ComponentModel;
using static Purchases;

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
        Warning,
        [Description("ERROR")]
        Error
    }
}

public static class Extensions
{
    public static string Name(this LogLevel level)
    {
        var type = level.GetType();
        var memInfo = type.GetMember(level.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
        var stringValue = ((DescriptionAttribute)attributes[0]).Description;
        return stringValue;
    }
}
