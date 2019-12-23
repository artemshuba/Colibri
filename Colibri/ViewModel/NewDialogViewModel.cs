using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Colibri.Helpers;
using Colibri.Services;
using Colibri.View;
using Colibri.ViewModel.Messaging;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Jupiter.Mvvm;
using Jupiter.Utils.Extensions;
using VkLib.Core.Friends;
using VkLib.Core.Users;

namespace Colibri.ViewModel
{
    public class NewDialogViewModel : ViewModelBase
    {
        private List<VkProfile> _friends;
        private CollectionViewSource _friendsCollection;

        private string _searchQuery;
        private List<VkProfile> _searchResults;
        private CancellationTokenSource _searchCancellationToken = new CancellationTokenSource();

        private int _selectedSortTypeIndex = 1;
        private DateTime _lastHintsRequestTime;

        #region Commands

        public RelayCommand<VkProfile> StartChatCommand { get; private set; }

        #endregion

        public List<VkProfile> Friends
        {
            get { return _friends; }
            private set { Set(ref _friends, value); }
        }

        public CollectionViewSource FriendsCollection
        {
            get { return _friendsCollection; }
            set { Set(ref _friendsCollection, value); }
        }

        public string SearchQuery
        {
            get
            {
                return _searchQuery;
            }
            set
            {
                if (Set(ref _searchQuery, value))
                {
                    SearchUsers();
                }
            }
        }

        public List<VkProfile> SearchResults
        {
            get { return _searchResults; }
            set { Set(ref _searchResults, value); }
        }

        public int SelectedSortTypeIndex
        {
            get { return _selectedSortTypeIndex; }
            set
            {
                if (Set(ref _selectedSortTypeIndex, value))
                {
                    AppSettings.NewDialogFriendsSortTypeIndex = value;
                    Load();
                }
            }
        }

        public NewDialogViewModel()
        {
            _selectedSortTypeIndex = AppSettings.NewDialogFriendsSortTypeIndex;

            RegisterTasks("users");

            Load();
        }

        protected override void InitializeCommands()
        {
            StartChatCommand = new RelayCommand<VkProfile>(friend =>
            {
                Navigator.Content.Navigate(typeof(ChatView), new ConversationViewModel(friend.Id));

                Messenger.Default.Send(new NewDialogStartedMessage() { UserId = friend.Id });
            });
        }

        private async void Load()
        {
            var friends = new List<VkProfile>();

            //try
            //{
            //    var response = await ServiceLocator.Vkontakte.Friends.Get(0, "first_name,last_name,photo,online,last_seen", null, 7, 0, FriendsOrder.ByRating);
            //    if (response != null)
            //        friends.AddRange(response.Items);
            //}
            //catch (Exception ex)
            //{
            //    Logger.Error(ex, "Unable to load friends");
            //}

            var t = TaskStarted("users");

            try
            {
                var response = await ServiceLocator.Vkontakte.Friends.Get(0, "first_name,last_name,photo,online,last_seen", null, 0, 0, _selectedSortTypeIndex != 0 ? FriendsOrder.ByName : FriendsOrder.ByRating);
                if (response != null && !response.Items.IsNullOrEmpty())
                    friends.AddRange(response.Items);
                else
                {
                    t.Error = Localizator.String("NewDialogFriendsEmptyError");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to load friends");

                t.Error = Localizator.String("NewDialogFriendsCommonError");
            }

            Friends = friends;

            if (_selectedSortTypeIndex != 0)
            {
                FriendsCollection = new CollectionViewSource()
                {
                    Source = _friends.ToAlphaGroups(f => _selectedSortTypeIndex == 1 ? f.FirstName : f.LastName),
                    ItemsPath = new PropertyPath("Value"),
                    IsSourceGrouped = true
                };
            }
            else
            {
                FriendsCollection = new CollectionViewSource()
                {
                    Source = _friends,
                    IsSourceGrouped = false
                };
            }

            t.Finish();
        }

        private async void SearchUsers()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                SearchResults = null;
                return;
            }

            _searchCancellationToken.Cancel();
            _searchCancellationToken = new CancellationTokenSource();

            try
            {
                var token = _searchCancellationToken.Token;

                if ((DateTime.Now - _lastHintsRequestTime).TotalMilliseconds < 500)
                {
                    await Task.Delay(500);
                }

                _lastHintsRequestTime = DateTime.Now;

                if (token.IsCancellationRequested)
                    return;

                var result = await ServiceLocator.UserService.SearchHints(_searchQuery);
                if (result != null && !token.IsCancellationRequested)
                    SearchResults = result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}