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

            var guildsDatabase = Mysql.QueryUlong("Select guildID from Birthday_DiscordBot.guilds;");
            var guildsClient = Program.Client.Guilds.ToArray().Select(variable => variable.Id).ToArray();
             
            foreach (var guildId in guildsDatabase)
            {
                if (!guildsClient.Contains(guildId)) Mysql.UpdateOrInsert($"delete from Birthday_DiscordBot.guilds where guildID = {guildId};");
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
                    Mysql.UpdateOrInsert($"Insert into Birthday_DiscordBot.guilds(guildID, channelID) VALUES(\"{guildId}\", \"{ Program.Client.GetGuild(guildId).DefaultChannel.Id}\");");
                    ClientEvents.Log(new LogMessage(LogSeverity.Info, "Added To Database",
                        $"Guild: {Program.Client.GetGuild(guildId).Name}"));
                }
            }
        }

        public static void AddGuild(ulong guildId, ulong channelId) => Mysql.UpdateOrInsert($"Insert into Birthday_DiscordBot.guilds (guildID, channelID) VALUES (\"{guildId}\", \"{channelId}\");");

        public static void AddUser(ulong userId, DateTime date, ulong guildId) => Mysql.UpdateOrInsert($"Insert into Birthday_DiscordBot.user (userID, birthday, guild) VALUES ({userId}, \"{date:yyyy-MM-dd}\", {guildId});");

        public static bool UserExist(ulong userId, ulong guildId) => Mysql.QueryUlong($"select userID from Birthday_DiscordBot.user left join Birthday_DiscordBot.guilds on user.guild = guilds.guildID where guilds.guildID = {guildId} and user.userID = {userId};").FirstOrDefault() == userId;

        public static ulong[] GetAllGuilds() => Mysql.QueryUlong($"Select * from Birthday_DiscordBot.guilds;");

        public static void DeleteUser(ulong userId, ulong guildId) => Mysql.UpdateOrInsert($"delete from Birthday_DiscordBot.user where userID = {userId} and guild = {guildId};");

        public static ulong[] GetAllUsersInGuildByBirthday(ulong guildId)
        {
            var stringDate = DateTime.Today.ToString("yyyy-MM-dd").Remove(0, 4).Insert(0, "____");
            return Mysql.QueryUlong(
                $"select userID from Birthday_DiscordBot.user left join Birthday_DiscordBot.guilds on user.guild = guilds.guildID where guilds.guildID = {guildId} and user.birthday like '{stringDate}';");
        }

        public static string GetPrefix(ulong guildId) => Mysql.QueryString($"select prefix from Birthday_DiscordBot.guilds where guildID = {guildId};");

        public static void ChangePrefix(string newPrefix, ulong guildId) => Mysql.UpdateOrInsert($"UPDATE Birthday_DiscordBot.guilds SET prefix = '{newPrefix}' WHERE(guildID = {guildId});");

        public static ulong GetChannel(ulong guildId) => Mysql.QueryUlong($"select channelID from Birthday_DiscordBot.guilds where guildID = {guildId};").FirstOrDefault();

        public static void SetChannel(ulong guildId, ulong newChannel) => Mysql.UpdateOrInsert($"update Birthday_DiscordBot.guilds set channelID = {newChannel} where guildID = {guildId};");
    }
}
