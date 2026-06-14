using DSharpPlus;
using DSharpPlus.Entities;
using Requestrr.WebApi.RequestrrBot.Locale;
using Requestrr.WebApi.RequestrrBot.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Requestrr.WebApi.RequestrrBot.ChatClients.Discord
{
    public class DiscordMusicUserInterface : IMusicUserInterface
    {
        private readonly DiscordInteraction _interactionContext;
        private readonly IMusicSearcher _musicSearcher;

        public DiscordMusicUserInterface(
            DiscordInteraction interactionContext,
            IMusicSearcher musicSearcher)
        {
            _interactionContext = interactionContext;
            _musicSearcher = musicSearcher;
        }


        public async Task ShowMusicArtistSelection(MusicRequest request, IReadOnlyList<MusicArtist> music)
        {
            List<DiscordSelectComponentOption> options = music.Take(15).Select(x => new DiscordSelectComponentOption(GetFormattedMusicArtistName(x), $"{request.CategoryId}/{x.ArtistId}")).ToList();
            DiscordSelectComponent select = new DiscordSelectComponent($"MuRSA/{_interactionContext.User.Id}/{request.CategoryId}", LimitStringSize(Language.Current.DiscordCommandMusicArtistRequestHelpDropdown), options);

            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddComponents(select).WithContent(Language.Current.DiscordCommandMusicArtistRequestHelp));
        }



        public async Task DisplayMusicArtistDetailsAsync(MusicRequest request, MusicArtist musicArtist)
        {
            string message = Language.Current.DiscordCommandMusicArtistRequestConfirm;
            DiscordButtonComponent requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"MuRCA/{_interactionContext.User.Id}/{request.CategoryId}/{musicArtist.ArtistId}", Language.Current.DiscordCommandRequestButton);

            var builder = (await AddPreviousDropdownsAsync(musicArtist, new DiscordWebhookBuilder().AddEmbed(GenerateMusicArtistDetails(musicArtist)))).AddComponents(requestButton).WithContent(message);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }


        public async Task WarnMusicArtistAlreadyAvailableAsync(MusicArtist musicArtist)
        {
            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"MMU/{_interactionContext.User.Id}/{musicArtist.ArtistId}", Language.Current.DiscordCommandAvailableButton, true);
            var builder = (await AddPreviousDropdownsAsync(musicArtist, new DiscordWebhookBuilder().AddEmbed(GenerateMusicArtistDetails(musicArtist)))).AddComponents(requestButton).WithContent(Language.Current.DiscordCommandMusicArtistAlreadyAvailable);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }


        public async Task WarnNoMusicArtistFoundAsync(string musicArtistName)
        {
            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(Language.Current.DiscordCommandMusicArtistNotFound.ReplaceTokens(LanguageTokens.MusicArtistName, musicArtistName)));
        }



        public static DiscordEmbed GenerateMusicArtistDetails(MusicArtist musicArtist)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithTitle(musicArtist.ArtistName)
                .WithTimestamp(DateTime.Now)
                .WithUrl($"https://musicbrainz.org/artist/{musicArtist.ArtistId}")
                .WithFooter("Powered by Requestrr");

            if (!string.IsNullOrWhiteSpace(musicArtist.Overview))
                embedBuilder.WithDescription(musicArtist.Overview.Substring(0, Math.Min(musicArtist.Overview.Length, 255)) + "(...)");

            if (!string.IsNullOrWhiteSpace(musicArtist.PosterPath) && musicArtist.PosterPath.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                embedBuilder.WithImageUrl(musicArtist.PosterPath);

            if (!string.IsNullOrWhiteSpace(musicArtist.Quality))
                embedBuilder.AddField($"__{Language.Current.DiscordEmbedMusicQuality}__", $"{musicArtist.Quality}", true);

            if (!string.IsNullOrWhiteSpace(musicArtist.PlexUrl))
                embedBuilder.AddField($"__Plex__", $"[{Language.Current.DiscordEmbedMusicListenNow}]({musicArtist.PlexUrl})", true);

            if (!string.IsNullOrWhiteSpace(musicArtist.EmbyUrl))
                embedBuilder.AddField($"__Emby__", $"[{Language.Current.DiscordEmbedMusicListenNow}]({musicArtist.EmbyUrl})", true);

            return embedBuilder.Build();
        }


        public async Task DisplayArtistRequestSuccessAsync(MusicArtist musicArtist)
        {
            DiscordButtonComponent successButton = new DiscordButtonComponent(ButtonStyle.Success, $"0/1/0", Language.Current.DiscordCommandRequestButtonSuccess);
            DiscordWebhookBuilder builder = (await AddPreviousDropdownsAsync(musicArtist, new DiscordWebhookBuilder().AddEmbed(GenerateMusicArtistDetails(musicArtist)))).AddComponents(successButton).WithContent(Language.Current.DiscordCommandMusicArtistRequestSuccess.ReplaceTokens(musicArtist));            
            await _interactionContext.EditOriginalResponseAsync(builder);
        }



        public async Task DisplayArtistRequestDeniedAsync(MusicArtist musicArtist)
        {
            DiscordButtonComponent deniedButton = new DiscordButtonComponent(ButtonStyle.Danger, $"0/1/0", Language.Current.DiscordCommandRequestButtonDenied);
            DiscordWebhookBuilder builder = (await AddPreviousDropdownsAsync(musicArtist, new DiscordWebhookBuilder().AddEmbed(GenerateMusicArtistDetails(musicArtist)))).AddComponents(deniedButton).WithContent(Language.Current.DiscordCommandMusicArtistRequestDenied);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }



        public async Task WarnMusicArtistUnavailableAndAlreadyHasNotificationAsync(MusicArtist musicArtist)
        {
            DiscordButtonComponent requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"MMU/{_interactionContext.User.Id}/{musicArtist.ArtistId}", Language.Current.DiscordCommandRequestButton, true);
            DiscordWebhookBuilder builder = (await AddPreviousDropdownsAsync(musicArtist, new DiscordWebhookBuilder().AddEmbed(GenerateMusicArtistDetails(musicArtist)))).AddComponents(requestButton).WithContent(Language.Current.DiscordCommandMusicArtistRequestAlreadyExistNotified);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }



        public async Task AskForNotificationArtistRequestAsync(MusicArtist musicArtist)
        {
            var notificationButton = new DiscordButtonComponent(ButtonStyle.Primary, $"MuNR/{_interactionContext.User.Id}/{musicArtist.ArtistId}", Language.Current.DiscordCommandRequestButton, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔔")));
            DiscordWebhookBuilder builder = (await AddPreviousDropdownsAsync(musicArtist, new DiscordWebhookBuilder().AddEmbed(GenerateMusicArtistDetails(musicArtist)))).AddComponents(notificationButton).WithContent(Language.Current.DiscordCommandMusicArtistNotificationRequest);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }


        public async Task DisplayNotificationArtistSuccessAsync(MusicArtist musicArtist)
        {
            DiscordButtonComponent successButton = new DiscordButtonComponent(ButtonStyle.Success, $"0/1/0", Language.Current.DiscordCommandNotifyMeSuccess);
            DiscordWebhookBuilder builder = (await AddPreviousDropdownsAsync(musicArtist, new DiscordWebhookBuilder().AddEmbed(GenerateMusicArtistDetails(musicArtist)))).AddComponents(successButton).WithContent(Language.Current.DiscordCommandMusicArtistNotificationSuccess.ReplaceTokens(musicArtist));
            await _interactionContext.EditOriginalResponseAsync(builder);
        }


        // ---------------- Album-level requesting ----------------

        public async Task ShowMusicAlbumArtistSelection(MusicRequest request, IReadOnlyList<MusicArtist> artists)
        {
            List<DiscordSelectComponentOption> options = artists.Take(15).Select(x => new DiscordSelectComponentOption(GetFormattedMusicArtistName(x), $"{request.CategoryId}/{x.ArtistId}")).ToList();
            DiscordSelectComponent select = new DiscordSelectComponent($"MuABA/{_interactionContext.User.Id}/{request.CategoryId}", LimitStringSize("Select an artist..."), options);

            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddComponents(select).WithContent("Which artist did you mean? Pick one to see their albums:"));
        }


        public async Task ShowMusicAlbumSelection(MusicRequest request, IReadOnlyList<MusicAlbum> albums)
        {
            List<DiscordSelectComponentOption> options = albums.Take(15).Select(x => new DiscordSelectComponentOption(GetFormattedMusicAlbumName(x), $"{request.CategoryId}/{x.AlbumId}")).ToList();
            DiscordSelectComponent select = new DiscordSelectComponent($"MuASA/{_interactionContext.User.Id}/{request.CategoryId}", LimitStringSize("Select an album..."), options);

            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddComponents(select).WithContent("Here are the albums I found, which one would you like?"));
        }


        public async Task WarnNoMusicAlbumFoundAsync(string albumName)
        {
            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"I could not find any album matching **{albumName}**."));
        }


        public async Task DisplayMusicAlbumDetailsAsync(MusicRequest request, MusicAlbum album)
        {
            DiscordButtonComponent requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"MuACA/{_interactionContext.User.Id}/{request.CategoryId}/{album.AlbumId}", Language.Current.DiscordCommandRequestButton);
            var builder = (await AddPreviousAlbumDropdownsAsync(album, new DiscordWebhookBuilder().AddEmbed(GenerateMusicAlbumDetails(album)))).AddComponents(requestButton).WithContent("Do you want to request this album?");
            await _interactionContext.EditOriginalResponseAsync(builder);
        }


        public async Task WarnMusicAlbumAlreadyAvailableAsync(MusicAlbum album)
        {
            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"0/1/0", Language.Current.DiscordCommandAvailableButton, true);
            var builder = (await AddPreviousAlbumDropdownsAsync(album, new DiscordWebhookBuilder().AddEmbed(GenerateMusicAlbumDetails(album)))).AddComponents(requestButton).WithContent("Good news! This album is already available. 🎶");
            await _interactionContext.EditOriginalResponseAsync(builder);
        }


        public async Task DisplayAlbumRequestSuccessAsync(MusicAlbum album)
        {
            DiscordButtonComponent successButton = new DiscordButtonComponent(ButtonStyle.Success, $"0/1/0", Language.Current.DiscordCommandRequestButtonSuccess);
            DiscordWebhookBuilder builder = (await AddPreviousAlbumDropdownsAsync(album, new DiscordWebhookBuilder().AddEmbed(GenerateMusicAlbumDetails(album)))).AddComponents(successButton).WithContent("✅ Your album request has been received!");
            await _interactionContext.EditOriginalResponseAsync(builder);
        }


        public async Task DisplayAlbumRequestDeniedAsync(MusicAlbum album)
        {
            DiscordButtonComponent deniedButton = new DiscordButtonComponent(ButtonStyle.Danger, $"0/1/0", Language.Current.DiscordCommandRequestButtonDenied);
            DiscordWebhookBuilder builder = (await AddPreviousAlbumDropdownsAsync(album, new DiscordWebhookBuilder().AddEmbed(GenerateMusicAlbumDetails(album)))).AddComponents(deniedButton).WithContent("Your album request was denied.");
            await _interactionContext.EditOriginalResponseAsync(builder);
        }


        public static DiscordEmbed GenerateMusicAlbumDetails(MusicAlbum album)
        {
            string title = string.IsNullOrWhiteSpace(album.ArtistName) ? album.AlbumTitle : $"{album.ArtistName} - {album.AlbumTitle}";
            if (title.Length > 250)
                title = title.Substring(0, 250);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithTitle(title)
                .WithTimestamp(DateTime.Now)
                .WithFooter("Powered by Requestrr");

            if (!string.IsNullOrWhiteSpace(album.AlbumId))
                embedBuilder.WithUrl($"https://musicbrainz.org/release-group/{album.AlbumId}");

            if (!string.IsNullOrWhiteSpace(album.Overview))
                embedBuilder.WithDescription(album.Overview.Substring(0, Math.Min(album.Overview.Length, 255)) + "(...)");

            if (!string.IsNullOrWhiteSpace(album.PosterPath) && album.PosterPath.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                embedBuilder.WithImageUrl(album.PosterPath);

            if (!string.IsNullOrWhiteSpace(album.ReleaseDate))
            {
                string year = album.ReleaseDate.Length >= 4 ? album.ReleaseDate.Substring(0, 4) : album.ReleaseDate;
                embedBuilder.AddField("__Year__", year, true);
            }

            return embedBuilder.Build();
        }



        private string GetFormattedMusicArtistName(MusicArtist music)
        {
            // Prefer real years active; fall back to the curated disambiguation blurb so the
            // parenthetical always helps tell same-named artists apart.
            string hint = !string.IsNullOrWhiteSpace(music.YearsActive)
                ? music.YearsActive
                : (!string.IsNullOrWhiteSpace(music.Disambiguation) ? music.Disambiguation : null);

            string name = hint != null ? $"{music.ArtistName} ({hint})" : music.ArtistName;
            return LimitStringSize(name);
        }
        private string GetFormattedMusicAlbumName(MusicAlbum album)
        {
            // The album picker is always scoped to one already-chosen artist, so lead with the
            // title and append the year to disambiguate reissues/versions.
            string year = !string.IsNullOrWhiteSpace(album.ReleaseDate) && album.ReleaseDate.Length >= 4
                ? album.ReleaseDate.Substring(0, 4)
                : null;
            string name = year != null ? $"{album.AlbumTitle} ({year})" : album.AlbumTitle;
            return LimitStringSize(name);
        }
        private string LimitStringSize(string value, int limit = 100)
        {
            return value.Count() > limit ? value[..(limit - 3)] + "..." : value;
        }


        private async Task<DiscordWebhookBuilder> AddPreviousDropdownsAsync(MusicArtist music, DiscordWebhookBuilder builder)
        {
            DiscordSelectComponent previousMusicSelector = (await _interactionContext.GetOriginalResponseAsync()).FilterComponents<DiscordSelectComponent>().FirstOrDefault();
            if (previousMusicSelector != null)
            {
                DiscordSelectComponent musicSelector = new DiscordSelectComponent(previousMusicSelector.CustomId, GetFormattedMusicArtistName(music), previousMusicSelector.Options);
                builder.AddComponents(musicSelector);
            }

            return builder;
        }


        private async Task<DiscordWebhookBuilder> AddPreviousAlbumDropdownsAsync(MusicAlbum album, DiscordWebhookBuilder builder)
        {
            DiscordSelectComponent previousMusicSelector = (await _interactionContext.GetOriginalResponseAsync()).FilterComponents<DiscordSelectComponent>().FirstOrDefault();
            if (previousMusicSelector != null)
            {
                DiscordSelectComponent musicSelector = new DiscordSelectComponent(previousMusicSelector.CustomId, GetFormattedMusicAlbumName(album), previousMusicSelector.Options);
                builder.AddComponents(musicSelector);
            }

            return builder;
        }
    }
}
