namespace Requestrr.WebApi.RequestrrBot.Music
{
    public class MusicArtist
    {
        public string DownloadClientId { get; set; }
        public string ArtistId { get; set; }
        public string ArtistName { get; set; }
        public string Overview { get; set; }

        // Hints for telling same-named artists apart in the picker.
        public string Disambiguation { get; set; } // curated MusicBrainz blurb, e.g. "1980s–1990s US grunge band"
        public string YearsActive { get; set; }    // from MusicBrainz life-span, e.g. "1988–1994"


        public bool Available { get; set; }
        public bool Monitored { get; set; }
        public string Quality { get; set; }
        public bool Requested { get; set; }


        public string PlexUrl { get; set; }
        public string EmbyUrl { get; set; }

        public string PosterPath { get; set; }
    }
}
