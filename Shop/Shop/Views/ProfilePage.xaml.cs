using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using Shop.Helpers;
using Shop.Model;
using System;
using System.IO;

namespace Shop.Views
{
    public partial class ProfilePage : ContentPage
    {
        public static bool IsAuth = false;

        private FirebaseClient firebaseClient = new FirebaseClient("https://car-shop-fde53-default-rtdb.europe-west1.firebasedatabase.app/");
        private FirebaseStorage firebaseStorage = new FirebaseStorage("car-shop-fde53.appspot.com");

        public UserProfile UserProfile { get; set; }

        public ProfilePage()
        {
            InitializeComponent();
            BindingContext = this;
            CheckAuth();
        }

        private async void OnGoToReg(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new RegisterPage());
        }

        private async void OnGoToJoin(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new JoinPage());
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            FirebaseHelper.Logout();
            CheckAuth();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CheckAuth();
            LoadUserProfile();
        }

        public void CheckAuth()
        {
            if (FirebaseHelper.AuthProvider.User == null)
            {
                IsAuth = false;
                Resources["authInfo"] = "�� �� ����� � �������!";
            }
            else
            {
                IsAuth = true;
                var userInfo = FirebaseHelper.AuthProvider.User;
                Resources["authInfo"] = userInfo.Info.Email;
            }
            Resources["IsAuth"] = IsAuth;
        }

        private void LoadUserProfile()
        {
            if (IsAuth)
            {
                var userInfo = FirebaseHelper.FirebaseClient.Child("users").Child(FirebaseHelper.AuthProvider.User.Uid).Child("Info").OnceSingleAsync<Firebase.Auth.UserInfo>().Result;

                Resources["DisplayName"] = userInfo.DisplayName;
                Resources["Email"] = userInfo.Email;
                Resources["PhotoUrl"] = userInfo.PhotoUrl;
            }
            else
            {
                Resources["DisplayName"] = string.Empty;
                Resources["Email"] = string.Empty;
                Resources["PhotoUrl"] = "profile.png";
            }
        }

        private async void UploadAvatar(object sender, EventArgs e)
        {
            try
            {
                var file = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select an image"
                });

                if (file != null)
                {
                    using (var stream = await file.OpenReadAsync())
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var imageUrl = await firebaseStorage.Child("avatars").Child(fileName).PutAsync(stream);
                        var photoUrl = await firebaseStorage.Child("avatars").Child(fileName).GetDownloadUrlAsync();

                        // Update the user's profile with the new photo URL
                        var userInfo = FirebaseHelper.AuthProvider.User;
                        userInfo.Info.PhotoUrl = photoUrl;

                        // Save the updated user profile in the database
                        await firebaseClient.Child("users").Child(userInfo.Uid).PutAsync(userInfo);

                        // Update the UI with the new photo URL
                        Resources["PhotoUrl"] = userInfo.Info.PhotoUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                Console.WriteLine($"Error uploading avatar: {ex.Message}");
            }
        }
        private async void OnEditProfileClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new EditProfilePage());
        }
    }
}