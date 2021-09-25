using Discord;
using System;
using System.Threading.Tasks;
using ZirconSound.SlashCommands;

namespace ZirconSound.Modules
{
    public class GeneralCommand : SlashModuleBase<SlashCommandContext>
    {

        [SlashCommand("ping", "Ping the bot!")]
        public async Task Ping()
        {
            await Context.Command.FollowupAsync("PONG!");
        }

        [SlashCommand("help", "Show the commands you can execute")]
        public async Task Help()
        {
            var embed = new EmbedBuilder
            {
                Title = "Commands",
                Description = "The current commands you can use with ZirconSound.",
                Color = Color.DarkRed,
                Timestamp = DateTime.Now,
            };

            embed.AddField("Ping", "Ping the bot should answer with 'PONG!'");
            embed.AddField("Leave", "The bot will leave your current channel.");
            embed.AddField("Play", "Play the specified video and will join your current channel. (Can use a youtube link or just the title.)");
            embed.AddField("Skip", "Will skip to the next song in the queue.");
            embed.AddField("Stop", "Will stop the current song.");
            embed.AddField("Pause", "Will pause the current song.");
            embed.AddField("Resume", "Will resume the current paused song.");
            embed.AddField("Queue", "Will show you the current song queue.");
            embed.AddField("Clear", "Will clear the song queue.");
            embed.AddField("Replay", "Will replay the current song.");
            embed.AddField("Seek", "Will seek to the that time in the track.\n{00:00:00(Hours:Minutes:Seconds)}");
            await Context.Command.FollowupAsync(embed: embed.Build());
        }
    }
}
