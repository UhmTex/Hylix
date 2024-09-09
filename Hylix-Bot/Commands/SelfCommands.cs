using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Hylix_Bot.Database;
using Microsoft.IdentityModel.Tokens;
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
        [SlashCommand("Profile", "Get your full profile information")]
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

        [SlashCommand("Inventory", "Gets your inventory")]
        public async Task GetInventory(InteractionContext ctx)
        {
            var dbHandler = new DBHandler();

            var inventoryResult = await dbHandler.GetInventoryAsync(ctx.Member.Id);

            bool inventoryValidity = inventoryResult.Item1;

            List<InventoryItem> inventoryList = inventoryResult.Item2;

            string text = "";

            if (!inventoryValidity)
            {
                await ctx.CreateResponseAsync("something went wrong");
                return;
            }

            if (inventoryList.Count == 0)
            {
                await ctx.CreateResponseAsync("No items in inventory");
                return;
            }      

            foreach (var item in inventoryList)
            {
                text += $"{inventoryList.IndexOf(item) + 1} - {item.Name} - {item.Quantity}x\n";
            }

            await ctx.CreateResponseAsync(text);
        }

        [SlashCommand("Sell", "Sell items for gold")]
        public async Task Sell(InteractionContext ctx, 
        [Option("name", "The name of the item in your inventory (case insensitive)")] string name,
        [Option("amount", "The amount you wish to sell")] long amount)
        {
            
            if (amount <= 0)
            {
                await ctx.CreateResponseAsync("You have entered an invalid amount", true);
                return;
            }

            var dbHandler = new DBHandler();

            var inventoryList = new List<InventoryItem>();

            inventoryList = (await dbHandler.GetInventoryAsync(ctx.Member.Id)).Item2;

            var filteredList = inventoryList.Where(x => x.Name.ToLower() == name.ToLower()).ToList();

            if (!filteredList.IsNullOrEmpty())
            {
                var itemToSell = filteredList[0];

                if (itemToSell.Quantity < amount)
                {
                    await ctx.CreateResponseAsync("You can't sell more than you have!", true);
                    return;
                }

                var verifySell = await dbHandler.UpdateUserGold(ctx.Member.Id, amount * itemToSell.Market_Value);
                
                await dbHandler.UpdateInventoryAsync(ctx.Member.Id, itemToSell.Id, (int)amount*(-1));

                await ctx.CreateResponseAsync(filteredList[0].Name);
            }
            else 
            {
                await ctx.CreateResponseAsync("The item was not found in your inventory, try again", true);
            }
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
