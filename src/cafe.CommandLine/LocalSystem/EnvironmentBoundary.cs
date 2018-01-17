using NLog;

namespace cafe.CommandLine.LocalSystem
{
    public class EnvironmentBoundary : IEnvironment
    {
        private static readonly Logger Logger = LogManager.GetLogger(typeof(EnvironmentBoundary).FullName);

        public string GetEnvironmentVariable(string key)
        {
            var value = System.Environment.GetEnvironmentVariable(key);
            Logger.Debug($"Retrieved environment variable {key} with value: {value}");
            return value;
        }

        public void SetSystemEnvironmentVariable(string key, string value)
        {
            System.Environment.SetEnvironmentVariable(key, value, System.EnvironmentVariableTarget.Machine);
        }
    }
}