namespace ZirconSound.Common.Enum;

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
    Debug,
    Info,
    Warning,
    Error
}

public enum InteractionsType
{
    SlashCommand,
    MessageComponent
}
