using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Hylix_Bot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Hylix_Bot.Handler
{
    public class MessageHandler
    {
        public async Task MonsterSpawn(MessageCreateEventArgs args) 
        {
            var random = new Random();
            
            if (random.Next(101) < 5)
            {
                Console.WriteLine($"Spawned in {args.Guild.Name}");

                var dbHandler = new DBHandler();

                var monsterData = await dbHandler.GetRandomMonster();

                var spawn_data = await dbHandler.StoreGuildMonsterAsync(monsterData.Id, args.Guild.Id);

                var embed = new DiscordEmbedBuilder()
                {
                    Title = $"A **{monsterData.Name}** has appeared! (Status: <:Valid:1281738662186324000>)",
                    Color = Global.colorlessEmbed,
                    Footer = new DiscordEmbedBuilder.EmbedFooter()
                };

                embed.AddField("**__Monster Description__**", monsterData.Description);
                embed.AddField("**__Element__**", $"<:Element:{monsterData.Element_Emoji}> • {monsterData.Element}", true);
                embed.AddField("**__Species__**", $"{monsterData.Species}", true);
                embed.AddField("**__Affiliation__**", $"{monsterData.Affiliation}", true);
                embed.Footer.Text = $"ID: {spawn_data.Item2} • Use the command /fight [ID] to fight! • Despawns after 50 seconds!";

                var fullResponse = new DiscordWebhookBuilder().WithContent("").AddEmbed(embed);

                //await ctx.EditResponseAsync(fullResponse);
                var botMessage = await args.Channel.SendMessageAsync(embed);

                await Task.Delay(50000);

                embed.Title = $"A **{monsterData.Name}** has appeared! (Status: <:Invalid:1281738680469422081>)";
                embed.Footer.Text = $"ID: {spawn_data.Item2} • Despawned";

                await dbHandler.DeleteGuildMonsterAsync(spawn_data.Item2);

                var escapedResponse = new DiscordWebhookBuilder().WithContent("").AddEmbed(embed);

                await botMessage.ModifyAsync("", new Optional<DiscordEmbed>(embed));
            }
        }

        public async Task HandleNewUser(DiscordUser user)
        {
            var dbHandler = new DBHandler();

            var userData = new User()
            {
                userName = user.Username,
                userId = user.Id,
            };

            await dbHandler.StoreUserAsync(userData);
        }
    }
}
