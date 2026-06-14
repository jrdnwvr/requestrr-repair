using System.Collections.Generic;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    public interface IMusicUserInterface
    {
        Task ShowMusicArtistSelection(MusicRequest request, IReadOnlyList<MusicArtist> music);
        Task WarnNoMusicArtistFoundAsync(string musicName);

        Task DisplayMusicArtistDetailsAsync(MusicRequest request, MusicArtist music);
        Task DisplayArtistRequestDeniedAsync(MusicArtist music);
        Task DisplayArtistRequestSuccessAsync(MusicArtist music);

        Task WarnMusicArtistAlreadyAvailableAsync(MusicArtist music);

        Task WarnMusicArtistUnavailableAndAlreadyHasNotificationAsync(MusicArtist music);
        Task AskForNotificationArtistRequestAsync(MusicArtist music);
        Task DisplayNotificationArtistSuccessAsync(MusicArtist music);

        // Album-level requesting
        Task ShowMusicAlbumArtistSelection(MusicRequest request, IReadOnlyList<MusicArtist> artists);
        Task ShowMusicAlbumSelection(MusicRequest request, IReadOnlyList<MusicAlbum> albums);
        Task WarnNoMusicAlbumFoundAsync(string albumName);
        Task DisplayMusicAlbumDetailsAsync(MusicRequest request, MusicAlbum album);
        Task DisplayAlbumRequestSuccessAsync(MusicAlbum album);
        Task DisplayAlbumRequestDeniedAsync(MusicAlbum album);
        Task WarnMusicAlbumAlreadyAvailableAsync(MusicAlbum album);
    }
}
