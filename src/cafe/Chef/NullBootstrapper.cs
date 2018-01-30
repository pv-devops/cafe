using System.Collections.Generic;

namespace cafe.Chef
{
    public class RunChefPolicy : IRunChefPolicy
    {
        protected static readonly string ChefInstallDirectory = $@"{ServerSettings.Instance.InstallRoot}\chef";
        protected static readonly string ClientConfigPath = ServerSettings.Instance.IsLocalMode ?
            $@"{ChefInstallDirectory}\.chef\config.rb" :
            $@"{ChefInstallDirectory}\client.rb";

        public virtual void PrepareEnvironmentForChefRun()
        {
            // nothing to do here
        }

        public virtual string[] ArgumentsForChefRun()
        {
            var arguments = new List<string> {"-c", ClientConfigPath};
            arguments.AddRange(AdditionalArgumentsForChefRun());
            return arguments.ToArray();
        }

        protected virtual string[] AdditionalArgumentsForChefRun()
        {
            return ServerSettings.Instance.IsLocalMode ? new[] { "--local-mode" } : new string[0];
        }

        public override string ToString()
        {
            return "normally";
        }
    }
}