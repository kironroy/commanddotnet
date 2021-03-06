﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommandDotNet.Exceptions;
using CommandDotNet.MicrosoftCommandLineUtils;
using CommandDotNet.Models;

namespace CommandDotNet.Parsing
{
    internal partial class CommandParser
    {
        private readonly AppSettings _appSettings;
        private readonly ParserContext _parserContext;

        public CommandParser(AppSettings appSettings, ParserContext parserContext)
        {
            _appSettings = appSettings;
            _parserContext = parserContext;
        }

        public ParseResult ParseCommand(CommandLineApplication app, string[] args)
        {
            CommandLineApplication currentCommand = app;
            CommandOption currentOption = null;
            IEnumerator<CommandArgument> arguments = new CommandArgumentEnumerator(app.Arguments);

            var remainingArguments = new List<Token>();

            var tokens = args.Tokenize(includeDirectives: _appSettings.EnableDirectives);

            tokens = ApplyArgumentTransformations(tokens);

            if (_parserContext.ParseDirectiveEnabled)
            {
                return new ParseResult(app, args, tokens, exitCode: 0);
            }

            bool ignoreRemainingArguments = false;

            foreach (var token in tokens)
            {
                if (ignoreRemainingArguments)
                {
                    remainingArguments.Add(token);
                    continue;
                }

                switch (token.TokenType)
                {
                    case TokenType.Option:
                        var optionResult = ParseOption(token, currentCommand, out currentOption);
                        switch (optionResult)
                        {
                            case ParseOptionResult.Succeeded:
                                break;
                            case ParseOptionResult.UnexpectedArgument:
                                ignoreRemainingArguments = true;
                                break;
                            case ParseOptionResult.ShowHelp:
                                currentCommand.ShowHelp();
                                return new ParseResult(currentCommand, args, tokens, exitCode: 0);
                            case ParseOptionResult.ShowVersion:
                                app.ShowVersion();
                                return new ParseResult(currentCommand, args, tokens, exitCode: 0);
                            default:
                                throw new ArgumentOutOfRangeException(optionResult.ToString());
                        }
                        break;
                    case TokenType.Argument:
                        var argumentResult = ParseArgument(token, ref currentCommand, ref currentOption, arguments);
                        switch (argumentResult)
                        {
                            case ParseArgumentResult.Succeeded:
                                break;
                            case ParseArgumentResult.UnexpectedArgument:
                                ignoreRemainingArguments = true;
                                break;
                            case ParseArgumentResult.NewSubCommand:
                                arguments = new CommandArgumentEnumerator(currentCommand.Arguments);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(argumentResult.ToString());
                        }
                        break;
                    case TokenType.Separator:
                        ignoreRemainingArguments = true;
                        break;
                    case TokenType.Directive:
                        throw new ArgumentOutOfRangeException($"Directives should have already been processed and removed: {token.RawValue}");
                    default:
                        throw new ArgumentOutOfRangeException($"Unknown {nameof(TokenType)}: {token.TokenType}");
                }
            }

            if (currentOption != null) // an option was left without a value
            {
                throw new CommandParsingException(currentCommand, $"Missing value for option '{currentOption.LongName}'");
            }

            return new ParseResult(currentCommand, args, tokens, unparsedTokens: new Tokens(remainingArguments));
        }

        private enum ParseArgumentResult
        {
            Succeeded,
            UnexpectedArgument,
            NewSubCommand
        }

        private enum ParseOptionResult
        {
            Succeeded,
            UnexpectedArgument,
            ShowHelp,
            ShowVersion,
        }

        private ParseArgumentResult ParseArgument(
            Token token, 
            ref CommandLineApplication command,
            ref CommandOption option, 
            IEnumerator<CommandArgument> arguments)
        {
            if (option != null)
            {
                if (option.TryParse(token.Value))
                {
                    option = null;
                    return ParseArgumentResult.Succeeded;
                }

                throw new CommandParsingException(command, $"Unexpected value '{token.Value}' for option '{option.LongName}'");
            }

            var subCommand = command.Commands
                .Where(c => c.Name.Equals(token.Value, StringComparison.OrdinalIgnoreCase))
                .Cast<CommandLineApplication>()
                .FirstOrDefault();
            if (subCommand != null)
            {
                command = subCommand;
                option = null;
                return ParseArgumentResult.NewSubCommand;
            }

            if (arguments.MoveNext())
            {
                arguments.Current.Values.Add(token.Value);
            }
            else
            {
                if (_appSettings.ThrowOnUnexpectedArgument)
                {
                    throw new CommandParsingException(command, $"Unrecognized command or argument '{token.RawValue}'");
                }
                return ParseArgumentResult.UnexpectedArgument;
            }

            return ParseArgumentResult.Succeeded;
        }

        private ParseOptionResult ParseOption(Token token, CommandLineApplication command, out CommandOption option)
        {
            var optionTokenType = token.OptionTokenType;

            string optionName = optionTokenType.GetName();

            option = optionTokenType.IsLong 
                ? command.FindOption(o => o.LongName, optionName)
                : command.FindOption(o => o.ShortName, optionName)
                  ?? command.FindOption(o => o.SymbolName, optionName);

            if (option == null)
            {
                if (_appSettings.ThrowOnUnexpectedArgument)
                {
                    throw new CommandParsingException(command, $"Unrecognized option '{token.RawValue}'");
                }
                return ParseOptionResult.UnexpectedArgument;
            }

            if (ReferenceEquals(option, command.OptionHelp))
            {
                return ParseOptionResult.ShowHelp;
            }

            if (ReferenceEquals(option, command.OptionVersion))
            {
                return ParseOptionResult.ShowVersion;
            }

            if (optionTokenType.HasValue)
            {
                if (!option.TryParse(optionTokenType.GetAssignedValue()))
                {
                    throw new CommandParsingException(command, $"Unexpected value '{token.Value}' for option '{option.LongName}'");
                }

                option = null;
            }
            else if(option.OptionType == CommandOptionType.NoValue)
            {
                // No value is needed for this option
                option.TryParse(null);
                option = null;
            }

            return ParseOptionResult.Succeeded;
        }

        private Tokens ApplyArgumentTransformations(Tokens args)
        {
            if (_parserContext.ParseDirectiveEnabled)
            {
                _appSettings.Out.WriteLine("==> received");
                foreach (var arg in args)
                {
                    _appSettings.Out.WriteLine(arg.RawValue);
                }
                _appSettings.Out.WriteLine();
            }

            var transformations = _parserContext.ArgumentTransformations.OrderBy(t => t.Order).AsEnumerable();

            // append ExpandClubbedFlags to the end.
            // it's a feature we want to ensure is applied to all arguments
            // to prevent cases later where short clubbed options aren't found
            transformations = transformations.Union(
                new[]
            {
                new ArgumentTransformation(
                    "Expand clubbed flags",
                    int.MaxValue,
                    Tokenizer.ExpandClubbedOptions),
            });

            foreach (var transformation in transformations)
            {
                try
                {
                    var tempArgs = transformation.Transformation(args);

                    if (_parserContext.ParseDirectiveEnabled)
                    {
                        if (args.Count == tempArgs.Count &&
                            Enumerable.Range(0, args.Count).All(i => args[i] == tempArgs[i]))
                        {
                            _appSettings.Out.WriteLine($"==> transformation: {transformation.Name} (no changes)");
                        }
                        else
                        {
                            _appSettings.Out.WriteLine($"==> transformation: {transformation.Name}");
                            foreach (var arg in tempArgs)
                            {
                                _appSettings.Out.WriteLine(arg.RawValue);
                            }
                            _appSettings.Out.WriteLine();
                        }
                    }

                    args = tempArgs;
                }
                catch (Exception e)
                {
                    throw new AppRunnerException($"transformation failure for: {transformation}", e);
                }
            }

            return args;
        }

        private class CommandArgumentEnumerator : IEnumerator<CommandArgument>
        {
            private readonly IEnumerator<CommandArgument> _enumerator;

            public CommandArgumentEnumerator(IEnumerable<CommandArgument> enumerable)
            {
                _enumerator = enumerable.GetEnumerator();
            }

            public CommandArgument Current => _enumerator.Current;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            public bool MoveNext()
            {
                if (Current == null || !Current.MultipleValues)
                {
                    return _enumerator.MoveNext();
                }

                // If current argument allows multiple values, we don't move forward and
                // all later values will be added to current CommandArgument.Values
                return true;
            }

            public void Reset()
            {
                _enumerator.Reset();
            }
        }
    }
}