using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    public interface IMusicSearcher
    {
        Task<IReadOnlyList<MusicArtist>> SearchMusicForArtistAsync(MusicRequest request, string artistName);
        Task<MusicArtist> SearchMusicForArtistIdAsync(MusicRequest request, string artistId);

        Task<IReadOnlyList<MusicAlbum>> SearchMusicForAlbumAsync(MusicRequest request, string albumName);
        Task<MusicAlbum> SearchMusicForAlbumIdAsync(MusicRequest request, string albumId);
        Task<IReadOnlyList<MusicAlbum>> GetMusicArtistDiscographyAsync(MusicRequest request, string artistId);


        Task<Dictionary<string, MusicArtist>> SearchAvailableMusicArtistAsync(HashSet<string> artistIds, CancellationToken token);
    }
}
