using System.Collections.Generic;
using System.Linq;
using CommandDotNet.MicrosoftCommandLineUtils;
using CommandDotNet.Models;

namespace CommandDotNet.AutoCompletion
{
    public class AutoCompleteInfo
    {
        private AutoCompleteInfo(string name)
        {
            Name = name;
        }
        
        public string Name { get; }
        
        public List<string> OptionNames { get; } = new List<string>();
        
        public List<AutoCompleteInfo> SubCommands { get; } = new List<AutoCompleteInfo>();
        
        
        public static AutoCompleteInfo Generate<T>(AppRunner<T> appRunner) where T : class
        {
            AppCreator appCreator = new AppCreator(appRunner.Settings);
            CommandLineApplication rootCommand = appCreator.CreateApplication(typeof(T), appRunner.DependencyResolver);
            AutoCompleteInfo details = GetDetails(rootCommand);
            return details;
        }

        private static AutoCompleteInfo GetDetails(CommandLineApplication command)
        {
            AutoCompleteInfo info = new AutoCompleteInfo(command.Name);
            
            foreach (var commandOptionInfo in command.CustomArguments
                .OfType<CommandOptionInfo>())
            {
                string nameToAdd = string.IsNullOrEmpty(commandOptionInfo.LongName)
                    ? commandOptionInfo.ShortName
                    : commandOptionInfo.LongName;

                string hyphenPrefix = string.IsNullOrEmpty(commandOptionInfo.LongName)
                    ? "-"
                    : "--";
                
                info.OptionNames.Add(hyphenPrefix + nameToAdd);
            }

            command.Commands.ForEach(cmd =>
            {
                info.SubCommands.Add(GetDetails(cmd as CommandLineApplication));
            });

            return info;
        }
    }
}