using System;
using CommandDotNet.Models;

namespace CommandDotNet.Example
{
    class Program
    {
        static int Main(string[] args)
        {
            AppRunner<Git> appRunner = new AppRunner<Git>();
            return appRunner.Run(args);
        }
    }
}