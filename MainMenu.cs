  using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System;

namespace FinalProjectChess
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.Design", MainLauncher = true)]
    public class MainMenu : Activity
    {
        Android.Content.ISharedPreferences sp;
        Button localChessBtn;
        Button onlineChessBtn;
        Button signOutBtn;
        TextView welcomeBackTv;
        bool isLoggedIn;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            //Set our view from the "main" layout resource

            SetContentView(Resource.Layout.main_menu);

            sp = this.GetSharedPreferences("SavedAccounts", Android.Content.FileCreationMode.Private);
            
            //Dont forget to remove
            var editor = sp.Edit();
            editor.PutString("Username", null);
            editor.Commit();

            localChessBtn = FindViewById<Button>(Resource.Id.btnLocal);
            onlineChessBtn = FindViewById<Button>(Resource.Id.btnOnline);
            signOutBtn = FindViewById<Button>(Resource.Id.btnSignOut);
            welcomeBackTv = FindViewById<TextView>(Resource.Id.tvWelcomeBack);

            isLoggedIn = true;

            localChessBtn.Click += click;
            onlineChessBtn.Click += click;
            signOutBtn.Click += signOutClick;
            if (sp.GetString("Username", null) == null || sp.GetString("Password", null) == null)
            {
                isLoggedIn = false;
                signOutBtn.Visibility = Android.Views.ViewStates.Invisible;
                welcomeBackTv.Visibility = Android.Views.ViewStates.Invisible;
            }
            else {
                welcomeBackTv.Text = "Welcome back " + sp.GetString("Username", null) + "!";
            }
            
        }

        private void signOutClick(object sender, EventArgs e)
        {
            Toast.MakeText(this, "You are the only user connected, try again later", ToastLength.Short).Show();
            ServerCommunication.closeSocket();
            signOutBtn.Visibility = Android.Views.ViewStates.Invisible;
            welcomeBackTv.Visibility = Android.Views.ViewStates.Invisible;
        }

        private void click(object sender, EventArgs e)
        {
            if (localChessBtn == ((Button) sender))
            {
                Intent intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
            }
            else if (onlineChessBtn == ((Button)sender))
            {
                if (ServerCommunication.socket != null) {
                    string output = ServerCommunication.sendAndReceive("Gimmie Users");
                    if (output != "Empty")
                    {
                        Intent intent = new Intent(this, typeof(BattleChooser));
                        intent.PutExtra("availableUsers", output);
                        this.StartActivity(intent);
                    }
                    else
                    {
                        Toast.MakeText(this, "You are the only user connected, try again later", ToastLength.Short).Show();
                    }
                }
                else if (isLoggedIn)
                {
                    string output = ServerCommunication.authenticate(sp.GetString("Username", null), sp.GetString("Password", null));
                    if (output == null)
                    {
                        Toast.MakeText(this, "There was an error connecting, try again later", ToastLength.Short).Show();
                    }
                    else
                    {
                        if (output != "Empty")
                        {
                            Intent intent = new Intent(this, typeof(BattleChooser));
                            intent.PutExtra("availableUsers", output);
                            Finish();
                            this.StartActivity(intent);
                        }
                        else
                        {
                            Toast.MakeText(this, "You are the only user connected, try again later", ToastLength.Short).Show();
                        }
                    }

                }
                else
                {
                    Intent intent = new Intent(this, typeof(LogInActivity));
                    StartActivity(intent);
                }
            }
        }
        protected override void OnResume()
        {
            if (sp.GetString("Username", null) == null || sp.GetString("Password", null) == null)
            {
                isLoggedIn = false;
                signOutBtn.Visibility = Android.Views.ViewStates.Invisible;
                welcomeBackTv.Visibility = Android.Views.ViewStates.Invisible;
            }
            else
            {
                isLoggedIn = true;
                welcomeBackTv.Visibility = Android.Views.ViewStates.Visible;
                signOutBtn.Visibility = Android.Views.ViewStates.Visible;

                welcomeBackTv.Text = "Welcome back " + sp.GetString("Username", null) + "!";
            }
            base.OnResume();
        }
        protected override void OnDestroy()
        {
            ServerCommunication.closeSocket();

            base.OnDestroy();
        }
    }
}