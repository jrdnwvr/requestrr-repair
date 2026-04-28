using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Movies
{
    /// <summary>
    /// /repair workflow for movies.  Mirrors the shape of
    /// <see cref="MovieIssueWorkflow"/>: search the library, allow the user
    /// to pick a movie, show a confirmation, then delete file + trigger
    /// Radarr search on confirm.
    /// </summary>
    public class MovieRepairWorkflow
    {
        private readonly MovieUserRequester _user;
        private readonly int _categoryId;
        private readonly IMovieSearcher _searcher;
        private readonly IMovieRepairer _repairer;
        private readonly IMovieUserInterface _userInterface;
        private readonly RepairSettings _repairSettings;

        public MovieRepairWorkflow(
            MovieUserRequester user,
            int categoryId,
            IMovieSearcher searcher,
            IMovieRepairer repairer,
            IMovieUserInterface userInterface,
            RepairSettings repairSettings)
        {
            _user = user;
            _categoryId = categoryId;
            _searcher = searcher;
            _repairer = repairer;
            _userInterface = userInterface;
            _repairSettings = repairSettings ?? new RepairSettings();
        }

        public async Task SearchExistingMovieAsync(string movieName)
        {
            if (!_repairSettings.Enabled || _repairer == null)
            {
                await _userInterface.WarnRepairDisabledAsync();
                return;
            }

            var request = new MovieRequest(_user, _categoryId);
            movieName = (movieName ?? string.Empty).Replace(".", " ");

            var movies = await _repairer.SearchExistingMovieAsync(request, movieName);

            if (movies == null || !movies.Any())
            {
                await _userInterface.WarnNoMovieFoundAsync(movieName);
                return;
            }

            if (movies.Count == 1)
            {
                await DisplayConfirmationAsync(movies.Single());
                return;
            }

            await _userInterface.ShowMovieRepairSelection(request, movies);
        }

        public async Task HandleSelectionAsync(int theMovieDbId)
        {
            if (!_repairSettings.Enabled || _repairer == null)
            {
                await _userInterface.WarnRepairDisabledAsync();
                return;
            }

            var request = new MovieRequest(_user, _categoryId);
            var movie = await _repairer.SearchExistingMovieAsync(request, theMovieDbId);

            if (movie == null)
            {
                await _userInterface.WarnNoMovieFoundByTheMovieDbIdAsync(theMovieDbId.ToString());
                return;
            }

            await DisplayConfirmationAsync(movie);
        }

        private async Task DisplayConfirmationAsync(Movie movie)
        {
            await _userInterface.DisplayMovieRepairConfirmationAsync(new MovieRequest(_user, _categoryId), movie, _repairSettings.DeleteFileBeforeReSearch);
        }

        public async Task ConfirmRepairAsync(int theMovieDbId)
        {
            if (!_repairSettings.Enabled || _repairer == null)
            {
                await _userInterface.WarnRepairDisabledAsync();
                return;
            }

            var request = new MovieRequest(_user, _categoryId);
            var movie = await _repairer.SearchExistingMovieAsync(request, theMovieDbId);

            if (movie == null)
            {
                await _userInterface.WarnNoMovieFoundByTheMovieDbIdAsync(theMovieDbId.ToString());
                return;
            }

            var result = await _repairer.RepairMovieAsync(request, movie, _repairSettings.DeleteFileBeforeReSearch);

            if (result != null && result.Success)
            {
                await _userInterface.DisplayMovieRepairSuccessAsync(movie, result);
            }
            else
            {
                await _userInterface.DisplayMovieRepairFailedAsync(movie, result);
            }
        }

        public async Task CancelAsync(int theMovieDbId)
        {
            var request = new MovieRequest(_user, _categoryId);
            Movie movie = null;
            try
            {
                if (_repairer != null)
                    movie = await _repairer.SearchExistingMovieAsync(request, theMovieDbId);
            }
            catch
            {
                // best-effort lookup for the cancellation embed
            }

            await _userInterface.DisplayMovieRepairCancelledAsync(movie);
        }
    }
}
