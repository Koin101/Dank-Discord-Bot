using System.Threading.Tasks;
using Discord.Interactions;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using JsonFlatFileDataStore;
using InteractionContext = Discord.Interactions.InteractionContext;

namespace Discord_Bot.Commands;

public class DankUserCommands(IDocumentCollection<DankUser> dankUserCollection) : ApplicationCommandModule
{
    public IDocumentCollection<DankUser> DankUserCollection = dankUserCollection;

    [DSharpPlus.SlashCommands.SlashCommand("addDankUser", "Adds a dank user to the DankUser DB"), 
     SlashRequireOwner]
    public async Task AddDankUser(InteractionContext ctx, 
        [Option("user", "User to add")] DiscordMember user)
    {
        DankUser dankUser = new DankUser(user.Id, user.Username, user.Nickname);

        await dankUserCollection.InsertOneAsync(dankUser);
    }
    
    
}