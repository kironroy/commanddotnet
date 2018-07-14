using System;
using CommandDotNet.Attributes;

namespace CommandDotNet.AutoCompletion
{
    public class AutoCompleteGenerationApp<T> where T:class
    {        
        [InjectProperty]
        public AppRunner<T> TargetAppRunner { get; set; }
        
        public void GenerateScripts(string outputFilePath = "./auto-complete-scripts.sh")
        {
            AutoCompleteInfo info = AutoCompleteInfo.Generate(TargetAppRunner);

            Console.WriteLine(outputFilePath);

            Console.WriteLine("--------------");

            Console.WriteLine(info.ToString());
            
            Console.WriteLine("--------------");

            Console.WriteLine("yey!");
        }
    }
}