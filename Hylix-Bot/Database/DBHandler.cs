using Hylix_Bot.Database;
using Npgsql;
using Supabase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

using static System.Console;

namespace Hylix_Bot
{

    internal class DBHandler
    {

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
                            element = ("No Element", (ulong)reader.GetInt64(1));
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
            Monster newElement;

            try
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();

                    var query = "SELECT * FROM data.monsters ORDER BY RANDOM() LIMIT 1;";

                    using (var cmd = new NpgsqlCommand(query, db))
                    {
                        var reader = await cmd.ExecuteReaderAsync();

                        await reader.ReadAsync();

                        var eleData = await GetElementAsync(reader.GetInt32(3));

                        newElement = new Monster()
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            Element = eleData.Item1,
                            Element_Emoji = eleData.Item2,
                            Species = await GetSpeciesAsync(reader.GetInt32(4)),
                            Affiliation = await GetAffiliationAsync(reader.GetInt32(5))
                        };
                    }
                }

                return newElement;
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<(bool, ulong)> StoreGuildMonsterAsync(int monsterId, ulong guildId)
        {
            ulong spawnId;

            string uniqueID = Ulid.NewUlid().ToString();

            try
            {
                using (var db = new NpgsqlConnection(dbconn))
                {
                    await db.OpenAsync();                    

                    var query = $"INSERT INTO data.monster_spawns (monster_id, guild_id, identifier) VALUES ({monsterId}, {guildId}, '{uniqueID}');";

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

                return (true, spawnId);
            }
            catch (Exception ex)
            {
                WriteLine(ex.ToString());
                return (false, 0);
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
    }
}
