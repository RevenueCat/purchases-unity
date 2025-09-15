using System.Collections.Generic;
using System.Text;

namespace RevenueCat.UI
{
    /// <summary>
    /// Simple JSON utility for serializing options.
    /// </summary>
    internal static class JsonUtility
    {
        public static string Serialize(Dictionary<string, object> dict)
        {
            if (dict == null || dict.Count == 0)
                return "{}";

            var sb = new StringBuilder();
            sb.Append("{");

            bool first = true;
            foreach (var kvp in dict)
            {
                if (!first)
                    sb.Append(",");

                sb.Append($"\"{kvp.Key}\":");
                
                if (kvp.Value is string)
                    sb.Append($"\"{kvp.Value}\"");
                else if (kvp.Value is bool)
                    sb.Append(kvp.Value.ToString().ToLower());
                else
                    sb.Append(kvp.Value?.ToString() ?? "null");

                first = false;
            }

            sb.Append("}");
            return sb.ToString();
        }
    }
} 
