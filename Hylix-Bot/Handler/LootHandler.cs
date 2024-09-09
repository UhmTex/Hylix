using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;

using static System.Console;

namespace Hylix_Bot
{
    internal static class AdventureHandler
    {
        public static async Task<List<Drop>> GenerateDropsAsync(InteractionContext ctx, int dropCount)
        {
            try
            {
                var dbHandler = new DBHandler();

                List<Drop> generatedDrops = new List<Drop>();

                for (int i=0; i < dropCount; i++)
                {                
                    Drop drop = (await dbHandler.GenerateDropAsync()).Item2;
                    generatedDrops.Add(drop);
                }

                foreach (var drop in generatedDrops)
                {
                    var res = await dbHandler.UpdateInventoryAsync(ctx.Member.Id, drop.Id, drop.Quantity);

                    if (!res)
                    {
                        await ctx.CreateResponseAsync("Something went wrong while calculating rewards");
                        return null;
                    }                
                }

                return generatedDrops;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return null;
            }
        }
    }
}