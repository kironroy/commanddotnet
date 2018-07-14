using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using CommandDotNet.Attributes;

namespace CommandDotNet.Models
{
    internal enum CommandFrom
    {
        FromMethod,
        FromSubmoduleClass
    }

    public class CommandInfo
    {
        private readonly AppSettings _settings;
        private readonly ArgumentInfoCreator _argumentInfoCreator;
        private readonly CommandFrom _commandFrom;

        public CommandInfo(MethodInfo methodInfo, AppSettings settings)
        {
            _settings = settings;
            _argumentInfoCreator = new ArgumentInfoCreator(settings);
            _commandFrom = CommandFrom.FromMethod;

            var metadataAttribute = methodInfo.GetCustomAttribute<ApplicationMetadataAttribute>(false);

            Name = metadataAttribute?.Name ?? methodInfo.Name.ChangeCase(_settings.Case);
            MethodName = methodInfo.Name;
            Description = metadataAttribute?.Description;
            ExtendedHelpText = metadataAttribute?.ExtendedHelpText;
            Syntax = metadataAttribute?.Syntax;

            Arguments = GetArguments(methodInfo);
        }

        public CommandInfo(Type submoduleType, AppSettings settings)
        {
            _settings = settings;
            _argumentInfoCreator = new ArgumentInfoCreator(settings);
            _commandFrom = CommandFrom.FromSubmoduleClass;
            
            var metadataAttribute = submoduleType.GetCustomAttribute<ApplicationMetadataAttribute>(false);
            
            Name = metadataAttribute?.Name ?? submoduleType.Name.ChangeCase(_settings.Case);
            MethodName = null; // this is not a method. its a submodule class
            Description = metadataAttribute?.Description;
            ExtendedHelpText = metadataAttribute?.ExtendedHelpText;
            Syntax = metadataAttribute?.Syntax;
        }

        private IEnumerable<ArgumentInfo> GetArguments(MethodInfo methodInfo)
        {
            List<ArgumentInfo> arguments = new List<ArgumentInfo>();

            foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
            {
                arguments.AddRange(_argumentInfoCreator.ConvertToArgumentInfos(parameterInfo, _settings.MethodArgumentMode));
            }
            
            return arguments;
        }
        
        public string Name { get; }
        
        public string MethodName { get; }
        
        public string Description { get; }
        
        public string ExtendedHelpText { get; }
        
        public string Syntax { get; }

        public IEnumerable<ArgumentInfo> Arguments { get; } = new List<ArgumentInfo>();
    }
}