using System.Collections.Generic;
using System.Linq;
using CommandDotNet.MicrosoftCommandLineUtils;

namespace CommandDotNet.Models
{
    internal class ValueInfo
    {
        private readonly IParameter _parameter;

        public ValueInfo(IParameter parameter)
        {
            _parameter = parameter;
        }

        internal bool HasValue => _parameter.HasValue();

        internal List<string> Values
        {
            get => _parameter.Values;
            set => _parameter.Values = value;
        }

        internal string Value => _parameter.Value(); 

        public override string ToString()
        {
            return string.Join(", ", _parameter.Values);
        }
    }
}