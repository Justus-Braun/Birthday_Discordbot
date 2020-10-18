using System;
using System.Timers;
using Birthday_Discordbot.MySql;
using Discord;
using Discord.Net;

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
                //Gets Guild
                var guild = Program.Client.GetGuild(variable);
            
                if (guild == null) continue;
                if (!(guild.GetChannel(MySqlCommends.GetChannel(guild.Id)) is IMessageChannel channel)) continue;

                //Get all Users who have birthday in this Guild
                var allUserWithBirthday = MySqlCommends.GetAllUsersInGuildByBirthday(guild.Id);
                try
                {
                    foreach (var user in allUserWithBirthday)
                    {
                        await ClientEvents.Log(new LogMessage(LogSeverity.Info, "Timer",
                            guild.GetUser(user).Username + " had Birthday"));
                        try
                        {
                            await channel.SendMessageAsync($"Alles gute zum Geburtstag {guild.GetUser(user).Mention}");
                        }
                        catch (HttpException ex)
                        {
                            await ClientEvents.Log(new LogMessage(LogSeverity.Error, guild.Name, ex.Message));
                            MySqlCommends.SetChannel(guild.Id, guild.DefaultChannel.Id);
                            await guild.Owner.SendMessageAsync(
                                $"Auf ihrem Server {guild.Name} wurde der Standart Channel auf ihren default Channel gesetzt");
                        }
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
