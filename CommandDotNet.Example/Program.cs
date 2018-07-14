using CommandDotNet.Models;

namespace CommandDotNet.Example
{
    class Program
    {
        static int Main(string[] args)
        {
            AppRunner<GitApplication> appRunner = new AppRunner<GitApplication>(new AppSettings()
            {
                Case = Case.KebabCase,
                HelpTextStyle = HelpTextStyle.Detailed
            });
            return appRunner.Run(args);
        }
    }
}