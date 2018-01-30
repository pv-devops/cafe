using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using cafe.Client;
using cafe.CommandLine;
using cafe.CommandLine.LocalSystem;
using cafe.LocalSystem;
using cafe.Shared;

namespace cafe.Options.Chef
{
    public class BootstrapChefZeroPolicyOption : RunJobOption<IChefServer>
    {
        private readonly IFileSystemCommands _fileSystemCommands;

        public BootstrapChefZeroPolicyOption(Func<IChefServer> chefServerFactory, ISchedulerWaiter schedulerWaiter,
            IFileSystemCommands fileSystemCommands)
            : base(chefServerFactory, schedulerWaiter,
                "bootstraps chef-zero to run the first time with the given policy group, exported repo, and optional data_bag name and definition")
        {
            _fileSystemCommands = fileSystemCommands;
        }

        protected override string ToDescription(Argument[] args)
        {
            string description = $"Bootstrapping chef-zero with Repo {FindRepoValue(args)} and Group {FindGroupValue(args)}";
            if (!string.IsNullOrEmpty(FindDataBagNameValue(args)) & !string.IsNullOrEmpty(FindDataBagUrlValue(args)))
            {
                description += $" using DataBag {FindDataBagNameValue(args)} at {FindDataBagUrlValue(args)}";
            }
            return description;
        }

        private static string FindGroupValue(Argument[] args)
        {
            return args.FindValueFromLabel("group:").Value;
        }

        private static string FindRepoValue(Argument[] args)
        {
            return args.FindValueFromLabel("repo:").Value;
        }

        private static string FindDataBagNameValue(Argument[] args)
        {
            return args.FindValueFromLabel("data-bag:").Value ?? "";
        }

        private static string FindDataBagUrlValue(Argument[] args)
        {
            return args.FindValueFromLabel("data-bag-json:").Value ?? "";
        }

        protected override Task<JobRunStatus> RunJobCore(IChefServer productServer, Argument[] args)
        {
            return productServer.BootstrapChefZero(FindGroupValue(args), FindRepoValue(args), FindDataBagNameValue(args), FindDataBagUrlValue(args));
        }
    }
}