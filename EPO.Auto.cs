//This Program will input base64 encoded .eml from standedin and output the attachment decoded to standedout
using MimeKit;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;

namespace EPO_Auto_IOC
{
    internal class Program
    {
        private static void Main()
        {
            // TODO replace all powershell.ps1 commands to C# 
            string data = GetInputString().ToString(); // Input base64 encoded .eml from standedin
            string encodeddata = Encoding.ASCII.GetString(Convert.FromBase64String(data)); // decode base64 data
            MimeMessage message = MimeMessage.Load(GenerateStreamFromString(encodeddata)); // Load a MimeMessage from string builder
            // TODO Send all this shit to a file for each eml 
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("==========================");
            Console.WriteLine("type: {0}", "Malware");
            Console.WriteLine("tag: {0}", "TLP:green");
            Console.WriteLine("from: {0}", GetAnonymizer(message.From.ToString()));
            Console.WriteLine("subject: {0}", GetAnonymizer(message.Subject));
            foreach (MimeEntity attachment in message.Attachments)
            {
                MimePart part = (MimePart)attachment; // get the attachment part
                string filename = part.FileName;
                using StreamReader reader = new StreamReader(part.Content.Open()); // Add stream with only attachemnt data
                string value = reader.ReadToEnd();

                // Write Attachment IOC to StandedOutF
                Console.WriteLine("hash|filename: {0}|{1}", GetHashString(value), GetAnonymizer(filename));
                Dictionary<string, List<IPAddress>> urlDic = GetUrls(value);
                foreach (KeyValuePair<string, List<IPAddress>> url in urlDic.OrderByDescending(i => i.Key))
                {
                    Console.WriteLine("url: {0}", url.Key);
                    foreach (IPAddress ip in url.Value)
                    {
                        Console.WriteLine("ip: {0}", ip);
                    }
                }
            }

            // TODO  by Sender e.g. <Victim.lastname@> Anonymizer bellow feilds  <fistname.lastname> or <firstname> or <lastname> or <firstname lastname> etc..
            // subject: Review for <Victim.lastname>
            // hash | filename: 3B7C5B5DFFFA2D7298AC631D55A6AC56B6B0BD427B53E650BEA47DE477666A6D | <Victim.lastname> - Victim.html

            static string GetAnonymizer(string dirtyString)
            {
                if (dirtyString != null)
                {
                    Regex stingparser = new Regex(@"U[A-z]{7}", RegexOptions.Compiled | RegexOptions.IgnoreCase); // Get URLS
                    return stingparser.Replace(dirtyString.ToString(), "Victim");
                }
                return null;
            }
        }

        static Dictionary<string, List<IPAddress>> GetUrls(string value) // URL Collection
        {
            List<String> domainList = new List<String>();
            Dictionary<string, List<IPAddress>> urlDic = new Dictionary<string, List<IPAddress>>();
            if (value.Contains("unescape")) value = HttpUtility.UrlDecode(value); // Decode URL
            value = HttpUtility.HtmlDecode(value); // Decode HTML
            Regex linkParser = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w-./?%&@#=]*)?", RegexOptions.Compiled | RegexOptions.IgnoreCase); // Get URLS
            foreach (Match url in linkParser.Matches(value)) // DNS and IP collection
            {
                List<IPAddress> ipList = new List<IPAddress>();
                Regex urlParser = new Regex(@"([a-z0-9][-a-z0-9_\+\.]*[a-z0-9])@([a-z0-9][-a-z0-9\.]*[a-z0-9]\.)([a-z0-9][a-z0-9])", RegexOptions.Compiled | RegexOptions.IgnoreCase); // Get victims url and clean
                string cleanUrl = urlParser.Replace(url.ToString(), "victim@example.com");
                if (!urlDic.ContainsKey(cleanUrl) & (cleanUrl.Contains("@") || cleanUrl.Contains(".php"))) // If cleanurl in dic.key don't create dup
                {
                    Uri myUri = new Uri(cleanUrl); // Full URL
                    string host = myUri.Host;  // Get only hostname
                    if (!domainList.Contains(host)) // If Domain already in collected don't get ips.
                    {
                        domainList.Add(host);
                        IPHostEntry hostname = Dns.GetHostEntry(host); // Get the dns of host
                        foreach (IPAddress address in hostname.AddressList) ipList.Add(address); // ip from each host
                    }
                    urlDic.Add(cleanUrl, new List<IPAddress>(ipList));
                }
            }
            return urlDic;
        }

        private static StringBuilder GetInputString()
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
        public static byte[] GetHash(string inputString)
        {
            using HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}