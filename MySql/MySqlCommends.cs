using System;
using System.Linq;
using System.Threading;
using Discord;
using Discord.WebSocket;

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
             
            foreach (var value in guildsDatabase)
            {
                if (!guildsClient.Contains(value))
                {
                    Mysql.DeleteGuild(value);
                }
            }

            foreach (var variable in guildsClient)
            {
                if (!guildsDatabase.Contains(variable))
                {
                    Mysql.AddGuildToDatabase(variable, Program.Client.GetGuild(variable).DefaultChannel.Id);
                }
            }
        }

        public static void AddGuild(ulong guildId, ulong channelId) => Mysql.AddGuildToDatabase(guildId, channelId);

        public static void AddUser(ulong userId, DateTime date, ulong guildId) => Mysql.AddUserToDatabase(userId, guildId, date);

        public static ulong[] GetAllGuilds() => (ulong[])Mysql.QueryThroughGuilds("*");

        public static ulong[] GetAllUsersInGuildByBirthday(ulong guildId) => Mysql.GetUserByGuild(guildId, DateTime.Today);
    }
}
