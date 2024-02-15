using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;

namespace Discord_Bot
{
    public class CustomHelpFormatter : BaseHelpFormatter
    {
        public DiscordEmbedBuilder EmbedBuilder { get; }

        private Command? Command { get; set; }

        //
        // Summary:
        //     Creates a new default help formatter.
        //
        // Parameters:
        //   ctx:
        //     Context in which this formatter is being invoked.
        public CustomHelpFormatter(CommandContext ctx)
            : base(ctx)
        {
            EmbedBuilder = new DiscordEmbedBuilder().WithTitle("Help").WithColor(32767);
        }

        //
        // Summary:
        //     Sets the command this help message will be for.
        //
        // Parameters:
        //   command:
        //     Command for which the help message is being produced.
        //
        // Returns:
        //     This help formatter.
        public override BaseHelpFormatter WithCommand(Command command)
        {
            Command = command;
            
            EmbedBuilder.WithDescription(Formatter.InlineCode(command.Name) + ": " + (command.Description ?? "this is a description"));
            CommandGroup commandGroup = command as CommandGroup;
            if ((object)commandGroup != null && commandGroup.IsExecutableWithoutSubcommands)
            {
                EmbedBuilder.WithDescription(EmbedBuilder.Description + "\n\nThis group can be executed as a standalone command.");
            }

            if (command.Aliases.Count > 0)
            {
                EmbedBuilder.AddField("Aliases", string.Join(", ", command.Aliases.Select(new Func<string, string>(Formatter.InlineCode))));
            }

            if (command.Overloads.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (CommandOverload item in command.Overloads.OrderByDescending((CommandOverload x) => x.Priority))
                {
                    stringBuilder.Append('`').Append(command.QualifiedName);
                    foreach (CommandArgument argument in item.Arguments)
                    {
                        stringBuilder.Append((argument.IsOptional || argument.IsCatchAll) ? " [" : " <").Append(argument.Name).Append(argument.IsCatchAll ? "..." : "")
                            .Append((argument.IsOptional || argument.IsCatchAll) ? ']' : '>');
                    }

                    stringBuilder.Append("`\n");
                    foreach (CommandArgument argument2 in item.Arguments)
                    {
                        stringBuilder.Append('`').Append(argument2.Name).Append(" (")
                            .Append(base.CommandsNext.GetUserFriendlyTypeName(argument2.Type))
                            .Append(")`: ")
                            .Append(argument2.Description ?? "No description provided.")
                            .Append('\n');
                    }

                    stringBuilder.Append('\n');
                }

                EmbedBuilder.AddField("Arguments", stringBuilder.ToString().Trim());
            }

            return this;
        }

        //
        // Summary:
        //     Sets the subcommands for this command, if applicable. This method will be called
        //     with filtered data.
        //
        // Parameters:
        //   subcommands:
        //     Subcommands for this command group.
        //
        // Returns:
        //     This help formatter.
        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            IOrderedEnumerable<IGrouping<string, Command>> orderedEnumerable = from xm in subcommands
                                                                               group xm by xm.Category into xm
                                                                               orderby xm.Key == null, xm.Key
                                                                               select xm;
            if (orderedEnumerable.Count() == 1 && orderedEnumerable.Single().Key == null)
            {
                EmbedBuilder.AddField(((object)Command != null) ? "Subcommands" : "Commands", string.Join(", ", subcommands.Select((Command x) => Formatter.InlineCode(x.Name))));
                return this;
            }

            foreach (IGrouping<string, Command> item in orderedEnumerable)
            {
                EmbedBuilder.AddField(item.Key ?? "Uncategorized commands", string.Join(", ", item.Select((Command xm) => Formatter.InlineCode(xm.Name))));
            }

            return this;
        }

        //
        // Summary:
        //     Construct the help message.
        //
        // Returns:
        //     Data for the help message.
        public override CommandHelpMessage Build()
        {
            if ((object)Command == null)
            {
                EmbedBuilder.WithDescription("Listing all top-level commands and groups. Specify a command to see more information.");
            }

            return new CommandHelpMessage(null, EmbedBuilder.Build());
        }
    }
}

