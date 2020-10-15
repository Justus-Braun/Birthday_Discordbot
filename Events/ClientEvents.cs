using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Birthday_Discordbot.MySql;
using Discord;
using Discord.WebSocket;
using Microsoft.VisualBasic;

namespace Birthday_Discordbot.Events
{
    public static class ClientEvents
    {
        public static async Task MessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot) { return; }

            var guildId = ((SocketGuildChannel) message.Channel).Guild.Id;
            var prefix = MySqlCommends.GetPrefix(guildId);
            var commend = message.Content.Remove(0, prefix.Length).Split(' ').First();
            var condition = message.Content.Remove(0, prefix.Length).Split(' ').Last();
            
            if (message.Content.StartsWith(prefix))
            {
                switch (commend)
                {
                    case "prefix":
                        if(((SocketGuildUser) message.MentionedUsers).GuildPermissions.Administrator)
                            MySqlCommends.ChangePrefix(condition, guildId);
                        break;

                    case "help":
                        await message.Channel.SendMessageAsync("Hallo ich bin der Geburtstagsbot ich kann:\n" +
                                                               $"Geburtsdatum angeben: {prefix}(geburtsdatum)  z.B. {prefix}01.01.2000\n" +
                                                               $"Prefix ändern: {prefix}prefix (neuer Perfix) z.B. {prefix}perfix !");
                        break;
                }



                if (DateTime.TryParse(message.Content.Remove(0, prefix.Length), out var result))
                {
                    var author = message.Author.Id;
                    //YYYY-MM-DD mysql date
                    if (MySqlCommends.UserExist(author, guildId))
                        MySqlCommends.DeleteUser(author, guildId);

                    MySqlCommends.AddUser(author, result, guildId);

                    await message.Channel.SendMessageAsync($"{message.Author.Mention} dich hab ich mir gemerkt");
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
            Console.Write($"{DateTime.Now.ToString(CultureInfo.CurrentCulture).Remove(0, 11)} {msg.Severity} {msg.Source} {msg.Exception}");
            Console.SetCursorPosition(40,Console.CursorTop);
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }
    }
}
