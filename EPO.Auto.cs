using MimeKit;
using System;
using System.IO;
using System.Text;

namespace EPO_Auto_IOC
{
    internal class Program
    {
        private static void Main()
        {
            // Build stringbuilder for encoded strings
            StringBuilder encodedStrings = BuildString();
            // Load a MimeMessage from string builder
            MimeMessage message = MimeMessage.Load(GenerateStreamFromString(DecodeBase64(encodedStrings.ToString())));
            foreach (MimeEntity attachment in message.Attachments)
            {
                if (attachment is MessagePart)
                {
                    MessagePart part = (MessagePart)attachment;
                    Console.WriteLine(part);
                }
                else
                {
                    MimePart part = (MimePart)attachment;
                    Stream filename = part.Content.Open();
                    using StreamReader reader = new StreamReader(filename);
                    string value = reader.ReadToEnd();
                    Console.WriteLine(value);
                }
            }
        }

        private static StringBuilder BuildString()
        {
            StringBuilder encodedStrings = new StringBuilder();
            while (true)
            {
                // Read standed in
                string encodedString = Console.ReadLine();
                // Empty line (last line)
                if (string.IsNullOrEmpty(encodedString))
                {
                    break;
                }
                // add each string to stringbuilder
                else
                {
                    _ = encodedStrings.AppendLine(encodedString.ToString());
                }
            }
            return encodedStrings;
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

        private static string DecodeBase64(string base64EncodedValue)
        {
            byte[] data = Convert.FromBase64String(base64EncodedValue);
            return Encoding.ASCII.GetString(data);
        }
    }
}