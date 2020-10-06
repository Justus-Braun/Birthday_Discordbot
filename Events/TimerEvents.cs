using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using Discord;

namespace Birthday_Discordbot
{
    public static class TimerEvents
    {
        private static DateTime _lastTimeCheck = DateTime.Today.Subtract(TimeSpan.FromDays(1));

        public static async void Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime currentDay = DateTime.Today;
            if (currentDay == _lastTimeCheck) return;

            await ClientEvents.Log(new LogMessage(LogSeverity.Info, "Timer", "New Day"));

            var guild = Program.Client.GetGuild(484297678159609856);
            var channel = guild.GetChannel(761982258562465802) as IMessageChannel;
            var author = await channel.GetUserAsync(358214265041321984);

            await channel.SendMessageAsync(author.Username);

            //new Day
            _lastTimeCheck = DateTime.Today;
        }
    }
}
