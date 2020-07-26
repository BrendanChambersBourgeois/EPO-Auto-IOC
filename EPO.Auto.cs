//This Program will input base64 encoded .eml from standedin and output the attachment decoded to standedout
// TODO add support for email body and other attachments.
// TODO create a % of how bad the webpage may be. 
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace EPO_Auto_IOC
{
    internal class Program
    {
        class IOC
        {
            public string type;
            public string tag;
            public string from;
            public string subject;
            public Dictionary<List<string>, Dictionary<string, List<IPAddress>>> attachments;

            public void WriteToConsole() // send to standed out
            {
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("========================");
                Console.WriteLine("type: {0}", type);
                Console.WriteLine("tag: {0}", tag);
                Console.WriteLine("from: {0}", from);
                Console.WriteLine("subject: {0}", subject);
                foreach (KeyValuePair<List<string>, Dictionary<string, List<IPAddress>>> attachment in attachments)
                {
                    Console.WriteLine("hash|filename: {0}|{1}", attachment.Key[1], attachment.Key[0]);
                    foreach (KeyValuePair<string, List<IPAddress>> url in attachment.Value.OrderByDescending(i => i.Key))
                    {
                        Console.WriteLine("url: {0}", url.Key);
                        foreach (IPAddress ip in url.Value)
                        {
                            Console.WriteLine("ip: {0}", ip);
                        }
                    }
                }
            }

            public void WriteToFile()    // send to file
            {
                string filename = $@"IOC\{Guid.NewGuid()}-IOC.txt";
                if (!File.Exists(filename))
                {
                    using var tw = new StreamWriter(filename, true);
                    tw.WriteLine("type: {0}", type);
                    tw.WriteLine("tag: {0}", tag);
                    tw.WriteLine("from: {0}", from);
                    tw.WriteLine("subject: {0}", subject);
                    foreach (KeyValuePair<List<string>, Dictionary<string, List<IPAddress>>> attachment in attachments)
                    {
                        tw.WriteLine("hash|filename: {0}|{1}", attachment.Key[1], attachment.Key[0]);
                        foreach (KeyValuePair<string, List<IPAddress>> url in attachment.Value.OrderByDescending(i => i.Key))
                        {
                            tw.WriteLine("url: {0}", url.Key);
                            foreach (IPAddress ip in url.Value)
                            {
                                tw.WriteLine("ip: {0}", ip);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("File Error");
                }
            }
        }

        private static void Main()
        {
            // TODO replace all powershell.ps1 commands to C# 
            string data = GetInputString().ToString(); // Input base64 encoded .eml from standedin
            string encodeddata = Encoding.ASCII.GetString(Convert.FromBase64String(data)); // decode base64 data
            MimeMessage message = MimeMessage.Load(GenerateStreamFromString(encodeddata)); // Load a MimeMessage from string builder
            string recipient = message.To.ToString();
            IOC main = new IOC
            {
                type = "Malware",
                tag = "TLP:green",
                from = GetAnonymizer(message.From.ToString(), recipient),
                subject = GetAnonymizer(message.Subject, recipient)
            };
            List<string> fileNameHash = new List<string>();
            Dictionary<List<string>, Dictionary<string, List<IPAddress>>> attachments = new Dictionary<List<string>, Dictionary<string, List<IPAddress>>>();
            foreach (MimeEntity attachment in message.Attachments)
            {
                MimePart part = (MimePart)attachment; // get the attachment part
                using StreamReader reader = new StreamReader(part.Content.Open()); // Add stream with only attachemnt data
                string value = reader.ReadToEnd();
                fileNameHash.Add(GetAnonymizer(part.FileName, recipient));
                fileNameHash.Add(GetHashString(value));
                attachments.Add(fileNameHash, new Dictionary<string, List<IPAddress>>(GetUrls(value)));
            }
            main.attachments = attachments;
            // main.WriteToConsole();
            if (attachments.Values.Count() > 0) main.WriteToFile();

            static string GetAnonymizer(string dirtyString, string recipient)
            {
                string cleanstring = null;

                if (dirtyString != null) // Get company name and replaces 
                {
                    Regex stingparser = new Regex(@"U[A-z]{7}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    cleanstring = stingparser.Replace(dirtyString.ToString(), "VictimCompany");
                }
                string user = recipient.Split("@")[0];
                if (user.Split(".").Length > 1) // gets reciptient name and replaces
                {
                    string firstname = user.Split(".")[0];
                    string lastname = user.Split(".")[1];
                    cleanstring = cleanstring.Replace(firstname.ToString(), "FirstName", StringComparison.OrdinalIgnoreCase);
                    cleanstring = cleanstring.Replace(lastname.ToString(), "LastName", StringComparison.OrdinalIgnoreCase);
                }
                return cleanstring;
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

