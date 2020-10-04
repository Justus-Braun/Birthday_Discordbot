using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Anssi;

namespace Birthday_Discordbot
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;

        private async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.MessageReceived += MessageReceived;


            var token = JObject.Parse(await File.ReadAllTextAsync("../../../config.json"))["api"]["token"].ToString();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private static async Task MessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot)
            {
                return;
            }

            if (message.Content.StartsWith("!"))
            {
                if (IsDate(message.Content.Remove(0, 1), out DateTime result))
                {
                    MysqlClass mysql = new MysqlClass();
                    mysql.AddDataAndUserToDatabase("test", $"{result:yyyy-MM-dd}");
                    await message.Channel.SendMessageAsync($"Fertig");
                }
                   
            }
        }


        private static bool IsDate(string date, out DateTime result)
        {
            //YYYY-MM-DD
            return DateTime.TryParse(date, out result);
        }
    }
}