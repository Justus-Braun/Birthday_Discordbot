using System;
using System.Linq;
using System.Threading;
using Birthday_Discordbot.Events;
using Discord;

namespace Birthday_Discordbot.MySql
{
    public static class MySqlCommends
    {
        private static readonly MysqlClass Mysql = new MysqlClass();

        public static void SyncClientWithDatabase()
        {
            while (Program.Client.ConnectionState != ConnectionState.Connected) Thread.Sleep(new Random().Next(4));

            var guildsDatabase = Mysql.QueryThroughGuilds("guildID");
            var guildsClient = Program.Client.Guilds.ToArray().Select(variable => variable.Id).ToArray();
             
            foreach (var guildId in guildsDatabase)
            {
                if (!guildsClient.Contains(guildId)) Mysql.DeleteGuild(guildId);
            }

            foreach (var guildId in guildsClient)
            {
                if (guildsDatabase.Contains(guildId))
                {
                    //Guild is in Database
                    ClientEvents.Log(new LogMessage(LogSeverity.Info, "In Database",
                        $"Guild: {Program.Client.GetGuild(guildId).Name}"));
                }
                else
                {
                    Mysql.AddGuildToDatabase(guildId, Program.Client.GetGuild(guildId).DefaultChannel.Id);
                    ClientEvents.Log(new LogMessage(LogSeverity.Info, "Added To Database",
                        $"Guild: {Program.Client.GetGuild(guildId).Name}"));
                }
            }
        }

        public static void AddGuild(ulong guildId, ulong channelId) => Mysql.AddGuildToDatabase(guildId, channelId);

        public static void AddUser(ulong userId, DateTime date, ulong guildId) => Mysql.AddUserToDatabase(userId, guildId, date);

        public static bool UserExist(ulong userId, ulong guildId) => Mysql.GetUser(userId, guildId) == userId;

        public static ulong[] GetAllGuilds() => Mysql.QueryThroughGuilds("*");

        public static void DeleteUser(ulong userId, ulong guildId) => Mysql.DeleteUser(userId, guildId);


        //TODO Use only date and no year
        public static ulong[] GetAllUsersInGuildByBirthday(ulong guildId) => Mysql.GetUserByGuild(guildId, DateTime.Today);
    }
}
