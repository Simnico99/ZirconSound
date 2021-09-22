using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZirconSound.Services;

namespace ZirconSound.Modules
{
    public class GeneralCommand : ModuleBase<SocketCommandContext>
    {

        [Command("ping")]
        public async Task Ping() 
        {
            await Context.Channel.SendMessageAsync("PONG!");
        }

        [Command("help")]
        public async Task Help()
        {
            var embed = new EmbedBuilder
            {
                Title = "Commands",
                Description = "The current commands you can use with ZirconSound.",
                Color = Color.DarkRed,
                Timestamp = DateTime.Now,
            };

            embed.AddField("Ping","Ping the bot should answer with 'PONG!'");
            embed.AddField("Leave", "The bot will leave your current channel.");
            embed.AddField("Play (p)", "Play the specified video and will join your current channel. (Can use a youtube link or just the title.)");
            embed.AddField("Skip (next, s)", "Will skip to the next song in the queue.");
            embed.AddField("Stop", "Will stop the current song.");
            embed.AddField("Pause", "Will pause the current song.");
            embed.AddField("Resume", "Will resume the current paused song.");
            embed.AddField("Queue {Page Number}", "Will show you the current song queue.");
            embed.AddField("Clear", "Will clear the song queue.");
            embed.AddField("Replay", "Will replay the current song.");
            embed.AddField("Seek {Time}", "Will seek to the that time in the track.\n{00:00:00(Hours:Minutes:Seconds)}");
            await ReplyAsync(embed: embed.Build());
        }
    }
}
