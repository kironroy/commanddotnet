using CommandDotNet.Attributes;

namespace CommandDotNet.Example
{
    public class Git
    {
        public void commit([Option(ShortName = "m")] string message,
            [Option(ShortName = "a", LongName = "internactive")] bool interactive,
            [Option] bool amend) { }

        public void clone(string url) { }

        public void push(
            [Option(ShortName = "f", LongName = "force")]
            bool force,
            [Option] bool tags,
            string repository){ }
        
        public void status(){ }
        
        public void checkout([Option]bool q, [Option] bool f, [Option] bool m, string branch) { }
        
        public class stash
        {
            [DefaultMethod]
            public void stashChanges() { }

            public void pop() { }
            
            public void list() { }
        }
    }
}