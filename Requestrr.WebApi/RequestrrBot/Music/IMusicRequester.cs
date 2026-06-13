using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    public interface IMusicRequester
    {
        Task<MusicRequestResult> RequestMusicAsync(MusicRequest request, MusicArtist music);
        Task<MusicRequestResult> RequestMusicAlbumAsync(MusicRequest request, MusicAlbum album);
    }


    public class MusicRequestResult
    {
        public bool WasDenied { get; set; }
    }
}
