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
        //Event gets Triggert if Bot Received a Message
        public static async Task MessageReceived(SocketMessage message)
        {
            //If the Author is a bot it leaves method
            if (message.Author.IsBot) { return; }

            var guildId = ((SocketGuildChannel) message.Channel).Guild.Id;
            var prefix = Database.GetPrefix(guildId);
            var commend = message.Content.Remove(0, prefix.Length).Split(' ').First();
            var condition = message.Content.Remove(0, prefix.Length).Split(' ').Last();

            //If the Message starts with the set Prefix
            if (message.Content.StartsWith(prefix))
            {
                switch (commend)
                {
                    case "":
                    case null:
                        break;

                    case "prefix":
                        //Changes prefix
                        if (IsAuthorAdmin(message))
                        {
                            Database.ChangePrefix(Database.EscapeMySqlInjction(condition), guildId);
                            await message.Channel.SendMessageAsync($"Prefix changed to {condition}");
                        }
                        break;

                    case "help":
                        //Help menu gets send
                        await message.Channel.SendMessageAsync("Hallo ich bin der Geburtstagsbot ich kann:\n" +
                                                               $"Geburtsdatum angeben: {prefix}(Geburtsdatum) DD.MM.YYYY  z.B. {prefix}01.01.2000\n" + 
                                                               $"Prefix ändern: {prefix}prefix (neuer Prefix) z.B. {prefix}prefix !\n" +
                                                               $"Ausgabe Channel ändern: {prefix}set");
                        break;

                    case "set":
                        //With this command user can set the Output Channel
                        if (IsAuthorAdmin(message)) 
                        {
                            //Sets Channel
                            Database.SetChannel(guildId, message.Channel.Id);
                            await message.Channel.SendMessageAsync("Ich gratuliere jetzt hier");
                        }
                        break;

                    default:
                        commend = ConvertToUsDate(commend);
                        if (DateTime.TryParse(commend, out var result))
                        {
                            //Gets Id from Author
                            var author = message.Author.Id;

                            //Deletes User if he is in DB
                            if (Database.UserExist(author, guildId))
                                Database.DeleteUser(author, guildId);

                            //User gets added
                            Database.AddUser(author, result, guildId);

                            await message.Channel.SendMessageAsync(
                                $"{message.Author.Mention} dich hab ich mir gemerkt");
                        }
                        else
                        {
                            //Sends Error Message
                            await message.Channel.SendMessageAsync("Wrong dataformat or worng spell");
                        }

                        break;
                }
            }
        }

        //Converts EU date to Us date
        private static string ConvertToUsDate(string commend)
        {
            var day = commend.Split('.').First();
            var month = commend.Split('.')[1];
            var year = commend.Split('.').Last();
            return $"{month}.{day}.{year}";
        }

        //Checks if User is admin on guild
        private static bool IsAuthorAdmin(SocketMessage message) => ((SocketGuildUser) message.Author).GuildPermissions.Administrator;

        //Event gets triggered if bot joins guild
        public static async Task JoinedGuild(SocketGuild guild)
        {
            Database.AddGuild(guild.Id, guild.DefaultChannel.Id);
            await Log(new LogMessage(LogSeverity.Info, "Added To Database", $"Guild: {guild.Name}"));
        } 

        //This method is used to write log
        public static Task Log(LogMessage msg)
        {
            Console.Write($"{DateTime.Now.ToString(CultureInfo.CurrentCulture).Remove(0, 11)} {msg.Severity} {msg.Source} {msg.Exception}");
            Console.SetCursorPosition(40, Console.CursorTop);
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }
    }
}
