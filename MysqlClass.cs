using System;
using System.IO;
using System.Threading;
using MySql.Data.MySqlClient;

namespace Birthday_Discordbot
{
    public class MysqlClass
    {
        private static readonly string ConString = GetConString();

        private static string GetConString()
        {
            const string path = "../../../MySqlServerCon.txt";
            using StreamReader sr = new StreamReader(path);
            return sr.ReadLine();
        }


        static MySqlConnection _databaseConnection;

        public static MySqlConnection Connection
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

        static MySqlConnection CreateConnection()
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

        private MySqlDataReader GetDataReader(MySqlCommand sqlAbfrage)
        {
            return sqlAbfrage.ExecuteReader();
        }

        public void Test()
        {
            CreateCommend("test");
        }
    }
}