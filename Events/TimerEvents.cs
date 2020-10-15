using System;
using System.Timers;
using Birthday_Discordbot.MySql;
using Discord;

namespace Birthday_Discordbot.Events
{
    public static class TimerEvents
    {
        private static DateTime _lastTimeCheck = DateTime.Today.Subtract(TimeSpan.FromDays(1));

        public static async void Elapsed(object sender, ElapsedEventArgs e)
        {
            //Check if day passed 
            if (DateTime.Today == _lastTimeCheck) return;

            //If new data write it to log
            await ClientEvents.Log(new LogMessage(LogSeverity.Info, "Timer", "New Day"));

            //Get all guilds from Database
            var allGuilds = MySqlCommends.GetAllGuilds();
            foreach (var variable in allGuilds)
            {
                var guild = Program.Client.GetGuild(variable);

                if (guild == null) continue;
                if (!(guild.GetChannel(guild.DefaultChannel.Id) is IMessageChannel channel)) continue;

                var allUserWithBirthday = MySqlCommends.GetAllUsersInGuildByBirthday(guild.Id);
                try
                {
                    foreach (var user in allUserWithBirthday)
                    {
                        await ClientEvents.Log(new LogMessage(LogSeverity.Info, "Timer",
                            guild.GetUser(user).Username + " had Birthday"));
                        await channel.SendMessageAsync($"Alles gute zum Geburtstag {guild.GetUser(user).Mention}");
                    }
                }
                catch (Discord.Net.HttpException ex)
                {
                    await ClientEvents.Log(new LogMessage(LogSeverity.Error, guild.Name, ex.Message));
                }
            }

            

            //new Day
            _lastTimeCheck = DateTime.Today;
        }
    }
}
