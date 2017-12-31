using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandDotNet.Attributes;
using CommandDotNet.MicrosoftCommandLineUtils;

namespace CommandDotNet.Models
{
    public abstract class ArgumentInfo
    {
        protected readonly ParameterInfo ParameterInfo;
        protected readonly AppSettings Settings;

        protected ArgumentInfo(){}
        
        internal ArgumentInfo(AppSettings settings)
        {
            Settings = settings;
        }

        internal ArgumentInfo(ParameterInfo parameterInfo, AppSettings settings)
            : this(settings)
        {
            ParameterInfo = parameterInfo;
            Type = parameterInfo.ParameterType;
            DefaultValue = parameterInfo.DefaultValue;
            MultipleValues = GetIsMultipleType();
            ShowInHelpText = GetShowInHelpText();
        }

        
        public Type Type { get; internal set; }
        public object DefaultValue { get; internal set; }
        public string TypeDisplayName { get; internal set; }
        public string Details { get; internal set; }
        public string AnnotatedDescription { get; internal set; }
        public string Description { get; internal set; }
        public bool MultipleValues { get; }
        public bool ShowInHelpText { get; set; }
                
        internal List<string> Values { get; } = new List<string>();
        internal bool HasValue => Values.Any();
        internal string Value => Values.FirstOrDefault();

        private bool GetShowInHelpText()
        {
            bool attributeExists = ParameterInfo.HasAttribute(out ArgumentAttribute attribute);
            return !attributeExists || attribute.ShowInHelpText;
        }
        
        private bool GetIsMultipleType()
        {
            return Type != typeof(string) && Type.IsCollection();
        }

        internal void AddValue(string value)
        {
            Values.Add(value);
        }

        protected abstract string GetAnnotatedDescription();

        protected abstract string GetTypeDisplayName();
        
        protected abstract string GetDetails();
        
        protected string GetEffectiveDescription()
        {
            return Settings.ShowArgumentDetails
                ? $"{Details.PadRight(Constants.PadLength)}{AnnotatedDescription}"
                : AnnotatedDescription;
        }
    }
}