using System.Collections.Generic;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    /// <summary>
    /// Implemented by download clients (currently Sonarr) that can re-download
    /// existing TV episode files by deleting them and triggering a fresh
    /// search.  Supports series-, season-, or episode-level scoping.
    /// </summary>
    public interface ITvShowRepairer
    {
        /// <summary>
        /// Searches the existing library for TV shows matching the title.
        /// Only returns shows that already exist in the library and are
        /// eligible for repair.
        /// </summary>
        Task<IReadOnlyList<SearchedTvShow>> SearchExistingTvShowAsync(TvShowRequest request, string tvShowName);

        /// <summary>
        /// Looks up an existing TV show in the library by its TheTvDb id.
        /// Returns null when the show is not present in the library.
        /// </summary>
        Task<SearchedTvShow> SearchExistingTvShowAsync(TvShowRequest request, int theTvDbId);

        /// <summary>
        /// Deletes the relevant episode file(s) for the TV show (when
        /// <paramref name="deleteFiles"/> is true) and triggers a fresh
        /// search.  When <paramref name="seasonNumber"/> is null the full
        /// series is repaired.  When only <paramref name="seasonNumber"/> is
        /// provided the entire season is repaired.  When both are provided
        /// only the single episode is repaired.
        /// </summary>
        Task<TvShowRepairResult> RepairTvShowAsync(TvShowRequest request, int theTvDbId, int? seasonNumber, int? episodeNumber, bool deleteFiles);

        /// <summary>
        /// Returns the season numbers for the given series that currently
        /// have at least one downloaded episode file.  Used to populate the
        /// interactive season picker in the /repair flow.
        /// </summary>
        Task<IReadOnlyList<int>> GetSeasonsWithFilesAsync(TvShowRequest request, int theTvDbId);

        /// <summary>
        /// Returns the episodes in the given season that currently have a
        /// downloaded file.  Used to populate the interactive episode
        /// picker in the /repair flow.
        /// </summary>
        Task<IReadOnlyList<RepairableEpisode>> GetEpisodesWithFilesAsync(TvShowRequest request, int theTvDbId, int seasonNumber);
    }

    public class TvShowRepairResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int FilesDeleted { get; set; }
        public bool SearchTriggered { get; set; }
    }

    /// <summary>
    /// Lightweight episode descriptor used to render the /repair episode
    /// picker.  Only the fields needed for the dropdown label are populated.
    /// </summary>
    public class RepairableEpisode
    {
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }
        public string AirDate { get; set; }
    }
}
