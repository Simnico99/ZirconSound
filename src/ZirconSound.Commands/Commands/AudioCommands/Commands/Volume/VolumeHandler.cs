using Discord;
using Lavalink4NET;
using Mediator;
using ZirconSound.Core.Enums;
using ZirconSound.Core.Extensions;
using ZirconSound.Core.Helpers;

namespace ZirconSound.Application.Commands.AudioCommands.Commands.Volume;

public sealed class VolumeHandler : ICommandHandler<VolumeCommand>
{
    private readonly IAudioService _audioService;

    public VolumeHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public async ValueTask<Unit> Handle(VolumeCommand command, CancellationToken cancellationToken)
    {
        var embed = EmbedHelpers.CreateGenericEmbedBuilder(command.Context);
        var player = await _audioService.GetPlayerAndSetContextAsync(command.Context.Guild.Id, command.Context);
        var embedType = GenericEmbedType.Info;
        var volume = command.Volume;

        if (command.VolumeButton.HasValue)
        {
            volume = (player!.Volume * 100) + command.VolumeButton.Value;
        }

        volume = (float)Math.Round(volume, 2);

        if (volume is > 1000 or < 1)
        {
            embed.AddField("Warning:", $"The volume can only be set between 1% and 1000%.");
            await command.Context.ReplyToLastCommandAsync(embed: embed.Build(GenericEmbedType.Warning));
            return Unit.Value;
        }

        if (volume is > 100)
        {
            embedType = GenericEmbedType.Warning;
            embed.AddField("Warning:", $"A volume above 100% might be distorted.");
        }

        var buttons = new ComponentBuilder()
            .WithButton("-5%", "volume-button-minus5", ButtonStyle.Secondary, disabled: (volume - 5) < 1)
            .WithButton("-1%", "volume-button-minus1", ButtonStyle.Secondary, disabled: (volume - 1) < 1)
            .WithButton("50%", "volume-button-set50", disabled: volume == 50)
            .WithButton("+1%", "volume-button-plus1", ButtonStyle.Secondary, disabled: (volume + 1) > 1000)
            .WithButton("+5%", "volume-button-plus5", ButtonStyle.Secondary, disabled: (volume + 5) > 1000);

        await player!.SetVolumeAsync(volume / 100f, cancellationToken);

        embed.AddField("Volume changed:", $"The volume has been set to {volume}%.");

        await command.Context.ReplyToLastCommandAsync(embed: embed.Build(embedType), component: buttons.Build());

        return Unit.Value;
    }
}