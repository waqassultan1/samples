﻿using SlapChat.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Linq;

namespace SlapChat.ViewModel
{
    public class SendViewModel : ViewModelBase
    {
        private IChatService chatService;
        private CameraViewModel parentViewModel;

        public SendViewModel()
        {
            chatService = ServiceLocator.Current.GetInstance<IChatService>();
            
            parentViewModel = ServiceLocator.Current.GetInstance<CameraViewModel>();
            parentViewModel.PropertyChanged += parentViewModel_PropertyChanged;
            ResetImageSource();


            SendPhoto = new RelayCommand(async () =>
            {
                PhotoRecord p = new PhotoRecord();
                
                // If they didn't explicitly toggle the list picker, assume 
                // they want the first contact in the list.
                if (SelectedFriend != null)
                {
                    p.RecepientUserId = SelectedFriend.UserId;
                }
                else
                {
                    p.RecepientUserId = Friends.First().UserId;
                }

                p.SenderUserId = App.CurrentUser.UserId;
                p.SenderName = App.CurrentUser.Name;

                await chatService.CreatePhotoRecordAsync(p);
                System.Net.Http.HttpResponseMessage m =
                await chatService.UploadPhotoAsync(p.Uri, p.UploadKey, parentViewModel.Image);

                App.RootFrame.Navigate(new Uri("/View/PhotosPage.xaml", UriKind.RelativeOrAbsolute));

            });

            RefreshCommand = new RelayCommand(async () =>
            {
                Friends = await chatService.ReadFriendsAsync(App.CurrentUser.UserId);
            });
        }

        public const string SelectedFriendPropertyName = "SelectedFriend";
        private User selectedFriend;

        public User SelectedFriend
        {
            get
            {
                return selectedFriend;
            }

            private set
            {
                if (selectedFriend == value)
                {
                    return;
                }

                selectedFriend = value;
                RaisePropertyChanged(SelectedFriendPropertyName);
            }
        }

        public const string FriendsPropertyName = "Friends";
        public ObservableCollection<User> friends;
        public ObservableCollection<User> Friends
        {
            get
            {
                return friends;
            }

            private set
            {
                if (friends == value)
                {
                    return;
                }

                friends = value;
                RaisePropertyChanged(FriendsPropertyName);
            }
        }

        public const string ImagePropertyName = "Image";
        private BitmapImage image;

        public BitmapImage Image
        {
            get
            {
                return image;
            }

            private set
            {
                if (image == value)
                {
                    return;
                }

                image = value;
                RaisePropertyChanged(ImagePropertyName);
            }
        }

        public RelayCommand SendPhoto
        {
            get;
            private set;
        }

        public RelayCommand RefreshCommand
        {
            get;
            private set;
        }

        private void ResetImageSource(){
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                Image = new BitmapImage();
                Image.SetSource(parentViewModel.Image);
            });
        }

        void parentViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (String.Equals(e.PropertyName, CameraViewModel.ImagePropertyName))
            {
                ResetImageSource();
            }
        }
    }


}
