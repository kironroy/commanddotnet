using System;
using CommandDotNet.Models;

namespace CommandDotNet.AutoCompletion
{
    public static class AutoCompleteScripGenerator
    {
        public static int GenerateAutoCompleteScript<T>(this AppRunner<T> appRunner, string[] args) where T : class
        {
            AppRunner<AutoCompleteGenerationApp<T>> newRunner = new AppRunner<AutoCompleteGenerationApp<T>>(
                new AppSettings
                {
                    Case = Case.DontChange
                })
            .UseDependencyResolver(AppRunnerResolver.Make(appRunner));

            return newRunner.Run(args);
        }
    }

    internal class AppRunnerResolver : IDependencyResolver
    {
        private readonly object _appRunner;

        private AppRunnerResolver(object appRunner)
        {
            _appRunner = appRunner;
        }
        
        internal static AppRunnerResolver Make<T>(AppRunner<T> appRunner) where T : class
        {
            return new AppRunnerResolver(appRunner);
        }
        
        public object Resolve(Type type)
        {
            return _appRunner;
        }
    }
}