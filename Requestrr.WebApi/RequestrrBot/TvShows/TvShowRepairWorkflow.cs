using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    /// <summary>
    /// /repair workflow for TV shows.  Mirrors the shape of
    /// <see cref="TvShowIssueWorkflow"/>.  Supports series-, season-, or
    /// episode-level scoping driven by optional season/episode parameters
    /// originally passed on the slash command.
    /// </summary>
    public class TvShowRepairWorkflow
    {
        private readonly TvShowUserRequester _user;
        private readonly int _categoryId;
        private readonly ITvShowSearcher _searcher;
        private readonly ITvShowRepairer _repairer;
        private readonly ITvShowUserInterface _userInterface;
        private readonly RepairSettings _repairSettings;
        private readonly int? _initialSeason;
        private readonly int? _initialEpisode;

        public TvShowRepairWorkflow(
            TvShowUserRequester user,
            int categoryId,
            ITvShowSearcher searcher,
            ITvShowRepairer repairer,
            ITvShowUserInterface userInterface,
            RepairSettings repairSettings,
            int? initialSeason = null,
            int? initialEpisode = null)
        {
            _user = user;
            _categoryId = categoryId;
            _searcher = searcher;
            _repairer = repairer;
            _userInterface = userInterface;
            _repairSettings = repairSettings ?? new RepairSettings();
            _initialSeason = initialSeason;
            _initialEpisode = initialEpisode;
        }

        public async Task SearchExistingTvShowAsync(string tvShowName)
        {
            if (!_repairSettings.Enabled || _repairer == null)
            {
                await _userInterface.WarnRepairDisabledAsync();
                return;
            }

            var request = new TvShowRequest(_user, _categoryId);
            tvShowName = (tvShowName ?? string.Empty).Replace(".", " ");

            var shows = await _repairer.SearchExistingTvShowAsync(request, tvShowName);

            if (shows == null || !shows.Any())
            {
                await _userInterface.WarnNoTvShowFoundAsync(tvShowName);
                return;
            }

            if (shows.Count == 1)
            {
                // Route the single-hit case through HandleSelectionAsync so the season/episode
                // pickers are shown when the user did not provide those args up-front.
                await HandleSelectionAsync(shows.Single().TheTvDbId);
                return;
            }

            await _userInterface.ShowTvShowRepairSelection(request, shows, _initialSeason, _initialEpisode);
        }

        public async Task HandleSelectionAsync(int theTvDbId)
        {
            // Power-user shortcut: if the slash command provided a season (and/or episode),
            // skip the relevant picker(s) and head straight to confirmation.
            if (_initialSeason.HasValue && _initialEpisode.HasValue)
            {
                await DisplayConfirmationAsync(theTvDbId);
                return;
            }

            if (_initialSeason.HasValue)
            {
                await ShowEpisodePickerAsync(theTvDbId, _initialSeason.Value);
                return;
            }

            await ShowSeasonPickerAsync(theTvDbId);
        }

        public async Task HandleSeasonSelectionAsync(int theTvDbId, int seasonNumber)
        {
            // After picking a season we always show the episode picker so the user can
            // narrow further or fall back to a whole-season repair.
            await ShowEpisodePickerAsync(theTvDbId, seasonNumber);
        }

        public async Task HandleEpisodeSelectionAsync(int theTvDbId, int seasonNumber, int? episodeNumber)
        {
            // episodeNumber == null means the user picked "Whole Season".
            await DisplayConfirmationAsync(theTvDbId, seasonNumber, episodeNumber);
        }

        private async Task ShowSeasonPickerAsync(int theTvDbId)
        {
            if (!_repairSettings.Enabled || _repairer == null)
            {
                await _userInterface.WarnRepairDisabledAsync();
                return;
            }

            var request = new TvShowRequest(_user, _categoryId);
            var tvShow = await GetTvShowOrFallbackAsync(request, theTvDbId);
            if (tvShow == null)
            {
                await _userInterface.WarnNoTvShowFoundByTvDbIdAsync(theTvDbId);
                return;
            }

            var seasons = await _repairer.GetSeasonsWithFilesAsync(request, theTvDbId);
            if (seasons == null || !seasons.Any())
            {
                await _userInterface.WarnRepairNoFilesAsync(tvShow);
                return;
            }

            await _userInterface.ShowSeasonSelectionForRepairAsync(request, tvShow, seasons);
        }

        private async Task ShowEpisodePickerAsync(int theTvDbId, int seasonNumber)
        {
            if (!_repairSettings.Enabled || _repairer == null)
            {
                await _userInterface.WarnRepairDisabledAsync();
                return;
            }

            var request = new TvShowRequest(_user, _categoryId);
            var tvShow = await GetTvShowOrFallbackAsync(request, theTvDbId);
            if (tvShow == null)
            {
                await _userInterface.WarnNoTvShowFoundByTvDbIdAsync(theTvDbId);
                return;
            }

            var episodes = await _repairer.GetEpisodesWithFilesAsync(request, theTvDbId, seasonNumber);
            if (episodes == null || !episodes.Any())
            {
                await _userInterface.WarnRepairNoFilesInSeasonAsync(tvShow, seasonNumber);
                return;
            }

            await _userInterface.ShowEpisodeSelectionForRepairAsync(request, tvShow, seasonNumber, episodes);
        }

        private async Task<TvShow> GetTvShowOrFallbackAsync(TvShowRequest request, int theTvDbId)
        {
            var existing = await _repairer.SearchExistingTvShowAsync(request, theTvDbId);
            if (existing == null) return null;

            try
            {
                var tvShow = await _searcher.GetTvShowDetailsAsync(request, theTvDbId);
                if (tvShow != null) return tvShow;
            }
            catch { /* fall through */ }

            return new TvShow
            {
                TheTvDbId = existing.TheTvDbId,
                Title = existing.Title,
                FirstAired = existing.FirstAired,
                Banner = existing.Banner,
                Seasons = Array.Empty<TvSeason>(),
            };
        }

        private Task DisplayConfirmationAsync(int theTvDbId)
        {
            return DisplayConfirmationAsync(theTvDbId, _initialSeason, _initialEpisode);
        }

        private async Task DisplayConfirmationAsync(int theTvDbId, int? seasonNumber, int? episodeNumber)
        {
            if (!_repairSettings.Enabled || _repairer == null)
            {
                await _userInterface.WarnRepairDisabledAsync();
                return;
            }

            var request = new TvShowRequest(_user, _categoryId);
            var existing = await _repairer.SearchExistingTvShowAsync(request, theTvDbId);

            if (existing == null)
            {
                await _userInterface.WarnNoTvShowFoundByTvDbIdAsync(theTvDbId);
                return;
            }

            // Use the searcher to fetch full TvShow details for the embed.
            TvShow tvShow = null;
            try
            {
                tvShow = await _searcher.GetTvShowDetailsAsync(request, theTvDbId);
            }
            catch
            {
                // fallback: synthesize a minimal TvShow from the searched TvShow
            }

            if (tvShow == null)
            {
                tvShow = new TvShow
                {
                    TheTvDbId = existing.TheTvDbId,
                    Title = existing.Title,
                    FirstAired = existing.FirstAired,
                    Banner = existing.Banner,
                    Seasons = Array.Empty<TvSeason>(),
                };
            }

            await _userInterface.DisplayTvShowRepairConfirmationAsync(request, tvShow, seasonNumber, episodeNumber, _repairSettings.DeleteFileBeforeReSearch);
        }

        public async Task ConfirmRepairAsync(int theTvDbId, int? seasonNumber, int? episodeNumber)
        {
            if (!_repairSettings.Enabled || _repairer == null)
            {
                await _userInterface.WarnRepairDisabledAsync();
                return;
            }

            var request = new TvShowRequest(_user, _categoryId);

            TvShow tvShow = null;
            try
            {
                tvShow = await _searcher.GetTvShowDetailsAsync(request, theTvDbId);
            }
            catch
            {
                tvShow = new TvShow { TheTvDbId = theTvDbId, Title = $"TVDB {theTvDbId}" };
            }

            var result = await _repairer.RepairTvShowAsync(request, theTvDbId, seasonNumber, episodeNumber, _repairSettings.DeleteFileBeforeReSearch);

            if (result != null && result.Success)
            {
                await _userInterface.DisplayTvShowRepairSuccessAsync(tvShow, seasonNumber, episodeNumber, result);
            }
            else
            {
                await _userInterface.DisplayTvShowRepairFailedAsync(tvShow, seasonNumber, episodeNumber, result);
            }
        }

        public async Task CancelAsync(int theTvDbId, int? seasonNumber, int? episodeNumber)
        {
            var request = new TvShowRequest(_user, _categoryId);
            TvShow tvShow = null;
            try
            {
                tvShow = await _searcher.GetTvShowDetailsAsync(request, theTvDbId);
            }
            catch
            {
                tvShow = new TvShow { TheTvDbId = theTvDbId, Title = $"TVDB {theTvDbId}" };
            }

            await _userInterface.DisplayTvShowRepairCancelledAsync(tvShow, seasonNumber, episodeNumber);
        }
    }
}
