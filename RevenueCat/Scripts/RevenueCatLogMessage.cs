using Newtonsoft.Json;

namespace RevenueCat
{
    public class RevenueCatLogMessage
    {
        public LogLevel Level { get; }

        public string Message { get; }

        [JsonConstructor]
        internal RevenueCatLogMessage(LogLevel level, string message)
        {
            Level = level;
            Message = message;
        }
    }
}
