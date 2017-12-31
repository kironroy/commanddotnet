using System;
using System.Collections.Generic;
using System.Reflection;
using CommandDotNet.Attributes;

namespace CommandDotNet.Example
{
    public class DirUtilities
    {
        private readonly int _logLevel;
        private readonly string _password;

        public DirUtilities(int logLevel, string password)
        {
            _logLevel = logLevel;
            _password = password;
        }
        
        [ApplicationMetadata(Name = "mkdir", Description = "create one or more directories")]
        public void createDirectories(
            string username,
            [Argument(Description = "names of the directory")]List<string> directoryNames)
        {

            directoryNames = directoryNames ?? new List<string>();

            Console.WriteLine($"Username: {username}, password: {_password}, log level: {_logLevel}");
            
            Console.WriteLine($"initiating creation of {directoryNames.Count} directories");
            int index = 0;
            
            foreach (string directoryName in directoryNames)
            {
                Console.WriteLine($"{++index} -> directory '{directoryName}' created!");
            }

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"total {directoryNames.Count} directories created");
        }
    }
}
