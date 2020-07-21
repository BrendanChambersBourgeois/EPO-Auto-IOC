//This Program will input base64 encoded .eml from standedin and output the attachment decoded to standedout
using MimeKit;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace EPO_Auto_IOC
{
    internal class Program
    {
        private static void Main()
        {
            string data = InputString().ToString(); // Input base64 encoded .eml from standedin
            string encodeddata = Encoding.ASCII.GetString(Convert.FromBase64String(data)); // decode base64 data
            MimeMessage message = MimeMessage.Load(GenerateStreamFromString(encodeddata)); // Load a MimeMessage from string builder
            foreach (MimeEntity attachment in message.Attachments)
            {
                MimePart part = (MimePart)attachment; // get the attachment part
                using StreamReader reader = new StreamReader(part.Content.Open()); // Add stream with only attachemnt data
                string value = reader.ReadToEnd();
                if (value.Contains("unescape")) value = HttpUtility.UrlDecode(value); // Decode URL
                value = HttpUtility.HtmlDecode(value); // Decode HTML
                var linkParser = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&@=]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase); // Get URLS
                foreach (Match m in linkParser.Matches(value)) Console.WriteLine(m); // find URLS and send to standedout
            }
        }

        private static StringBuilder InputString()
        {
            StringBuilder values = new StringBuilder();
            string value = Console.ReadLine();
            while (!string.IsNullOrEmpty(value))
            {
                values.AppendLine(value);
                value = Console.ReadLine();
            }
            return values;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}