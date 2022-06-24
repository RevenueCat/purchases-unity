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
        public readonly string Message;
        public readonly int Code;
        public readonly string UnderlyingErrorMessage;
        public readonly string ReadableErrorCode;

        public Error(JSONNode response)
        {
            Message = response["message"];
            Code = (int) response["code"];
            UnderlyingErrorMessage = response["underlyingErrorMessage"];
            ReadableErrorCode = response["readableErrorCode"];
        }

        public override string ToString()
        {
            return $"{nameof(Message)}: {Message}, " +
                   $"{nameof(Code)}: {Code}, " +
                   $"{nameof(UnderlyingErrorMessage)}: {UnderlyingErrorMessage}, " +
                   $"{nameof(ReadableErrorCode)}: {ReadableErrorCode}";
        }
    }
}