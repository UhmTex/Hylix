using Hylix_Bot.Database;
using Npgsql;
using Supabase;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using static System.Console;

namespace Hylix_Bot
{

    internal class DBHandler
    {
        private Random random = new Random();

        readonly string dbconn = "Server=aws-0-eu-central-1.pooler.supabase.com;Port=6543;Username=postgres.xhtgynnynxkxrkecteps;Password=rNz15gvrWvpdN8md7HvP;Database=postgres;Timeout=300;Pooling=false;CommandTimeout=300";

        public async Task<bool> StoreUserAsync(User user)
        {
            var validUser = await UserValidAsync(user);

            if (validUser == 1)
            {
                return false;
            }

            try
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = "INSERT INTO data.userdata (user_name, user_id) " +
                                $"VALUES ('{user.userName}', '{user.userId}');";

                    using (var cmd = new NpgsqlCommand(query, db))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return false;
            }
        }
        
        private async Task<int> UserValidAsync(User user)
        {
            int validation = 0;

            try
            {
                using (var conn = new NpgsqlConnection(dbconn))
                {
                    await conn.OpenAsync();

                    var query = $"SELECT COUNT(*) FROM data.userdata u WHERE u.user_name='{user.userName}' AND u.user_id={user.userId};";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        var check = await cmd.ExecuteScalarAsync();

                        validation = Convert.ToInt16(check);
                    }
                }

                return validation;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return -1;
            }
        }

        public async Task<bool> UpdateUserClassAsync(User user, int classId)
        {
            var validUser = await UserValidAsync(user);

            if (validUser == 0)
            {
                await StoreUserAsync(user);
            }

            try
            {
                using (var conn = new NpgsqlConnection(dbconn))
                {
                    await conn.OpenAsync();

                    var query = $"UPDATE data.userdata SET class_id = {classId} WHERE user_id = {user.userId}";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task<bool> UpdateUserGold(BigInteger userId, BigInteger quantity)
        {
            try 
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = $"UPDATE data.userdata SET gold = gold + {quantity} WHERE user_id = {userId};";

                    using (var cmd = new NpgsqlCommand(query, db))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task<string> GetClassAsync(int classId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(dbconn))
                {
                    await conn.OpenAsync();

                    var query = $"SELECT (name) FROM data.classes WHERE id={classId};";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        var reader = await cmd.ExecuteReaderAsync();
                        await reader.ReadAsync();

                        return reader.GetString(0);
                    }
                }
            }
            catch (Exception e)
            {
                WriteLine(e.ToString());
                return null;
            }
        }

        public async Task<string> GetAffiliationAsync(int affiliationId)
        {
            try
            {
                using (var conn = new NpgsqlConnection(dbconn))
                {
                    await conn.OpenAsync();

                    var query = $"SELECT (name) FROM data.affiliation WHERE id={affiliationId};";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        var reader = await cmd.ExecuteReaderAsync();

                        await reader.ReadAsync();

                        return reader.GetString(0);
                    }
                }
            }
            catch (Exception e)
            {
                WriteLine(e.ToString());
                return null;
            }
        }

        public async Task<string> GetSpeciesAsync(int speciesId)
        {
            var species = "ERROR";

            try
            {
                using (var conn = new NpgsqlConnection(dbconn))
                {
                    await conn.OpenAsync();

                    var query = $"SELECT (name) FROM data.species WHERE id={speciesId};";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        var reader = await cmd.ExecuteReaderAsync();

                        await reader.ReadAsync();

                        species = reader.GetString(0);
                    }
                }

                return species;
            }
            catch (Exception e)
            {
                WriteLine(e.ToString());
                return species;
            }
        }

        public async Task<(string, ulong)> GetElementAsync(int elementId)
        {
            (string, ulong) element = ("ERROR", 0);

            try
            {
                using (var conn = new NpgsqlConnection(dbconn))
                {
                    await conn.OpenAsync();

                    var query = $"SELECT name, emoji_id FROM data.elements WHERE id={elementId};";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        var reader = await cmd.ExecuteReaderAsync();

                        await reader.ReadAsync();

                        if (reader.GetString(0) != "None")
                        {
                            element = (reader.GetString(0), (ulong)reader.GetInt64(1));
                        }
                        else
                        {
                            element = ("None", (ulong)reader.GetInt64(1));
                        }
                    }
                }

                return element;
            }
            catch (Exception e)
            {
                WriteLine(e.ToString());
                return ("Error!", 0);
            }
        }

        public async Task<UserProfile> GetUserProfileAsync(ulong UserId)
        {
            UserProfile userProfile;

            try
            {
                using (var conn = new NpgsqlConnection(dbconn))
                {
                    await conn.OpenAsync();

                    var query = $"SELECT species_id, affiliation_id, class_id, gold FROM data.userdata WHERE user_id={UserId};";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        var reader = await cmd.ExecuteReaderAsync();

                        await reader.ReadAsync();

                        userProfile = new UserProfile
                        {                            
                            gold = (ulong)reader.GetInt64(3),
                            speciesId = reader.GetInt32(0),
                            affiliationId = reader.GetInt32(1),
                            classId = reader.GetInt32(2)
                        };
                    }                    
                }

                return userProfile;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return new UserProfile 
                { 
                    gold = 0,
                    speciesId = 1,
                    affiliationId = 1,
                    classId = 1
                };

            }
        }

        public async Task<List<KeyValuePair<BigInteger, string>>> GetAllSpeciesAsync()
        {
            List<KeyValuePair<BigInteger, string>> speciesList = new List<KeyValuePair<BigInteger, string>>();

            try
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = "SELECT id, name FROM data.species;";

                    using (var cmd = new NpgsqlCommand(query, db))
                    {
                        var reader = await cmd.ExecuteReaderAsync();

                        foreach (var item in reader)
                        {
                            var KeyValuePairData = new KeyValuePair<BigInteger, string>(reader.GetInt64(0), reader.GetString(1));
                            speciesList.Add(KeyValuePairData);
                        }

                    }
                }

                return speciesList;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                speciesList.Add(new KeyValuePair<BigInteger, string>(0, "Error"));
                return speciesList;
            }
        }

        public async Task<List<KeyValuePair<BigInteger, string>>> GetAllClassesAsync()
        {
            List<KeyValuePair<BigInteger, string>> classesList = new List<KeyValuePair<BigInteger, string>>();

            try
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = "SELECT id, name FROM data.classes;";

                    using (var cmd = new NpgsqlCommand(query, db))
                    {
                        var reader = await cmd.ExecuteReaderAsync();

                        foreach (var item in reader)
                        {
                            var KeyValuePairData = new KeyValuePair<BigInteger, string>(reader.GetInt64(0), reader.GetString(1));
                            classesList.Add(KeyValuePairData);
                        }

                    }
                }

                return classesList;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                classesList.Add(new KeyValuePair<BigInteger, string>(0, "Error"));
                return classesList;
            }
        }

        public async Task<List<Element>> GetAllElementsAsync()
        {
            List<Element> elementsList = new List<Element>();

            try
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = "SELECT id, name, emoji_id FROM data.elements;";

                    using (var cmd = new NpgsqlCommand(query, db))
                    {
                        var reader = await cmd.ExecuteReaderAsync();

                        foreach (var item in reader)
                        {
                            var newElement = new Element()
                            {
                                Id = (ulong)reader.GetInt64(0),
                                Name = reader.GetString(1),
                                Emoji_Id = (ulong)reader.GetInt64(2)
                            };
                            elementsList.Add(newElement);
                        }

                    }
                }

                return elementsList;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                var newElement = new Element()
                {
                    Id = 1,
                    Name = "Error",
                    Emoji_Id = 1,
                };
                elementsList.Add(newElement);
                return elementsList;
            }
        }

        public async Task<Monster> GetRandomMonster()
        {
            Monster selectedMonster = new Monster
            {
                Id = 0,
                Name = "ERROR",
                Description = "Something went wrong...",
                Element = "ERROR",
                Element_Emoji = 0,
                Affiliation = "Probably bad",
                Spawn_Chance = 0                
            };

            try
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = "SELECT id, name, description, element_id, species_id, affiliation_id, spawn_chance FROM data.monsters;";

                    using (var cmd = new NpgsqlCommand(query, db)) 
                    {
                        var reader = await cmd.ExecuteReaderAsync();

                        List<Monster> monsterList = new List<Monster>();
                        int totalWeight = 0;

                        while (await reader.ReadAsync()) 
                        {
                            var eleData = await GetElementAsync(reader.GetInt32(3));

                            var newMonster = new Monster()
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.GetString(2),
                                Element = eleData.Item1,
                                Element_Emoji = eleData.Item2,
                                Species = await GetSpeciesAsync(reader.GetInt32(4)),
                                Affiliation = await GetAffiliationAsync(reader.GetInt32(5)),
                                Spawn_Chance = reader.GetInt32(6)
                            };

                            monsterList.Add(newMonster);
                            totalWeight += reader.GetInt32(6);
                        }

                        int randomValue = random.Next(totalWeight);
                        int cumulativeWeight = 0;

                        foreach (var monster in monsterList) 
                        {
                            cumulativeWeight += (int)monster.Spawn_Chance;
                            if (randomValue < cumulativeWeight)
                            {
                                selectedMonster = monster;
                                break;
                            }
                        }                    
                    }
                }

                return selectedMonster;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return selectedMonster;
            }
        }

        public async Task<(bool, Monster)> GetMonsterAsync(int monsterId) {
            Monster monster = new Monster
            {
                Id = 0,
                Name = "ERROR",
                Description = "Something went wrong...",
                Element = "ERROR",
                Element_Emoji = 0,
                Affiliation = "Probably bad",
                Spawn_Chance = 0,
                Tier = 0             
            };

            NpgsqlDataReader reader;

            try 
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = $"SELECT id, name, description, element_id, species_id, affiliation_id, tier_id FROM data.monsters WHERE id={monsterId};";

                    using (var cmd = new NpgsqlCommand(query, db)) 
                    {
                        reader = await cmd.ExecuteReaderAsync();

                        await reader.ReadAsync();

                        if (!reader.HasRows) {
                            await db.CloseAsync();
                            return (false, monster);
                        }

                        var monsterElement = await GetElementAsync(reader.GetInt32(3));

                        monster = new Monster 
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            Element = monsterElement.Item1,
                            Element_Emoji = monsterElement.Item2,
                            Species = await GetSpeciesAsync(reader.GetInt32(4)),
                            Affiliation = await GetAffiliationAsync(reader.GetInt32(5)),
                            Spawn_Chance = await GetSpawnChanceAsync(reader.GetInt32(0)),
                            Tier = reader.GetInt32(6)
                        };
                    }
                }

                return (true, monster);
            }
            catch (Exception ex) 
            {
                WriteLine(ex.ToString());
                return (false, monster);
            }
        }

        public async Task<float> GetSpawnChanceAsync(int monsterId)
        {
            var result = 0f;
            try
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = $"select (select spawn_chance from data.monsters where id={monsterId})::float/(select sum(spawn_chance) from data.monsters)::float*100;";

                    using (var cmd = new NpgsqlCommand(query, db))
                    {
                        var num = await cmd.ExecuteScalarAsync();

                        result = (float)Convert.ToDouble(num);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return 0;
            }
        }

        public async Task<(bool, ulong, string)> StoreGuildMonsterAsync(int monsterId, ulong guildId, ulong messageId=0)
        {
            ulong spawnId;

            string uniqueID = Ulid.NewUlid().ToString();

            try
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();                    

                    var query = $"INSERT INTO data.monster_spawns (monster_id, guild_id, identifier, message_id) VALUES ({monsterId}, {guildId}, '{uniqueID}', {messageId});";

                    using (var cmd = new NpgsqlCommand(query, db))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = $"SELECT id FROM data.monster_spawns WHERE identifier='{uniqueID}'";

                    using (var cmd = new NpgsqlCommand(query, db))
                    {
                        var reader = await cmd.ExecuteReaderAsync();

                        await reader.ReadAsync();

                        spawnId = (ulong)reader.GetInt64(0);
                    }
                }

                return (true, spawnId, uniqueID);
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return (false, 0, "ERROR");
            }
        }

        public async Task<bool> UpdateGuildMonsterAsync(ulong messageId, string identifier) 
        {
            try 
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = $"UPDATE data.monster_spawns SET message_id={messageId} WHERE identifier='{identifier}';";

                    using (var cmd = new NpgsqlCommand(query, db)) 
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex) 
            {
                WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task<(bool, ulong, ulong, ulong)> GetGuildMonsterAsync(ulong fightId)
        {
            ulong messageId = 0;
            ulong monsterId = 0;

            try 
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = $"SELECT message_id, monster_id FROM data.monster_spawns WHERE id={fightId}";

                    using (var cmd = new NpgsqlCommand(query, db))
                    {
                        var reader = await cmd.ExecuteReaderAsync();

                        if (!reader.HasRows) 
                        {
                            await db.CloseAsync();
                            return (false, 0, 0, 0);
                        }

                        await reader.ReadAsync();

                        messageId = (ulong)reader.GetInt64(0);
                        monsterId = (ulong)reader.GetInt64(1);
                    }
                }

                return (true, messageId, fightId, monsterId);
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return (false, 0, 0, 0);
            }
        }

        public async Task<bool> GetMonsterValidityAsync(ulong fightId)
        {
            bool valid;
            
            try
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = $"SELECT COUNT(*) FROM data.monster_spawns WHERE id={fightId}";

                    using (var cmd = new NpgsqlCommand(query, db))
                    {
                        var reader = await cmd.ExecuteScalarAsync();

                        if (Convert.ToInt64(reader) > 0)
                        {
                            valid = true;
                        }
                        else 
                        {
                            valid = false;
                        }
                    }
                }

                return valid;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task DeleteGuildMonsterAsync(ulong spawn_Id)
        {
            try
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = $"DELETE FROM data.monster_spawns WHERE id={spawn_Id};";

                    using (var cmd = new NpgsqlCommand(query,db))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
            }
        }

        public async Task<(bool, Drop)> GenerateDropAsync()
        {
            Drop resultDrop = new Drop();

            try
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = "SELECT id, name, emoji_id, drop_chance, count_range FROM data.drops;";

                    using (var cmd = new NpgsqlCommand(query, db))
                    {
                        var reader = await cmd.ExecuteReaderAsync();

                        if (!reader.HasRows)
                        {
                            WriteLine("no lines");
                            await db.CloseAsync();
                            return (false, resultDrop);
                        }

                        var alldropsList = new List<Drop>();
                        int totalWeight = 0;

                        while (await reader.ReadAsync())
                        {
                            var newDrop = new Drop()
                            {
                                Id = (ulong)reader.GetInt64(0),
                                Name = reader.GetString(1),
                                Emoji_Id = (ulong)reader.GetInt64(2),
                                Drop_Chance = reader.GetInt32(3),
                                Count_Range = (int[])reader.GetValue(4),
                                Quantity = Random.Shared.Next(((int[])reader.GetValue(4))[0], ((int[])reader.GetValue(4))[1])
                            };
                            
                            alldropsList.Add(newDrop);
                            totalWeight += reader.GetInt32(3);
                        }

                        int randomValue = random.Next(totalWeight);
                        int cumulativeWeight = 0;

                        foreach (var drop in alldropsList)
                        {
                            cumulativeWeight += drop.Drop_Chance;
                            
                            if (randomValue < cumulativeWeight)
                            {
                                resultDrop = drop;
                                break;
                            }
                        }    
                    }
                }

                return (true, resultDrop);
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return (false, resultDrop);
            }
        }
    
        public async Task<(bool, List<InventoryItem>)> GetInventoryAsync(ulong userId)
        {
            List<InventoryItem> inventoryList = new List<InventoryItem>();

            try
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = $@"
                        SELECT 
                            ui.quantity,
                            d.name,
                            d.emoji_id,
                            d.id,
                            d.market_value
                        FROM
                            data.user_inventory ui
                        JOIN
                            data.userdata ud ON ui.user_id = ud.id
                        JOIN
                            data.drops d ON ui.item_id= d.id
                        WHERE
                            ud.user_id={userId}";

                    using (var cmd = new NpgsqlCommand(query, db))
                    {
                        var reader = await cmd.ExecuteReaderAsync();

                        if (!reader.HasRows) 
                        {
                            return (true, inventoryList);
                        }

                        while (await reader.ReadAsync())
                        {
                            var inventoryItem = new InventoryItem
                            {
                                Quantity = reader.GetInt64(0),
                                Name = reader.GetString(1),
                                Emoji_Id = reader.GetInt64(2),
                                Id = reader.GetInt64(3),
                                Market_Value = reader.GetInt64(4)
                            };

                            inventoryList.Add(inventoryItem);
                        }
                    }
                }

                return (true, inventoryList);
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return (false, inventoryList);
            }
        }
    
        public async Task<bool> UpdateInventoryAsync(ulong userId, BigInteger itemId, int quantity)
        {
            try
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = $@"
                        INSERT INTO data.user_inventory as ui (user_id, item_id, quantity)
                        VALUES
                            (
                                (SELECT id FROM data.userdata WHERE user_id = {userId}), 
                                {itemId}, 
                                {quantity}
                            )
                        ON CONFLICT (user_id, item_id) 
                        DO UPDATE SET 
                            quantity = ui.quantity + EXCLUDED.quantity;";

                    using (var cmd = new NpgsqlCommand(query, db))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    using (var db2 = new NpgsqlConnection(dbconn))
                    {
                        await db2.OpenAsync();

                        var query2 = "DELETE FROM data.user_inventory WHERE quantity <= 0;";

                        using (var cmd = new NpgsqlCommand(query2, db))
                        {
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return false;
            }
        }
    }
}
