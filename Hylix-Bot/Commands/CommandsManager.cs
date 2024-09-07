using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Hylix_Bot.Database;
using System;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Hylix_Bot.Commands
{
    public class CommandsManager : BaseCommandModule
    {
        [Command("allspecies")]
        public async Task GetAllSpecies(CommandContext ctx)
        {
            var dbHandler = new DBHandler();

            var speciesList = await dbHandler.GetAllSpeciesAsync();

            speciesList.Sort((x, y) => x.Key.CompareTo(y.Key));

            var embedString = "";

            foreach (var species in speciesList)
            {
                embedString += $"{species.Key} - {species.Value}\n";
            }

            var embed = new DiscordEmbedBuilder()
            {
                Title = "All species Ids",
                Description = embedString,
            };

            await ctx.Channel.SendMessageAsync(embed: embed);
        }

        [Command("allclasses")]
        public async Task GetAllClasses(CommandContext ctx)
        {
            var dbHandler = new DBHandler();

            var classessList = await dbHandler.GetAllClassesAsync();

            classessList.Sort((x, y) => x.Key.CompareTo(y.Key));

            var embedString = "";

            foreach (var _class in classessList)
            {
                embedString += $"{_class.Key} - {_class.Value}\n";
            }

            var embed = new DiscordEmbedBuilder()
            {
                Title = "All class Ids",
                Description = embedString,
            };

            await ctx.Channel.SendMessageAsync(embed: embed);
        }

        /*[Command("allelements")]
        public async Task GetAllElements(CommandContext ctx)
        {
            var dbHandler = new DBHandler();

            var elementsList = await dbHandler.GetAllElementsAsync();

            elementsList.Sort((x, y) => x.Key.CompareTo(y.Key));

            var embedString = "";

            foreach (var ele in elementsList)
            {
                embedString += $"{ele.Key} - {ele.Value}\n";
            }

            var embed = new DiscordEmbedBuilder()
            {
                Title = "All element Ids",
                Description = embedString,
            };

            await ctx.Channel.SendMessageAsync(embed: embed);
        }    */    
    }

    public class SlashCommandsManager : ApplicationCommandModule
    {
        [SlashCommand("Info", "Displays all relevant information about Hylix")]        
        public async Task GetInfo(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
            {
                Title = $"<:Hylix:1281589634740129792> **__Hylix Bot__**",
                Description = @"Hylix is an apsiring full RPG mini-game bot set in the fantasy realm of **Hylixia**",
                Color = Global.colorlessEmbed
            };

            embed.AddField("**__Hylixia__**", "Hylixia is a fantasy realm rich with excitement, controlled by the 5 primordial gods of creation. Brimming with an extraordinary abundance of magic and treasure, adventurers set out to conquer dungeons, fight dragons and become the strongest they possibly can be!");

            await ctx.CreateResponseAsync(embed);
        }

        [SlashCommand("pInfo", "Displays all relevant information about Hylix")]
        [SlashRequireOwner]
        public async Task pGetInfo(InteractionContext ctx)
        {
            var firstInfo = new DiscordEmbedBuilder()
            {
                Title = $"<:Hylix:1281589634740129792> **__Hylix Bot__**",
                Description = @"Hylix is an apsiring full RPG mini-game bot set in the fantasy realm of **Hylixia**",
                Color = Global.colorlessEmbed
            };

            firstInfo.AddField("**__Hylixia__**", "Hylixia is a fantasy realm rich with excitement, controlled by the 5 primordial gods of creation. Brimming with an extraordinary abundance of magic and treasure, adventurers set out to conquer dungeons, fight dragons and become the strongest they possibly can be!\n\nHylix is aspiring to be a continuous, ever-developed, and user market governed RPG mini-game bot on a global scope that will consist of many popular mechanics shown in the largest of RPG games. From crafting of completely unique items like weapons and armors no-one has had before, to fighting hundreds of different unique legendary monsters from an ever growing list of species, classes, affiliations, materials and elements. The sky is the limit for the aspiring adventurers that will dive into the world of Hylix.");
            firstInfo.AddField("**__Development Information__**", "Hylix is actively developed by <@198177090921562112>\nThe bot may be offline often, it means it is under active maintenance or in deployment.");

            //DiscordMessage msg = ctx.Guild.m

            var secondInfo = new DiscordEmbedBuilder()
            {
                Title = $"**__Features__**",
                Description = "This is the list of all currently active/in-development features",
                Color = Global.colorlessEmbed
            };

            secondInfo.AddField("Self Information", "Using /profile will grant you access to view your current profile and information");
            secondInfo.AddField("ID Information", "using the commands /allclasses,/allspecies,/allaffiliations you can see all current available objects and their IDs");
            secondInfo.AddField("Monster Spawns", "Monsters will spawn as you chat, or when you use the command /adventure(IN-DEVELOPMENT) and /fight [Fight ID](IN-DEVELOPMENT). Doing so will allow you to fight them for loot, treasures, and glory");

            await ctx.Channel.SendMessageAsync(firstInfo);
            await ctx.Channel.SendMessageAsync(secondInfo);
        }



        [SlashCommand("Adventure", "Goes on an adventure!")]
        [SlashCooldown(1, 60, SlashCooldownBucketType.User)]
        public async Task StartAdventure(InteractionContext ctx)
        {
            var loadEmbed1 = new DiscordEmbedBuilder()
            {
                Title = "Out on an adventure"
            };

            await ctx.CreateResponseAsync(loadEmbed1);
        }

        [SlashCommand("Spawn", "Adventure spawn cmd")]
        [SlashRequireOwner]
        public async Task testMon(InteractionContext ctx)
        {

            var embedStart = new DiscordEmbedBuilder()
            {
                Title = "A monster is being spawned...",
                Color = Global.colorlessEmbed,
            };

            await ctx.CreateResponseAsync(embedStart);

            var dbHandler = new DBHandler();

            var monsterData = await dbHandler.GetRandomMonster();

            var spawn_data = await dbHandler.StoreGuildMonsterAsync(monsterData.Id, ctx.Guild.Id);

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

            await ctx.EditResponseAsync(fullResponse);            

            await Task.Delay(50000);

            embed.Title = $"A **{monsterData.Name}** has appeared! (Status: <:Invalid:1281738680469422081>)";
            embed.Footer.Text = $"ID: {spawn_data.Item2} • Despawned";

            await dbHandler.DeleteGuildMonsterAsync(spawn_data.Item2);

            var escapedResponse = new DiscordWebhookBuilder().WithContent("").AddEmbed(embed);

            await ctx.EditResponseAsync(escapedResponse);
        }

        [SlashCommand("Fight", "Fight against a monster!")]
        public async Task Fight(InteractionContext ctx, [Option("Id", "The fight Id to enage in")] long id)
        {
            await ctx.CreateResponseAsync("test");
        }
        

        [SlashCommand("allspecies", "Gets the list of all species and their IDs")]
        public async Task GetAllSpecies(InteractionContext ctx)
        {
            var dbHandler = new DBHandler();

            var speciesList = await dbHandler.GetAllSpeciesAsync();

            speciesList.Sort((x, y) => x.Key.CompareTo(y.Key));

            var embedString = "";

            foreach (var species in speciesList)
            {
                embedString += $"{species.Key} - {species.Value}\n";
            }

            var embed = new DiscordEmbedBuilder()
            {
                Title = "Species",
                Description = $"**__All Species and their IDs__**\n\n{embedString}",
                Color = Global.colorlessEmbed,
            };

            await ctx.CreateResponseAsync(embed: embed);
        }

        [SlashCommand("allclasses", "Gets a list of all classes and their IDs")]
        public async Task GetAllClasses(InteractionContext ctx)
        {
            var dbHandler = new DBHandler();

            var classessList = await dbHandler.GetAllClassesAsync();

            classessList.Sort((x, y) => x.Key.CompareTo(y.Key));

            var embedString = "";

            foreach (var _class in classessList)
            {
                embedString += $"{_class.Key} - {_class.Value}\n";
            }

            var embed = new DiscordEmbedBuilder()
            {
                Title = "Classes",
                Description = $"**__All Classes and their IDs__**\n\n{embedString}",
                Color = Global.colorlessEmbed,
            };

            await ctx.CreateResponseAsync(embed: embed);
        }

        [SlashCommand("allelements", "Gets a list of all elements and their IDs")]
        public async Task GetAllElements(InteractionContext ctx)
        {
            var dbHandler = new DBHandler();

            var elementsList = await dbHandler.GetAllElementsAsync();

            elementsList.Sort((x, y) => x.Id.CompareTo(y.Id));

            var embedString = "";

            foreach (var ele in elementsList)
            {
                if (ele.Emoji_Id == 0)
                {
                    embedString += $"{ele.Id} • {ele.Name}\n";
                }
                else
                {
                    embedString += $"{ele.Id} • {ele.Name} • <:{ele.Name}:{ele.Emoji_Id}>\n";
                }
            }         

            var embed = new DiscordEmbedBuilder()
            {
                Title = $"{Global.Element_Emoji} Elements",
                Description = $"**__All Elements and their IDs__**\n\n{embedString}",
                Color = Global.colorlessEmbed,
            };

            await ctx.CreateResponseAsync(embed: embed);
        }
    }
}
