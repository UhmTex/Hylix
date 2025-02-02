﻿using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Hylix_Bot.Database;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

using static System.Console;

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
            var alignmentName = await dbHandler.GetAlignmentAsync(userProfile.alignmentId);
            var className = await dbHandler.GetClassAsync(userProfile.classId);

            var profileEmbed = new DiscordEmbedBuilder()
            {
                Title=$"Profile",
                Color = Global.colorlessEmbed
            };           

            profileEmbed.WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl);

            profileEmbed.AddField("Gold", userProfile.gold.ToString());
            profileEmbed.AddField("Species", speciesName, true);
            profileEmbed.AddField("Alignment", alignmentName, true);
            profileEmbed.AddField("Class", className, true );

            await ctx.CreateResponseAsync(profileEmbed);
        }

        [SlashCommand("imgtest", "nothing")]
        public async Task ImgTest(InteractionContext ctx)
        {
            var url = SupabaseHandler.supabase.Storage.From("images").GetPublicUrl("TestImage.png");

            WriteLine("test1");
            var res = await SupabaseHandler.supabase.From<userTest>().Get();
            WriteLine("test2");
            List<userTest> check = res.Models;

            var text = string.Empty;

            foreach (var item in check)
            {
                text += $"{item.Id} - {item.Name}";
            }

            WriteLine($"test {check} {check.Count}");
            WriteLine(check.Count);

            await ctx.CreateResponseAsync(text);

            //await ctx.CreateResponseAsync(url);
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

            inventoryList.Sort((x, y) => x.Name.CompareTo(y.Name));

            foreach (var item in inventoryList)
            {
                var noEmoji = string.Empty;
                var hasEmoji = $"<:{item.Name.Replace(" ","_")}:{item.Emoji_Id}> ";

                var emojiIcon = item.Emoji_Id==0 ? noEmoji : hasEmoji;

                text += $"{emojiIcon}{item.Name} • {item.Quantity}x\n";
            }

            if (inventoryList.Count == 0)
            {
                text = "No items in inventory";
            }            

            //{inventoryList.IndexOf(item) + 1}. 

            var embed = new DiscordEmbedBuilder()
            {
                Title = $"Inventory",
                Description = text,
                Footer = new DiscordEmbedBuilder.EmbedFooter(),
                Color = Global.colorlessEmbed
            };

            embed.Footer.Text = "Sorted by alphabetical order (A-Z)";

            embed.WithAuthor(ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl);

            await ctx.CreateResponseAsync(embed);
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
                    await ctx.CreateResponseAsync("You can't sell more than you own!", true);
                    return;
                }

                var sellAmount = amount * itemToSell.Market_Value;

                var verifySell = await dbHandler.UpdateUserGold(ctx.Member.Id, sellAmount);
                
                await dbHandler.UpdateInventoryAsync(ctx.Member.Id, itemToSell.Id, (int)amount*(-1));

                await ctx.CreateResponseAsync($"You have successfully sold {itemToSell.Name} for {sellAmount} gold!");
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
