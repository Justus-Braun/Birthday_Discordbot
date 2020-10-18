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

        private readonly Timer _timer = new Timer(1000 * 60 * 5);

        private async Task MainAsync()
        {
            //Client Events
            Client = new DiscordSocketClient();
            Client.Log += ClientEvents.Log; 
            Client.MessageReceived += ClientEvents.MessageReceived;
            Client.JoinedGuild += ClientEvents.JoinedGuild;

            //Gets Token from Json
            var token = JObject.Parse(await File.ReadAllTextAsync("config.json"))["api"]["token"].ToString();

            //Logging in 
            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();

            //Deletes all unused Guilds in Database and Adds missing ones
            MySqlCommends.SyncClientWithDatabase();

            //Starts timer
            _timer.Elapsed += TimerEvents.Elapsed;
            _timer.Enabled = true;

            await Task.Delay(-1);
        }
    }
}