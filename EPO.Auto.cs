using System;
using System.Text;
using System.IO;
using MimeKit;

namespace EPO_Auto_IOC
{
    internal class Program
    {
        static void Main()
        {
            // Build stringbuilder for encoded strings
            var encodedStrings = new System.Text.StringBuilder();
            while(true)
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
                    encodedStrings.AppendLine(encodedString.ToString());
                }
            }
            // Load a MimeMessage from string builder
            var message = MimeMessage.Load(GenerateStreamFromString(DecodeBase64(encodedStrings.ToString())));
            foreach (MimeEntity attachment in message.Attachments)
            {
                if (attachment is MessagePart)
                {
                    var part = (MessagePart)attachment;
                    Console.WriteLine(part);
                }
                else
                {
                    var part = (MimePart)attachment;
                    var filename = part.Content.Open();
                    using var reader = new StreamReader(filename);
                    string value = reader.ReadToEnd();
                    Console.WriteLine(value);
                }
            }
        }
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
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