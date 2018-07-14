using System.Collections.Generic;

namespace CommandDotNet.MicrosoftCommandLineUtils
{
    public interface IParameter
    {
        List<string> Values { get; set; }
        string Value();
        bool HasValue();
    }
}