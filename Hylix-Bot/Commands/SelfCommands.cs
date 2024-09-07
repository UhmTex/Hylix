using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Hylix_Bot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Hylix_Bot.Commands
{
    public class SelfCommandsHandler : ApplicationCommandModule
    {
        [SlashCommand("Profile", "Get your profile information")]
        public async Task GetProfile(InteractionContext ctx)
        {
            var dbHandler = new DBHandler();

            var userProfile = await dbHandler.GetUserProfileAsync(ctx.Member.Id);

            var speciesName = await dbHandler.GetSpeciesAsync(userProfile.speciesId);
            var affiliationName = await dbHandler.GetAffiliationAsync(userProfile.affiliationId);
            var className = await dbHandler.GetClassAsync(userProfile.classId);

            var profileEmbed = new DiscordEmbedBuilder()
            {
                Title=$"{ctx.Member.Username}'s Profile"
            };

            profileEmbed.AddField("Gold", userProfile.gold.ToString());
            profileEmbed.AddField("Species", speciesName);
            profileEmbed.AddField("Affiliation", affiliationName);
            profileEmbed.AddField("Class", className);

            await ctx.CreateResponseAsync(profileEmbed);
        }

        /*[Command("Setclass")]
        public async Task SetClassCommand(CommandContext ctx, int classId = -1)
        {
            var dbHandler = new DBHandler();

            if (classId == -1)
            {
                var _ClassDBList = await dbHandler.GetAllClassesAsync();

                var classList = new List<DiscordSelectComponentOption>();

                foreach (var _class in _ClassDBList)
                {
                    classList.Add(new DiscordSelectComponentOption($"{_class.Key}: {_class.Value}", _class.Key.ToString()));
                }

                var classListComponent = new DiscordSelectComponent("Classes", "Choose a class...", classList);

                var embedMessage = new DiscordEmbedBuilder()
                {
                    Title = "Class Selection",
                    Description = "Using the dropdown menu below, select your new class!"
                };

                var setClassMessage = new DiscordMessageBuilder().AddEmbed(embedMessage).AddComponents(classListComponent);

                await ctx.Channel.SendMessageAsync(setClassMessage);
            } 
            else
            {
                if (classId <= 0)
                {
                    await ctx.Channel.SendMessageAsync($"{ctx.Message.Author} You have entered and invalid class id! Please try again");
                }
                else
                {
                    User user = new User()
                    {
                        userName = ctx.Message.Author.Username,
                        userId = ctx.Message.Author.Id
                    };

                    await dbHandler.UpdateUserClassAsync(user, classId);
                }
            }
        }*/
    }
}
