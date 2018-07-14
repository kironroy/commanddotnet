using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CommandDotNet.Attributes;
using CommandDotNet.Extensions;
using CommandDotNet.MicrosoftCommandLineUtils;
using CommandDotNet.Models;

namespace CommandDotNet
{
    internal class AppCreator
    {
        private readonly AppSettings _appSettings;

        public AppCreator(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public CommandLineApplication CreateApplication(
            Type type,
            IDependencyResolver dependencyResolver,
            CommandLineApplication parentApplication = null)
        {
            bool isRootApp = parentApplication == null;

            CommandLineApplication app;

            ApplicationMetadataAttribute consoleApplicationAttribute = type.GetCustomAttribute<ApplicationMetadataAttribute>(false);

            if (isRootApp)
            {
                string assemblyName = $"{Assembly.GetEntryAssembly().GetName().Name}.dll";
                string defaultRootCommand = $"dotnet {assemblyName}";
                string rootCommandName = consoleApplicationAttribute?.Name ?? defaultRootCommand;
                
                app = new CommandLineApplication(_appSettings)
                {
                    Name = rootCommandName,
                    FullName = consoleApplicationAttribute?.Name ?? assemblyName,
                    Description = consoleApplicationAttribute?.Description,
                    ExtendedHelpText = consoleApplicationAttribute?.ExtendedHelpText,
                    Syntax = consoleApplicationAttribute?.Syntax
                };
                
                app.HelpOption(Constants.HelpTemplate);
                
                AddVersion(app);
            }
            else
            {
                var commandInfo = new CommandInfo(type, _appSettings);
                
                app = parentApplication.Command(commandInfo);
            }

            CommandCreator commandCreator = new CommandCreator(type, app, dependencyResolver, _appSettings);
            
            commandCreator.CreateDefaultCommand();

            commandCreator.CreateCommands();

            CreateSubApplications(type, app, dependencyResolver);

            return app;
        }

        private void AddVersion(CommandLineApplication app)
        {
            if (_appSettings.EnableVersionOption)
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                app.VersionOption("-v | --version", shortFormVersion: fvi.ProductVersion);
            }
        }
        
        private void CreateSubApplications(Type type,
            CommandLineApplication parentApplication,
            IDependencyResolver dependencyResolver)
        {
            IEnumerable<Type> propertySubmodules = 
                type.GetDeclaredProperties<SubCommandAttribute>()
                .Select(p => p.PropertyType);
            
            IEnumerable<Type> inlineClassSubmodules = type
                .GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(x=> x.HasAttribute<SubCommandAttribute>())
                .Where(x=> !x.IsCompilerGenerated())
                .Where(x=> !typeof(IAsyncStateMachine).IsAssignableFrom(x));

            var submoduleTypes = propertySubmodules.Union(inlineClassSubmodules);
            
            foreach (Type submoduleType in submoduleTypes)
            {
                AppCreator appCreator = new AppCreator(_appSettings);
                appCreator.CreateApplication(submoduleType, dependencyResolver, parentApplication);
            }
        }
    }
}