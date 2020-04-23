using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RevenueCat.SimpleJSON;

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

        public Error(JSONNode response)
        {
            message = response["message"];
            code = (int) response["code"];
            underlyingErrorMessage = response["underlyingErrorMessage"];
            readableErrorCode = response["readableErrorCode"];
        }

        public override string ToString()
        {
            return $"{nameof(message)}: {message}, " +
                   $"{nameof(code)}: {code}, " +
                   $"{nameof(underlyingErrorMessage)}: {underlyingErrorMessage}, " +
                   $"{nameof(readableErrorCode)}: {readableErrorCode}";
        }
    }
}