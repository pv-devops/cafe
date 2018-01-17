using System;
using System.Runtime.InteropServices;
using cafe.CommandLine;
using cafe.CommandLine.LocalSystem;
using cafe.LocalSystem;
using cafe.Shared;
using NLog;

namespace cafe.Options
{
    public class InitOption : Option
    {
        [Flags]
        enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            UIntPtr wParam,
            IntPtr lParam,
            SendMessageTimeoutFlags fuFlags,
            uint uTimeout,
            out UIntPtr lpdwResult);

        private static readonly Logger Logger = LogManager.GetLogger(typeof(InitOption).FullName);

        private readonly string _cafeDirectory;
        private readonly IEnvironment _environment;

        public InitOption(string cafeDirectory, IEnvironment environment) : base(
            "initializes cafe to run on this machine")
        {
            _cafeDirectory = cafeDirectory;
            _environment = environment;
        }

        protected override string ToDescription(Argument[] args)
        {
            return "Initializing Cafe to Run on This Machine";
        }

        public const string PathEnvironmentVariableKey = "PATH";

        protected override Result RunCore(Argument[] args)
        {
            var path = GetPathEnvironmentVariable();
            if (!path.Contains(_cafeDirectory))
            {
                Presenter.ShowMessage("Adding Cafe to path environment variable so it can run from anywhere", Logger);
                Logger.Info($"Path does not contain cafe directory {_cafeDirectory}, so adding it");
                path += $";{_cafeDirectory}";
                _environment.SetSystemEnvironmentVariable(PathEnvironmentVariableKey, path);
                if (!BroadcastPathChange())
                {
                    Presenter.ShowMessage("You'll need to reboot for these changes to be in effect", Logger);
                }
                else
                {
                    Presenter.ShowMessage("You'll need to close/reopen shell window for these changes to be in effect", Logger);
                }
                Logger.Debug($"After updating path, its value is now {GetPathEnvironmentVariable()}");
            }
            else
            {
                Presenter.ShowMessage("Cafe is already on the PATH environment variable", Logger);
            }
            return Result.Successful();
        }

        private string GetPathEnvironmentVariable()
        {
            var path = _environment.GetEnvironmentVariable(PathEnvironmentVariableKey);
            return path;
        }

        private bool BroadcastPathChange()
        {
            IntPtr HWND_BROADCAST = new IntPtr(0xffff);
            const uint WM_WININICHANGE = 0x001A;
            const uint WM_SETTINGCHANGE = WM_WININICHANGE;
            const int MSG_TIMEOUT = 5000;
            UIntPtr MSG_RESULT;
            IntPtr result = SendMessageTimeout(
                HWND_BROADCAST,
                WM_SETTINGCHANGE,
                UIntPtr.Zero,
                (IntPtr)Marshal.StringToHGlobalAnsi("Environment"),
                SendMessageTimeoutFlags.SMTO_ABORTIFHUNG,
                MSG_TIMEOUT,
                out MSG_RESULT);
            return result != IntPtr.Zero ? true : false;
        }
    }
}