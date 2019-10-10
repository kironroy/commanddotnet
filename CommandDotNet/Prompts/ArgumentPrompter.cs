﻿using System;
using System.Collections.Generic;
using CommandDotNet.Extensions;

namespace CommandDotNet.Prompts
{
    public class ArgumentPrompter : IArgumentPrompter
    {
        private readonly Func<CommandContext, IArgument, string> _getPromptTextCallback;
        private readonly IPrompter _prompter;

        public ArgumentPrompter(
            IPrompter prompter,
            Func<CommandContext, IArgument, string> getPromptTextCallback = null)
        {
            _prompter = prompter;
            _getPromptTextCallback = getPromptTextCallback;
        }

        public virtual ICollection<string> PromptForArgumentValues(CommandContext commandContext, IArgument argument, out bool isCancellationRequested)
        {
            var argumentName = _getPromptTextCallback?.Invoke(commandContext, argument) ?? argument.Name;
            var promptText = $"{argumentName} ({argument.TypeInfo.DisplayName})";

            ICollection<string> inputs = new List<string>();

            if (argument.Arity.AllowsZeroOrMore())
            {
                if (_prompter.TryPromptForValues(
                    out var values, out isCancellationRequested, promptText))
                {
                    inputs.AddRange(values);
                }
            }
            else
            {
                if (_prompter.TryPromptForValue(
                    out var value, out isCancellationRequested, promptText, 
                    isPassword: argument.TypeInfo.UnderlyingType == typeof(Password)))
                {
                    inputs.Add(value);
                }
            }

            return inputs;
        }
    }
}