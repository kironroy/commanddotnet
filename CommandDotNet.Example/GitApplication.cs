using System;
using System.Runtime.InteropServices;
using CommandDotNet.Attributes;

namespace CommandDotNet.Example
{
    [ApplicationMetadata(Description = "Fake git application", Name = "git")]
    public class GitApplication
    {
        [SubCommand]
        public Submodule SubmoduleProperty { get; set; }

        [SubCommand]
        public class Remote
        {
            [SubCommand]
            public class Origin
            {
                private readonly string _name;

                public Origin(string name)
                {
                    _name = name;
                }
                
                public void Show()
                {
                    Console.WriteLine("remote origin: " +
                                      (string.IsNullOrEmpty(_name) ? "master" : _name));
                }
            }
        }
        
        [ApplicationMetadata(Description = "Stashes all changes when executed without any arguments. " +
                                           "See stash --help for further information",
            Name = "stash")]
        [SubCommand]
        public class Stash
        {
            private readonly string _name;

            public Stash([Option(ShortName = "n",LongName = "name")]string name)
            {
                _name = name;
            }
            
            [DefaultMethod]
            public void DoStash()
            {
                if(string.IsNullOrEmpty(_name))
                    Console.WriteLine($"changes stashed");
                else
                {
                    Console.WriteLine($"changes stashed with name '{_name}'");
                }
            }
        
            [ApplicationMetadata(Name = "pop", Description = "Applies last stashed changes")]
            public void Pop()
            {
                Console.WriteLine($"stash popped");
            }

            [ApplicationMetadata(Name = "list", Description = "Lists all saved stashed changes")]
            public void List()
            {
                Console.WriteLine($"here's the list of stash");
            }
        }
        
        [ApplicationMetadata(Name = "commit", Description = "Commits all staged changes")]
        public void Commit([Option(ShortName = "m")]string commitMessage)
        {
            Console.WriteLine("Commit successful");
        }
    }
    
    public class Submodule
    {     
        public void Add(string name)
        {
            Console.WriteLine($"submodule added: {name}");
        }

        public void Remove(string name)
        {
            Console.WriteLine($"submodule: {name} has been removed");
        }
    }
}