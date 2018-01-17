﻿using System.Linq;
using NLog;
using NLog.Config;

namespace cafe.CommandLine
{
    public static class LoggingInitializer
    {
        private static readonly Logger Logger = LogManager.GetLogger(typeof(LoggingInitializer).FullName);

        public const string ServerLoggingConfigurationFile = "nlog-server.config";
        private const string ClientLoggingConfigurationFile = "nlog-client.config";

        public static void ConfigureLogging(params string[] args)
        {
            var file = LoggingConfigurationFileFor(args);
            LogManager.Configuration = new XmlLoggingConfiguration(file, false);
            Logger.Info($"Logging set up based on {file}");
        }

        public static string LoggingConfigurationFileFor(string[] args)
        {
            string cafeDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string fileName = args.FirstOrDefault() == "server" ? ServerLoggingConfigurationFile : ClientLoggingConfigurationFile;
            return System.IO.Path.Combine(cafeDir, fileName);
        }
    }
}