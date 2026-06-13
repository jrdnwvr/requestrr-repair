namespace Requestrr.WebApi.RequestrrBot.Music
{
    public class MusicAlbum
    {
        // Lidarr internal album id (only known once the album exists in Lidarr).
        public string DownloadClientId { get; set; }

        // MusicBrainz release-group id (foreignAlbumId) used for lookups + interaction routing.
        public string AlbumId { get; set; }
        public string AlbumTitle { get; set; }

        // MusicBrainz artist id (foreignArtistId) + display name.
        public string ArtistId { get; set; }
        public string ArtistName { get; set; }

        public string Overview { get; set; }
        public string ReleaseDate { get; set; }

        public bool Available { get; set; }
        public bool Monitored { get; set; }
        public bool Requested { get; set; }

        public string PosterPath { get; set; }
    }
}
