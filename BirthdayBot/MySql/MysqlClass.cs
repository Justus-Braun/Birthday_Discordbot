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

namespace Birthday_Discordbot.MySql
{
    public class MysqlClass
    {
        private static readonly string ConString = JObject.Parse(File.ReadAllText("config.json"))["database"]["connectionString"].ToString();

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

        public string QueryString(string command)
        {
            bool error;
            string prefix = default;
            do
            {
                error = false;
                try
                {
                    var mySqlCommend = CreateCommend(command);
                    var reader = GetDataReader(mySqlCommend);
                    prefix = ReaderReadsString(reader).FirstOrDefault();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Thread.Sleep(new Random().Next(5));
                    error = true;
                }
            } while (error);
            return prefix;
        }

        public ulong[] QueryUlong(string command)
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

        public void UpdateOrInsert(string command) => CreateCommend(command).ExecuteNonQuery();
    }
}