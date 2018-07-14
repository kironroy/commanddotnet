using System.Collections.Generic;
using System.Linq;

namespace CommandDotNet.Models
{
    internal class ValueInfo
    {
        public ValueInfo(List<string> values)
        {
            Values = values;
        }
        
        internal bool HasValue => Values != null && Values.Any();

        internal List<string> Values { get; private set; }

        internal void Set(List<string> values)
        {
            Values = values;
        }
        
        internal void Set(string value)
        {
            Values = new List<string>(){value};
        }
        
        internal string Value => HasValue ? Values.First() : null;

        public override string ToString()
        {
            return string.Join(", ", Values ?? new List<string>());
        }
    }
}