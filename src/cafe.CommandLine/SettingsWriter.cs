using System;
using System.IO;
using Newtonsoft.Json;
using NLog;

namespace cafe.CommandLine
{
    public class SettingsWriter
    {
        private static readonly Logger Logger = LogManager.GetLogger(typeof(SettingsWriter).FullName);

        public static void Write(string file, object value)
        {
            using (StreamWriter fileStream = File.CreateText(file))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(fileStream, value);
            }
        }
    }
}