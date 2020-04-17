using System;
using System.Diagnostics.CodeAnalysis;

public partial class Purchases
{
    [Serializable]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public class Error
    {
        public string message;
        public int code;
        public string underlyingErrorMessage;
        public string readableErrorCode;
    }
}