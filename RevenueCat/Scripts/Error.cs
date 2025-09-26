using Newtonsoft.Json;

namespace RevenueCat
{
    public class Error
    {
        [JsonProperty("message")]
        public  string Message;

        [JsonProperty("code")]
        public  int Code;

        [JsonProperty("underlyingErrorMessage")]
        public  string UnderlyingErrorMessage;

        [JsonProperty("readableErrorCode")]
        public  string ReadableErrorCode;

        [JsonConstructor]
        internal Error(
            [JsonProperty("message")] string message,
            [JsonProperty("code")] int code,
            [JsonProperty("underlyingErrorMessage")] string underlyingErrorMessage,
            [JsonProperty("readableErrorCode")] string readableErrorCode)
        {
            Message = message;
            Code = code;
            UnderlyingErrorMessage = underlyingErrorMessage;
            ReadableErrorCode = readableErrorCode;
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