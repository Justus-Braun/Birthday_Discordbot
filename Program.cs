using System.IO;
using System.Threading.Tasks;
using Birthday_Discordbot.Events;
using Birthday_Discordbot.MySql;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Timer = System.Timers.Timer;

namespace Birthday_Discordbot
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public static DiscordSocketClient Client;

        private readonly Timer _timer = new Timer(1000);

        private async Task MainAsync()
        {
            Client = new DiscordSocketClient();
            Client.Log += ClientEvents.Log; 
            Client.MessageReceived += ClientEvents.MessageReceived;
            Client.JoinedGuild += ClientEvents.JoinedGuild;

            var token = JObject.Parse(await File.ReadAllTextAsync("../../../config.json"))["api"]["token"].ToString();

            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();

            MySqlCommends.SyncClientWithDatabase();

            _timer.Elapsed += TimerEvents.Elapsed;
            _timer.Enabled = true;

            await Task.Delay(-1);
        }
    }
}