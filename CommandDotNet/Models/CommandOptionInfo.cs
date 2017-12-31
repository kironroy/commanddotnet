using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandDotNet.Attributes;
using CommandDotNet.Exceptions;
using CommandDotNet.MicrosoftCommandLineUtils;

namespace CommandDotNet.Models
{
    public class CommandOptionInfo : ArgumentInfo
    {   
        public CommandOptionInfo(AppSettings settings) : base(settings)
        {
        }

        public CommandOptionInfo(ParameterInfo parameterInfo, AppSettings settings) : base(parameterInfo, settings)
        {
            BooleanMode = GetBooleanMode();
            CommandOptionType = GetCommandOptionType();
            Template = GetTemplate();
            
            TypeDisplayName = GetTypeDisplayName();
            AnnotatedDescription = GetAnnotatedDescription();
            Details = GetDetails();
            Description = GetEffectiveDescription();
            
            //intennal -------------
            Inherited = false;
            SetInternals();
            //----------------------
        }

        private CommandOptionInfo(string description, CommandOptionType optionType)
        {
            Description = description;
            CommandOptionType = optionType;
        }
        
        public CommandOptionType CommandOptionType { get; set; }
        
        public string Template { get; set; }
        
        public BooleanMode BooleanMode { get; set; }
        
        //intennal -------------
        internal bool Inherited { get; }
        
        internal string LongName { get; set; }
        
        internal string SymbolName { get; set; }
        
        internal string ShortName { get; set; }
        
        internal string ValueName { get; set; }
        // -------------


        internal static CommandOptionInfo HelpOption()
        {
            return new CommandOptionInfo(
                Constants.HelpDescription, 
                CommandOptionType.NoValue)
            {
                LongName = "help",
                ShortName = "h"
            };
        }
        
        private void SetInternals()
        {
            foreach (var part in Template.Split(new[] { ' ', '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (part.StartsWith("--"))
                {
                    LongName = part.Substring(2);
                }
                else if (part.StartsWith("-"))
                {
                    var optName = part.Substring(1);

                    // If there is only one char and it is not an English letter, it is a symbol option (e.g. "-?")
                    if (optName.Length == 1 && !IsEnglishLetter(optName[0]))
                    {
                        SymbolName = optName;
                    }
                    else
                    {
                        ShortName = optName;
                    }
                }
                else if (part.StartsWith("<") && part.EndsWith(">"))
                {
                    ValueName = part.Substring(1, part.Length - 2);
                }
                else
                {
                    throw new ArgumentException($"Invalid template pattern '{Template}'", nameof(Template));
                }
            }

            if (string.IsNullOrEmpty(LongName) && string.IsNullOrEmpty(ShortName) && string.IsNullOrEmpty(SymbolName))
            {
                throw new ArgumentException($"Invalid template pattern '{Template}'", nameof(Template));
            }
        }
        
        private bool IsEnglishLetter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }
        
        public bool TryParse(string value)
        {
            switch (CommandOptionType)
            {
                case CommandOptionType.MultipleValue:
                    Values.Add(value);
                    break;
                case CommandOptionType.SingleValue:
                    if (Values.Any())
                    {
                        return false;
                    }
                    Values.Add(value);
                    break;
                case CommandOptionType.NoValue:
                    if (value != null)
                    {
                        return false;
                    }
                    // Add a value to indicate that this option was specified
                    Values.Add("on");
                    break;
                default:
                    break;
            }
            return true;
        }
        
        private BooleanMode GetBooleanMode()
        {
            OptionAttribute attribute = ParameterInfo.GetCustomAttribute<OptionAttribute>();

            if (attribute == null || attribute.BooleanMode == BooleanMode.Unknown)
                return Settings.BooleanMode;

            if (Type == typeof(bool) || Type == typeof(bool?))
            {
                return attribute.BooleanMode;
            }

            throw new AppRunnerException(
                $"BooleanMode property is set to `{attribute.BooleanMode}` for a non boolean parameter type. " +
                $"Property name: {ParameterInfo.Name} " +
                $"Type : {Type.Name}");
        }
        
        private string GetTemplate()
        {
            OptionAttribute attribute = ParameterInfo.GetCustomAttribute<OptionAttribute>(false);

            StringBuilder sb = new StringBuilder();
            
            bool longNameAdded = false;
            bool shortNameAdded = false;
            
            if (!string.IsNullOrWhiteSpace(attribute?.LongName))
            {
                sb.Append($"--{attribute?.LongName}");
                longNameAdded = true;
            }

            if (!string.IsNullOrWhiteSpace(attribute?.ShortName))
            {
                if (longNameAdded)
                {
                    sb.Append(" | ");
                }

                sb.Append($"-{attribute?.ShortName}");
                shortNameAdded = true;
            }

            string defaultTemplate = ParameterInfo.Name.Length == 1 ? $"-{ParameterInfo.Name}" : $"--{ParameterInfo.Name}";
            if(!longNameAdded & !shortNameAdded) sb.Append(defaultTemplate);
            
            //todo: value names
            //if(CommandOptionType != CommandOptionType.NoValue) sb.Append($" <{attribute?.LongName ?? Name}>");
            
            return sb.ToString();
        }
        
        private CommandOptionType GetCommandOptionType()
        {
            if (typeof(IEnumerable).IsAssignableFrom(Type) && Type != typeof(string))
            {
                return CommandOptionType.MultipleValue;
            }

            if ((Type == typeof(bool) || Type == typeof(bool?)) && BooleanMode == BooleanMode.Implicit)
            {
                return CommandOptionType.NoValue;
            }
            
            return CommandOptionType.SingleValue;
        }

        protected override string GetDetails()
        {
            return
                $"{GetTypeDisplayName()}{(DefaultValue != DBNull.Value ? " | Default value: " + DefaultValue : null)}";
        }

        protected override string GetTypeDisplayName()
        {
            if (Type.Name == "String") return Type.Name;

            if (BooleanMode == BooleanMode.Implicit && (Type == typeof(bool) || Type == typeof(bool?)))
            {
                return "Flag";
            }

            if (typeof(IEnumerable).IsAssignableFrom(Type))
            {
                return $"{Type.GetGenericArguments().FirstOrDefault()?.Name} (Multiple)";
            }

            return Nullable.GetUnderlyingType(Type)?.Name ?? Type.Name;
        }
        
        protected override string GetAnnotatedDescription()
        {
            OptionAttribute descriptionAttribute = ParameterInfo.GetCustomAttribute<OptionAttribute>();
            return descriptionAttribute?.Description;
        }
        
        public override string ToString()
        {
            return $"{ParameterInfo.Name} | '{Value ?? "null"}' | {Details} | {Template}";
        }
    }
}