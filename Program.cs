using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using K4os.Compression.LZ4.Streams;
using Newtonsoft.Json.Linq;
using Timer = System.Timers.Timer;

namespace Birthday_Discordbot
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public static DiscordSocketClient Client;

        private readonly Timer _timer = new Timer(10000);

        private async Task MainAsync()
        {
            Client = new DiscordSocketClient();
            Client.Log += ClientEvents.Log; 
            Client.MessageReceived += ClientEvents.MessageReceived;
            Client.JoinedGuild += ClientEvents.JoinedGuild;

            _timer.Elapsed += TimerEvents.Elapsed;
            _timer.Enabled = true;

            MySqlCommends.DeleteNonUseGuilds(Client);

            var token = JObject.Parse(await File.ReadAllTextAsync("../../../config.json"))["api"]["token"].ToString();

            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();

            await Task.Delay(-1);
        }
    }
}