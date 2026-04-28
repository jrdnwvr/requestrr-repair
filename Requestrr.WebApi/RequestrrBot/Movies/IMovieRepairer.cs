using System.Collections.Generic;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Movies
{
    /// <summary>
    /// Implemented by download clients (currently Radarr) that can re-download
    /// an existing movie file by deleting it and triggering a fresh search.
    /// </summary>
    public interface IMovieRepairer
    {
        /// <summary>
        /// Searches the existing library for movies matching the title.  Only
        /// returns movies that already exist in the library (i.e. are eligible
        /// for repair).
        /// </summary>
        Task<IReadOnlyList<Movie>> SearchExistingMovieAsync(MovieRequest request, string movieName);

        /// <summary>
        /// Looks up an existing movie in the library by its TheMovieDb id.
        /// Returns null when the movie is not present in the library.
        /// </summary>
        Task<Movie> SearchExistingMovieAsync(MovieRequest request, int theMovieDbId);

        /// <summary>
        /// Deletes the existing file(s) for the movie (when
        /// <paramref name="deleteFiles"/> is true) and triggers a fresh search.
        /// </summary>
        Task<MovieRepairResult> RepairMovieAsync(MovieRequest request, Movie movie, bool deleteFiles);
    }

    public class MovieRepairResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int FilesDeleted { get; set; }
        public bool SearchTriggered { get; set; }
    }
}
