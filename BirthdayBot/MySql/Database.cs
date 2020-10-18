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
using static Birthday_Discordbot.Program;
using ConnectionState = Discord.ConnectionState;

namespace Birthday_Discordbot.MySql
{
    public static class Database
    {
        //Gets Connectionstring
        private static readonly string ConString = JObject.Parse(Json)["database"]["connectionString"].ToString();

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

        private static MySqlCommand CreateCommend(string command)
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

        public static string EscapeMySqlInjction(string text)
        {
            text = text.Replace(";", "");
            text = text.Replace("\"", "");
            text = text.Replace("--", "");
            text = text.Replace("/*", "");
            text = text.Replace("*/", "");
            text = text.Replace("'", "");
            text = text.Replace("xp_", "");
            return text;
        }

        #region Reader

        private static MySqlDataReader GetDataReader(MySqlCommand sqlAbfrage) => sqlAbfrage.ExecuteReader();

        private static ulong[] ReaderReadUlong(IDataReader reader)
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

        private static string[] ReaderReadsString(IDataReader reader)
        {
            List<string> list = new List<string>();
            try
            {
                while (reader.Read())
                {
                    list.Add((string)reader[0]);
                }
                reader.Close();
            }
            catch (Exception e)
            {
                ClientEvents.Log(new LogMessage(LogSeverity.Error, e.Source, e.Message));
            }
            return list.ToArray();
        }

        #endregion

        #region DML
        private static string QueryString(string command)
        {
            bool error;
            string queryString = default;
            do
            {
                error = false;
                try
                {
                    var mySqlCommend = CreateCommend(command);
                    var reader = GetDataReader(mySqlCommend);
                    queryString = ReaderReadsString(reader).FirstOrDefault();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Thread.Sleep(new Random().Next(5));
                    error = true;
                }
            } while (error);
            return queryString;
        }

        private static ulong[] QueryUlong(string command)
        {
            bool error;
            ulong[] entrys = default;
            do
            {
                error = false;
                try
                {
                    var mySqlCommend = CreateCommend(command);
                    var reader = GetDataReader(mySqlCommend);
                    entrys = ReaderReadUlong(reader);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Thread.Sleep(new Random().Next(5));
                    error = true;
                }
            } while (error);
            return entrys;
        }

        private static void DataDefinition(string command) => CreateCommend(command).ExecuteNonQuery();
        #endregion

        #region Actions
        //Syncs Bot with Database
        public static void SyncClientWithDatabase()
        {
            //Waits until Client is Connected
            while (Client.ConnectionState != ConnectionState.Connected) Thread.Sleep(new Random().Next(4));

            //Gets all Guilds from Database
            var guildsDatabase = QueryUlong("Select guildID from Birthday_DiscordBot.guilds;");
            //Gets all Guilds from Client
            var guildsClient = Client.Guilds.ToArray().Select(variable => variable.Id).ToArray();

            //Deletes all guilds in Database where the Bot is not there anymore
            foreach (var guildId in guildsDatabase)
            {
                if (!guildsClient.Contains(guildId)) DataDefinition($"delete from Birthday_DiscordBot.guilds where guildID = {guildId};");
            }


            foreach (var guildId in guildsClient)
            {
                if (guildsDatabase.Contains(guildId))
                {
                    //Guild is in Database
                    ClientEvents.Log(new LogMessage(LogSeverity.Info, "In Database",
                        $"Guild: {Client.GetGuild(guildId).Name}"));
                }
                else
                {
                    DataDefinition($"Insert into Birthday_DiscordBot.guilds(guildID, channelID) VALUES(\"{guildId}\", \"{ Client.GetGuild(guildId).DefaultChannel.Id}\");");
                    ClientEvents.Log(new LogMessage(LogSeverity.Info, "Added To Database",
                        $"Guild: {Client.GetGuild(guildId).Name}"));
                }
            }
        }

        public static void AddGuild(ulong guildId, ulong channelId) =>
            DataDefinition(
                $"Insert into Birthday_DiscordBot.guilds (guildID, channelID) VALUES (\"{guildId}\", \"{channelId}\");");

        public static void AddUser(ulong userId, DateTime date, ulong guildId) => 
            DataDefinition($"Insert into Birthday_DiscordBot.user (userID, birthday, guild) VALUES ({userId}, \"{date:yyyy-MM-dd}\", {guildId});");

        public static bool UserExist(ulong userId, ulong guildId) => 
            QueryUlong($"select userID from Birthday_DiscordBot.user left join Birthday_DiscordBot.guilds on user.guild = guilds.guildID where guilds.guildID = {guildId} and user.userID = {userId};").FirstOrDefault() == userId;

        public static ulong[] GetAllGuilds() => 
            QueryUlong("Select * from Birthday_DiscordBot.guilds;");

        public static void DeleteUser(ulong userId, ulong guildId) => 
            DataDefinition($"delete from Birthday_DiscordBot.user where userID = {userId} and guild = {guildId};");

        public static ulong[] GetAllUsersInGuildByBirthday(ulong guildId)
        {
            var stringDate = DateTime.Today.ToString("yyyy-MM-dd").Remove(0, 4).Insert(0, "____");
            return QueryUlong(
                $"select userID from Birthday_DiscordBot.user left join Birthday_DiscordBot.guilds on user.guild = guilds.guildID where guilds.guildID = {guildId} and user.birthday like '{stringDate}';");
        }

        public static string GetPrefix(ulong guildId) => 
            QueryString($"select prefix from Birthday_DiscordBot.guilds where guildID = {guildId};");

        public static void ChangePrefix(string newPrefix, ulong guildId) => 
            DataDefinition($"UPDATE Birthday_DiscordBot.guilds SET prefix = '{newPrefix}' WHERE(guildID = {guildId});");

        public static ulong GetChannel(ulong guildId) => 
            QueryUlong($"select channelID from Birthday_DiscordBot.guilds where guildID = {guildId};").FirstOrDefault();

        public static void SetChannel(ulong guildId, ulong newChannel) => 
            DataDefinition($"update Birthday_DiscordBot.guilds set channelID = {newChannel} where guildID = {guildId};");
        #endregion
    }
}