using System;
using System.Globalization;
using System.Linq;
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

            var guildId = ((SocketGuildChannel)message.Channel).Guild.Id;
            var prefix = MySqlCommends.GetPrefix(guildId);
            var commend = message.Content.Remove(0, prefix.Length).Split(' ').First();
            var condition = message.Content.Remove(0, prefix.Length).Split(' ').Last();

            if (message.Content.StartsWith(prefix))
            {
                switch (commend)
                {
                    case "":
                    case null:
                        break;

                    case "prefix":
                        if (IsAuthorAdmin(message))
                        {
                            MySqlCommends.ChangePrefix(condition, guildId);
                            await message.Channel.SendMessageAsync($"Prefix changed to {condition}");
                        }
                        break;

                    case "help":
                        await message.Channel.SendMessageAsync("Hallo ich bin der Geburtstagsbot ich kann:\n" +
                                                               $"Geburtsdatum angeben: {prefix}(Geburtsdatum) DD.MM.YYYY  z.B. {prefix}01.01.2000\n" + 
                                                               $"Prefix ändern: {prefix}prefix (neuer Prefix) z.B. {prefix}prefix !" +
                                                               $"Ausgabe Channel ändern: {prefix}set");
                        break;

                    case "set":
                        if (IsAuthorAdmin(message))
                        {
                            MySqlCommends.SetChannel(guildId, message.Channel.Id);
                            await message.Channel.SendMessageAsync("Ich gratuliere jetzt hier");
                        }
                        break;


                    default:
                        if (DateTime.TryParse(commend, out var result))
                        {
                            //Gets Id from Author
                            var author = message.Author.Id;

                            //YYYY-MM-DD mysql date
                            //Deletes User if he is in DB
                            if (MySqlCommends.UserExist(author, guildId))
                                MySqlCommends.DeleteUser(author, guildId);

                            //User gehts Added
                            MySqlCommends.AddUser(author, result, guildId);

                            await message.Channel.SendMessageAsync(
                                $"{message.Author.Mention} dich hab ich mir gemerkt");
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("Wrong Dataformat");
                        }

                        break;
                }
            }
        }

        private static bool IsAuthorAdmin(SocketMessage message) => ((SocketGuildUser) message.Author).GuildPermissions.Administrator;

        public static async Task JoinedGuild(SocketGuild guild)
        {
            MySqlCommends.AddGuild(guild.Id, guild.DefaultChannel.Id);
            await Log(new LogMessage(LogSeverity.Info, "Added To Database", $"Guild: {guild.Name}"));
        }

        public static Task Log(LogMessage msg)
        {
            Console.Write($"{DateTime.Now.ToString(CultureInfo.CurrentCulture).Remove(0, 11)} {msg.Severity} {msg.Source} {msg.Exception}");
            Console.SetCursorPosition(40, Console.CursorTop);
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }
    }
}
