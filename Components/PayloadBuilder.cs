using QRay.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QRay.Components
{
    public static class PayloadBuilder
    {
        public static string WiFi(string ssid, string password, NetworkEncryption encryptionType = NetworkEncryption.WPA, bool hidden = false)
        {
            return $"WIFI:T:{(encryptionType == NetworkEncryption.None ? "" : encryptionType)};" +
                   $"S:{EscapeString(ssid)};" +
                   $"P:{EscapeString(password)};" +
                   $"{(hidden ? "H:true;" : "")};";
        }

        public static string SMS(string tel, string message)
        {
            if (!Regex.IsMatch(tel, "^[\\+]?[(]?[0-9]{3}[)]?[-\\s\\.]?[0-9]{3}[-\\s\\.]?[0-9]{4,6}$"))
            {
                throw new ArgumentException("The input is not a valid telephone number");
            }

            return $"SMSTO:{tel}:{EscapeString(message)}";
        }

        private static string EscapeString(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s
                .Replace("\\", "\\\\")
                .Replace(";", "\\;")
                .Replace(",", "\\,")
                .Replace(":", "\\:");
        }
    }
}
