using System;
using System.Threading.Tasks;
using Birthday_Discordbot.MySql;
using Discord;
using Discord.WebSocket;

namespace Birthday_Discordbot.Events
{
    public static class ClientEvents
    {
        public static async Task MessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot) { return; }
            if (message.Content.StartsWith("!"))
            {
                if (DateTime.TryParse(message.Content.Remove(0, 1), out DateTime result))
                {
                    //YYYY-MM-DD mysql date
                    MySqlCommends.AddUser(message.Author.Id, result, ((SocketGuildChannel) message.Channel).Id);
                    await message.Channel.SendMessageAsync($"Fertig");
                }
                else
                {
                    await message.Channel.SendMessageAsync("Wrong Dataformat");
                }
            }
        }

        public static async Task JoinedGuild(SocketGuild guild)
        {
            MySqlCommends.AddGuild(guild.Id, guild.DefaultChannel.Id);
            await Log(new LogMessage(LogSeverity.Info, "Added To Database", $"Guild: {guild.Name}"));
        }

        public static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
