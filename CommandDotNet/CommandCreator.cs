using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandDotNet.Attributes;
using CommandDotNet.Exceptions;
using CommandDotNet.Extensions;
using CommandDotNet.MicrosoftCommandLineUtils;
using CommandDotNet.Models;

namespace CommandDotNet
{
    internal class CommandCreator
    {
        private readonly Type _type;
        private readonly CommandLineApplication _app;
        private readonly AppSettings _settings;
        private readonly CommandRunner _commandRunner;

        public CommandCreator(Type type, CommandLineApplication app, IDependencyResolver dependencyResolver, AppSettings settings)
        {
            _type = type;
            _app = app;
            _settings = settings;
            
            //get values for constructor params
            IEnumerable<ArgumentInfo> constructorValues = GetOptionValuesForConstructor();
            
            _commandRunner = new CommandRunner(app, type, constructorValues, dependencyResolver, settings);
        }

        public void CreateDefaultCommand()
        {
            CommandInfo defaultCommandInfo = _type.GetDefaultCommandInfo(_settings);
            
            _app.OnExecute(async () =>
            {
                if (defaultCommandInfo != null)
                {
                    if (defaultCommandInfo.Arguments.Any())
                    {
                        throw new AppRunnerException("Method with [DefaultMethod] attribute does not support parameters");
                    }

                    return await _commandRunner.RunCommand(defaultCommandInfo, null);
                }

                _app.ShowHelp();
                return 0;
            });
        }

        public void CreateCommands()
        {            
            foreach (CommandInfo commandInfo in _type.GetCommandInfos(_settings))
            {
                List<ArgumentInfo> argumentValues = new List<ArgumentInfo>();

                CommandLineApplication command = _app.Command(commandInfo);

                foreach (ArgumentInfo argument in commandInfo.Arguments)
                {
                    argumentValues.Add(argument);
                    switch (argument)
                    {
                        case CommandOptionInfo option:
                            SetValueForOption(option, command);
                            break;
                        case CommandParameterInfo parameter:
                            SetValueForParameter(parameter, command);
                            break;
                    }
                }

                command.OnExecute(async () => await _commandRunner.RunCommand(commandInfo, argumentValues));
            }
        }

        private static void SetValueForParameter(CommandParameterInfo parameter, CommandLineApplication command)
        {
            CommandArgument argument = command.Argument(parameter);
            parameter.SetValue(argument);
        }

        private static void SetValueForOption(CommandOptionInfo option, CommandLineApplication command)
        {
            option.SetValue(command.Option(option));
        }

        private IEnumerable<ArgumentInfo> GetOptionValuesForConstructor()
        {
            IEnumerable<ParameterInfo> parameterInfos = _type
                .GetConstructors()
                .FirstOrDefault()
                ?.GetParameters();

            if(parameterInfos != null && parameterInfos.Any(p => p.HasAttribute<ArgumentAttribute>()))
                throw new AppRunnerException("Constructor arguments can not have [Argument] attribute. Please use [Option] attribute");

            ArgumentInfoCreator argumentInfoCreator = new ArgumentInfoCreator(_settings);
            
            List<ArgumentInfo> argumentInfos = new List<ArgumentInfo>();

            foreach (var parameterInfo in parameterInfos)
            {
                argumentInfos.AddRange(argumentInfoCreator.ConvertToArgumentInfos(parameterInfo, ArgumentMode.Option));
            }

            foreach (ArgumentInfo argumentInfo in argumentInfos)
            {
                var optionInfo = (CommandOptionInfo) argumentInfo;
                optionInfo.SetValue(_app.Option(optionInfo));
            }
            
            return argumentInfos;
        }
    }
}