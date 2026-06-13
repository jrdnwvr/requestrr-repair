using DSharpPlus.SlashCommands;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    /// <summary>
    /// Chosen on the /music command: request a whole artist (all albums) or a single album.
    /// Default is Album.
    /// </summary>
    public enum MusicRequestScope
    {
        [ChoiceName("album")]
        Album = 0,

        [ChoiceName("artist")]
        Artist = 1
    }
}
