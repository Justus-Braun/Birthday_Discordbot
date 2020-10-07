using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

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
            Console.WriteLine("Database Connection is Open");
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

        public void AddUserToDatabase(string username, string date)
        {
            var mySqlCommand = CreateCommend($"Insert into Birthday_DiscordBot.user (username, birthday) VALUES (\"{username}\", \"{date}\");");
            mySqlCommand.ExecuteNonQuery();
        }

        public void AddGuildToDatabase(ulong guild, ulong channel)
        {
            var mySqlCommand = CreateCommend($"Insert into Birthday_DiscordBot.guilds (guildID, channelID) VALUES (\"{guild}\", \"{channel}\");");
            mySqlCommand.ExecuteNonQuery();
        }

        public IEnumerable<ulong> QueryThroughGuilds(string colummeName)
        {
            var mySqlCommend = CreateCommend($"Select {colummeName} from Birthday_DiscordBot.guilds;");
            var reader = GetDataReader(mySqlCommend);
            var guilds = reader.Cast<ulong>().ToArray();
            reader.Close();
            return guilds;
        }

        public void DeleteGuild(ulong guildId)
        {
            var mySqlCommand = CreateCommend($"delete from guilds where guildID = {guildId};");
            mySqlCommand.ExecuteNonQuery();
        }
    }
}