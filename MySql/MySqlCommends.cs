using System.Linq;
using Discord.WebSocket;

namespace Birthday_Discordbot.MySql
{
    public static class MySqlCommends
    {
        private static readonly MysqlClass Mysql = new MysqlClass();

        public static void DeleteNonUseGuilds(BaseSocketClient client)
        {
            var guildsDatabase = Mysql.QueryThroughGuilds("guildID");
            var guildsClient = client.Guilds.ToArray().Select(variable => variable.Id).ToArray();

            foreach (var value in guildsDatabase)
            {
                if (!guildsClient.Contains(value))
                {
                    Mysql.DeleteGuild(value);
                }
            }
        }

        public static void AddGuild(ulong guildId, ulong channelId) => Mysql.AddGuildToDatabase(guildId, channelId);

        public static void AddUser(string username, string date) => Mysql.AddUserToDatabase(username, date);

        public static ulong[] GetAllGuilds()
        {
            return (ulong[])Mysql.QueryThroughGuilds("*");
        }
    }
}
