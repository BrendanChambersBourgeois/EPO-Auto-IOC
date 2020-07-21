//This Program will input base64 encoded .eml from standedin and output the attachment decoded to standedout
using MimeKit;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;

namespace EPO_Auto_IOC
{
    internal class Program
    {
        private static void Main()
        {
            string data = InputString().ToString(); // Input base64 encoded .eml from standedin
            string encodeddata = Encoding.ASCII.GetString(Convert.FromBase64String(data)); // decode base64 data
            MimeMessage message = MimeMessage.Load(GenerateStreamFromString(encodeddata)); // Load a MimeMessage from string builder
            // -------------------------------------------
            // TODO add header support & IOC list
            // e.g
            // type: phish
            // from: evil@user.com
            // subject: New Audio Message
            // Attachment: 📞Audio_Message-223_101.htm
            // Date: 01-01-01 11:11:11
            // -------------------------------------------
                //foreach (MimeEntity body in message.BodyParts)
                //{
                //    MimePart part = (MimePart)body;
                //    using StreamReader reader = new StreamReader(part.Content.Open());
                //    string value = reader.ReadToEnd();
                //    Console.WriteLine(value);
                //}
            // -------------------------------------------
            // TODO add attachment IOC list support 
            // e.g
            // url: hXXps://evil[.]com/view/bank/pishing
            // ip: 12.34.56.78
            // -------------------------------------------
            foreach (MimeEntity attachment in message.Attachments)
            {
                MimePart part = (MimePart)attachment; // get the attachment part
                using StreamReader reader = new StreamReader(part.Content.Open()); // Add stream with only attachemnt data
                string value = reader.ReadToEnd();
                if (value.Contains("unescape")) value = HttpUtility.UrlDecode(value); // Decode URL
                value = HttpUtility.HtmlDecode(value); // Decode HTML
                var linkParser = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w-./?%&@#=]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase); // Get URLS
                foreach (Match url in linkParser.Matches(value))
                {
                    Regex regex = new Regex(@"([a-z0-9][-a-z0-9_\+\.]*[a-z0-9])@([a-z0-9][-a-z0-9\.]*[a-z0-9]\.)([a-z0-9][a-z0-9])", RegexOptions.Compiled | RegexOptions.IgnoreCase); // Get victims url and clean
                    string cleanUrl = regex.Replace(url.ToString(), "victim@example.com");
                    Uri myUri = new Uri(cleanUrl); // Full URL
                    string host = myUri.Host;  // Get only hostname
                    IPHostEntry hostname = Dns.GetHostEntry(host); // Get the dns of host
                    Console.WriteLine(cleanUrl);
                    foreach (IPAddress address in hostname.AddressList) // ip from each host
                    {
                        Console.WriteLine(address);
                    }
                }
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