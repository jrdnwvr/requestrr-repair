using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    public class MusicRequestingWorkflow
    {
        private readonly int _categoryId;
        private readonly MusicUserRequester _user;
        private readonly IMusicSearcher _musicSearcher;
        private readonly IMusicRequester _requester;
        private readonly IMusicUserInterface _userInterface;
        private readonly IMusicNotificationWorkflow _notificationWorkflow;


        public MusicRequestingWorkflow(
            MusicUserRequester user,
            int categoryId,
            IMusicSearcher searcher,
            IMusicRequester requester,
            IMusicUserInterface userInterface,
            IMusicNotificationWorkflow notificationWorkflow
        )
        {
            _categoryId = categoryId;
            _user = user;
            _musicSearcher = searcher;
            _requester = requester;
            _userInterface = userInterface;
            _notificationWorkflow = notificationWorkflow;
        }


        public async Task SearchMusicForArtistAsync(string artistName)
        {
            IReadOnlyList<MusicArtist> musicList = await SearchMusicForArtistListAsync(artistName);

            if (musicList.Any())
            {
                if (musicList.Count > 1)
                {
                    await _userInterface.ShowMusicArtistSelection(new MusicRequest(_user, _categoryId), musicList);
                }
                else if (musicList.Count == 1)
                {
                    MusicArtist music = musicList.Single();
                    await HandleMusicSelectionAsync(music);
                }
            }
        }


        public async Task<IReadOnlyList<MusicArtist>> SearchMusicForArtistListAsync(string artistName)
        {
            IReadOnlyList<MusicArtist> music = Array.Empty<MusicArtist>();

            artistName = artistName.Replace(".", " ");
            music = await _musicSearcher.SearchMusicForArtistAsync(new MusicRequest(_user, _categoryId), artistName);

            if (!music.Any())
                await _userInterface.WarnNoMusicArtistFoundAsync(artistName);

            return music;
        }


        public async Task HandleMusicArtistSelectionAsync(string musicArtistId)
        {
            await HandleMusicSelectionAsync(await _musicSearcher.SearchMusicForArtistIdAsync(new MusicRequest(_user, _categoryId), musicArtistId));
        }


        private async Task HandleMusicSelectionAsync(MusicArtist musicArtist)
        {
            if (CanBeRequested(musicArtist))
            {
                await _userInterface.DisplayMusicArtistDetailsAsync(new MusicRequest(_user, _categoryId), musicArtist);
            }
            else
            {
                if (musicArtist.Available)
                {
                    await _userInterface.WarnMusicArtistAlreadyAvailableAsync(musicArtist);
                }
                else
                {
                    await _notificationWorkflow.NotifyForExistingRequestAsync(_user.UserId, musicArtist);
                }
            }
        }



        /// <summary>
        /// Handles the request for an artist
        /// </summary>
        /// <param name="artistId"></param>
        /// <returns></returns>
        public async Task RequestMusicArtistAsync(string artistId)
        {
            MusicArtist musicArtist = await _musicSearcher.SearchMusicForArtistIdAsync(new MusicRequest(_user, _categoryId), artistId);
            MusicRequestResult result = await _requester.RequestMusicAsync(new MusicRequest(_user, _categoryId), musicArtist);

            if (result.WasDenied)
            {
                await _userInterface.DisplayArtistRequestDeniedAsync(musicArtist);
            }
            else
            {
                await _userInterface.DisplayArtistRequestSuccessAsync(musicArtist);
                await _notificationWorkflow.NotifyForNewRequestAsync(_user.UserId, musicArtist);
            }
        }



        private static bool CanBeRequested(MusicArtist music)
        {
            return !music.Available && !music.Requested;
        }


        // ---------------- Album-level requesting ----------------

        public async Task SearchMusicForAlbumAsync(string albumName)
        {
            albumName = albumName.Replace(".", " ");
            IReadOnlyList<MusicAlbum> albums = await _musicSearcher.SearchMusicForAlbumAsync(new MusicRequest(_user, _categoryId), albumName);

            if (albums == null || !albums.Any())
            {
                await _userInterface.WarnNoMusicAlbumFoundAsync(albumName);
                return;
            }

            if (albums.Count > 1)
                await _userInterface.ShowMusicAlbumSelection(new MusicRequest(_user, _categoryId), albums);
            else
                await HandleMusicAlbumSelectionInternalAsync(albums.Single());
        }

        public async Task HandleMusicAlbumSelectionAsync(string albumId)
        {
            MusicAlbum album = await _musicSearcher.SearchMusicForAlbumIdAsync(new MusicRequest(_user, _categoryId), albumId);

            if (album == null)
            {
                await _userInterface.WarnNoMusicAlbumFoundAsync(albumId);
                return;
            }

            await HandleMusicAlbumSelectionInternalAsync(album);
        }

        private async Task HandleMusicAlbumSelectionInternalAsync(MusicAlbum album)
        {
            if (!album.Available)
                await _userInterface.DisplayMusicAlbumDetailsAsync(new MusicRequest(_user, _categoryId), album);
            else
                await _userInterface.WarnMusicAlbumAlreadyAvailableAsync(album);
        }

        public async Task RequestMusicAlbumAsync(string albumId)
        {
            MusicAlbum album = await _musicSearcher.SearchMusicForAlbumIdAsync(new MusicRequest(_user, _categoryId), albumId);

            if (album == null)
            {
                await _userInterface.WarnNoMusicAlbumFoundAsync(albumId);
                return;
            }

            MusicRequestResult result = await _requester.RequestMusicAlbumAsync(new MusicRequest(_user, _categoryId), album);

            if (result.WasDenied)
                await _userInterface.DisplayAlbumRequestDeniedAsync(album);
            else
                await _userInterface.DisplayAlbumRequestSuccessAsync(album);
        }
    }
}
