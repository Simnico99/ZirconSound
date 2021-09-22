using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZirconSound.Enum
{
    public enum AudioState 
    { 
        BotIsInVoiceChannel,
        BotIsNotInVoiceChannel,
        BotAndUserInSameVoiceChannel,
        BotAndUserNotInSameVoiceChannel,
        UserIsInVoiceChannel,
        UserIsNotInVoiceChannel,
        MusicIsPlaying,
        MusicIsNotPlaying,
        MusicIsPaused,
        MusicIsNotPaused,
        QueueIsEmpty,
        QueueIsNotEmpty
    }

    public enum ZirconEmbedType
    {
        Info,
        Warning,
        Error,
    }
}
