using System;

namespace CommandDotNet.Example
{
    class Program
    {
        static int Main(string[] args)
        {
            AppRunner<MyApplication> appRunner = new AppRunner<MyApplication>();

            appRunner.OnBeforeCommandExecute += (sender, eventArgs) =>
            {
                var a = eventArgs.CommandInfo;
                var b = eventArgs.ParameterValues;
                
                Console.WriteLine("OnBeforeCommandExecute-----");
            };
            
            return appRunner.Run(args);
        }
    }
}