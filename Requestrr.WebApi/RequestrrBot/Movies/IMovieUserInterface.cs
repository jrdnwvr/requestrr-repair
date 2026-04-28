using System.Collections.Generic;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public interface IMovieUserInterface
    {
        Task ShowMovieSelection(MovieRequest request, IReadOnlyList<Movie> movies);
        Task ShowMovieIssueSelection(MovieRequest request, IReadOnlyList<Movie> movies);
        Task WarnNoMovieFoundByTheMovieDbIdAsync(string theMovieDbIdTextValue);
        Task WarnNoMovieFoundAsync(string movieName);
        Task WarnMovieAlreadyAvailableAsync(Movie movie);
        Task DisplayMovieDetailsAsync(MovieRequest request, Movie movie);
        Task DisplayMovieIssueDetailsAsync(MovieRequest request, Movie movie, string issue);
        Task DisplayMovieIssueModalAsync(MovieRequest request, Movie movie, string issue);
        Task CompleteMovieIssueModalRequestAsync(Movie movie, bool success);
        Task DisplayRequestDeniedAsync(Movie movie);
        Task DisplayRequestSuccessAsync(Movie movie);
        Task WarnMovieUnavailableAndAlreadyHasNotificationAsync(Movie movie);
        Task WarnMovieAlreadyRequestedAsync(Movie movie);
        Task DisplayNotificationSuccessAsync(Movie movie);
        Task AskForNotificationRequestAsync(Movie movie);

        // /repair workflow methods
        Task ShowMovieRepairSelection(MovieRequest request, IReadOnlyList<Movie> movies);
        Task DisplayMovieRepairConfirmationAsync(MovieRequest request, Movie movie, bool deleteFiles);
        Task DisplayMovieRepairSuccessAsync(Movie movie, MovieRepairResult result);
        Task DisplayMovieRepairFailedAsync(Movie movie, MovieRepairResult result);
        Task DisplayMovieRepairCancelledAsync(Movie movie);
        Task WarnRepairDisabledAsync();
    }
}