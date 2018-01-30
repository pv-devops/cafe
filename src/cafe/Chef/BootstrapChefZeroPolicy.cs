using cafe.CommandLine.LocalSystem;
using NLog;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using cafe.CommandLine;

namespace cafe.Chef
{
    public class BootstrapChefZeroPolicy : RunChefPolicy
    {
        private static readonly Logger Logger = LogManager.GetLogger(typeof(BootstrapChefZeroPolicy).FullName);

        private static readonly string ClientLocalModeConfigPath = $@"{ChefInstallDirectory}\.chef\config.rb";

        private readonly IFileSystemCommands _fileSystemCommands;
        private readonly string _policyGroup;
        private readonly string _repoUrl;
        private readonly string _dataBagName;
        private readonly string _dataBagUrl;

        public BootstrapChefZeroPolicy(IFileSystemCommands fileSystemCommands, string policyGroup, string repoUrl, string dataBagName, string dataBagUrl)
        {
            _fileSystemCommands = fileSystemCommands;
            _policyGroup = policyGroup;
            _repoUrl = repoUrl;
            _dataBagName = dataBagName;
            _dataBagUrl = dataBagUrl;
        }

        public override string[] ArgumentsForChefRun()
        {
            var arguments = new List<string> { "-c", ClientLocalModeConfigPath };
            arguments.AddRange(AdditionalArgumentsForChefRun());
            return arguments.ToArray();
        }
        
        protected override string[] AdditionalArgumentsForChefRun()
        {
            return new[] { "--local-mode" };
        }

        public override string ToString()
        {
            return $"for the first time with group {_policyGroup} using repo {_repoUrl} and data_bag {_dataBagUrl}";
        }

        public override void PrepareEnvironmentForChefRun()
        {
            Logger.Info("Preparing chef-zero environment for the first run");
            DeployChefZeroPolicyRepo();
        }
        
        private void DeployChefZeroPolicyRepo()
        {
            _fileSystemCommands.EnsureDirectoryExists(ChefInstallDirectory);
            Logger.Debug($"Deploying {_repoUrl} to {ChefInstallDirectory}");
            try
            {
                FetchChefZeroPolicyRepo().Wait();
                UpdateServerJson();
                if (!string.IsNullOrEmpty(_dataBagUrl) & !string.IsNullOrEmpty(_dataBagName))
                {
                    WriteDataBag().Wait();
                }
                else
                {
                    Logger.Warn($"Skipping preparation of data_bag since a valid data_bag URL and name must be supplied when bootstraping a chef-zero policy repo. Details: name = {_dataBagName} | url = {_dataBagUrl}");
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error($"An error occurred while deploying chef-zero policy repo {_repoUrl} | Details: {ex.Message} :: {ex.StackTrace}");
                throw;
            }
        }

        private async Task FetchChefZeroPolicyRepo()
        {
            Logger.Debug($"Fetching chef-zero policy repo {_repoUrl}...");
            HttpClient client = new HttpClient();
            var task = client.GetStreamAsync(_repoUrl);
            Stream inStream = await task;
            Logger.Debug($"Unpacking chef-zero repo file stream to {ChefInstallDirectory}");
            Stream gzipStream = new GZipInputStream(inStream);
            TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
            tarArchive.ExtractContents(ChefInstallDirectory);
            tarArchive.Close();
            gzipStream.Close();
            inStream.Close();
        }

        private async Task WriteDataBag()
        {
            string dataBagParentDir = $@"{ChefInstallDirectory}\data_bags";
            string dataBagDir = $@"{dataBagParentDir}\{_dataBagName}";
            string dataBagFileName = Path.GetFileName(_dataBagUrl);
            string dataBagPath = $@"{dataBagDir}\{dataBagFileName}";

            Logger.Debug($"Fetching data_bag {_dataBagName} at {_dataBagUrl}...");
            HttpClient client = new HttpClient();
            var task = client.GetStringAsync(_dataBagUrl);
            string dataBagJson = await task;
            Logger.Debug($"Saving data_bag contents to {dataBagPath}...");
            _fileSystemCommands.EnsureDirectoryExists(dataBagDir);
            _fileSystemCommands.WriteFileText(dataBagPath, dataBagJson);
        }

        private void UpdateServerJson()
        {
            Logger.Debug($"Writing server.json with IsLocalMode = true");
            var chefZeroSettings = SettingsReader.Read<ServerSettings>("Server", "server.json");
            chefZeroSettings.IsLocalMode = true;
            SettingsWriter.Write("server.json", chefZeroSettings);
        }
    }
}