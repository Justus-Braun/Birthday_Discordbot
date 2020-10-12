using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using Birthday_Discordbot.Events;
using Discord;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Collections;
using ConnectionState = System.Data.ConnectionState;

namespace Birthday_Discordbot.MySql
{
    public class MysqlClass
    {
        private static readonly string ConString = JObject.Parse(File.ReadAllText("../../../config.json"))["database"]["connectionString"].ToString();

        private static MySqlConnection _databaseConnection;

        private static MySqlConnection Connection
        {
            get
            {
                if (_databaseConnection == null)
                {
                    LazyInitializer.EnsureInitialized(ref _databaseConnection, CreateConnection);
                }

                return _databaseConnection;
            }
        }

        private static MySqlConnection CreateConnection()
        {
            var db = new MySqlConnection(ConString);
            db.Open();
            ClientEvents.Log(new LogMessage(LogSeverity.Info, "Database", "Connection Open"));
            return db;
        }

        // Wenn ein Fall erzwungen werden muss, um die Verbindung zu schließen
        static void CloseConnection()
        {
            if (_databaseConnection != null)
            {
                _databaseConnection.Close();
                _databaseConnection.Dispose();
                _databaseConnection = null;
            }
        }

        private MySqlCommand CreateCommend(string command)
        {
            MySqlCommand sqlAbfrage = Connection.CreateCommand();
            try
            {
                sqlAbfrage.CommandText = command;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return sqlAbfrage;
        }

        private static MySqlDataReader GetDataReader(MySqlCommand sqlAbfrage) => sqlAbfrage.ExecuteReader();

        private static ulong[] ReaderReads(IDataReader reader)
        {
            List<ulong> list = new List<ulong>();
            try
            {
                while (reader.Read())
                {
                    list.Add((ulong)reader[0]);
                }
                reader.Close();
            }
            catch (Exception e)
            {
                ClientEvents.Log(new LogMessage(LogSeverity.Error, e.Source, e.Message));
            }
            return list.ToArray();
        }

        public void AddUserToDatabase(ulong userId, ulong guildId, DateTime date)
        {
            var mySqlCommand =
                CreateCommend(
                    $"Insert into Birthday_DiscordBot.user (userID, birthday, guild) VALUES ({userId}, \"{date:yyyy-MM-dd}\", {guildId});");
            mySqlCommand.ExecuteNonQuery();
        }

        public void AddGuildToDatabase(ulong guild, ulong channel)
        {
            var mySqlCommand = CreateCommend($"Insert into Birthday_DiscordBot.guilds (guildID, channelID) VALUES (\"{guild}\", \"{channel}\");");
            mySqlCommand.ExecuteNonQuery();
        }

        public ulong[] QueryThroughGuilds(string colummeName)
        {
            bool error;
            ulong[] guilds = default;
            do
            {
                error = false;
                try
                {
                    var mySqlCommend = CreateCommend($"Select {colummeName} from Birthday_DiscordBot.guilds;");
                    var reader = GetDataReader(mySqlCommend);
                    guilds = ReaderReads(reader);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Thread.Sleep(new Random().Next(5));
                    error = true;
                }
            } while (error);
            return guilds;
        }

        public void DeleteGuild(ulong guildId)
        {
            var mySqlCommand = CreateCommend($"delete from Birthday_DiscordBot.guilds where guildID = {guildId};");
            mySqlCommand.ExecuteNonQuery();
        }

        public void DeleteUser(ulong userId, ulong guildId)
        {
            var mySqlCommand = CreateCommend($"delete from Birthday_DiscordBot.user where userID = {userId} and guild = {guildId};");
            mySqlCommand.ExecuteNonQuery();
        }

        public ulong[] GetUserInGuildByBirthday(ulong guildId, DateTime date, bool allYears)
        {
            bool error;
            ulong[] guilds = default;
            do
            {
                error = false;
                try
                {
                    var date2 = date.ToString("yyyy-MM-dd");
                    if (allYears)
                    {
                        date2 = date2.Remove(0,4).Insert(0, "^CU[0-9]'+'");
                    }

                    var mySqlCommend = CreateCommend($"select userID from Birthday_DiscordBot.user left join Birthday_DiscordBot.guilds on user.guild = guilds.guildID where guilds.guildID = {guildId} and user.birthday rlike '{date2}';");
                    var reader = GetDataReader(mySqlCommend);
                    guilds = ReaderReads(reader);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Thread.Sleep(new Random().Next(5));
                    error = true;
                }
            } while (error);
            return guilds;
        }

        public ulong GetUser(ulong userId, ulong guildId)
        {
            bool error;
            ulong user = default;
            do
            {
                error = false;
                try
                {
                    var mySqlCommend = CreateCommend($"select userID from Birthday_DiscordBot.user left join Birthday_DiscordBot.guilds on user.guild = guilds.guildID where guilds.guildID = {guildId} and user.userID = {userId};");
                    var reader = GetDataReader(mySqlCommend);
                    user = ReaderReads(reader).FirstOrDefault();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Thread.Sleep(new Random().Next(5));
                    error = true;
                }
            } while (error);
            return user;
        }


    }
}