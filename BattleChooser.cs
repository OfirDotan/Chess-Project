using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System;
using System.Threading;

namespace FinalProjectChess
{
    [Activity(Label = "BattleChooser")]
    public class BattleChooser : Activity
    {
        LinearLayout scrollView;
        Button battleButton;
        ImageButton refreshButton;
        string chosenUsername;
        bool didClick;
        static Handler handler = new Handler();
        Activity currActivity;
        bool didMoveToMatch;

        Button lastButtonClicked;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.battle_users);
            chosenUsername = "";
            scrollView = FindViewById<LinearLayout>(Resource.Id.scrollView);
            battleButton = FindViewById<Button>(Resource.Id.btnBattle);
            refreshButton = FindViewById<ImageButton>(Resource.Id.btnRefresh);
            lastButtonClicked = null;
            battleButton.Click += battleClick;
            refreshButton.Click += refreshClick;
            didClick = false;
            currActivity = this;
            didMoveToMatch = false;
            string availableUsers = Intent.GetStringExtra("availableUsers");
            if (availableUsers != null && availableUsers != "Empty") {
                string[] users = availableUsers.Split(',');

                for (int i = 0; i < users.Length; i++)
                {
                    Button button = new Button(this);
                    button.Text = users[i];
                    LinearLayout.LayoutParams ivLLParams = new LinearLayout.LayoutParams(175, 175);
                    ivLLParams.TopMargin = 5;
                    ivLLParams.RightMargin = 15;
                    ivLLParams.Gravity = Android.Views.GravityFlags.Center;
                    button.SetBackgroundColor(Android.Graphics.Color.ParseColor("#2F72B5"));
                    button.SetTextColor(Android.Graphics.Color.ParseColor("#1F4B77"));
                    button.LayoutParameters = ivLLParams;
                    button.Click += chosenUserClick;
                    scrollView.AddView(button);
                }
            }
            // Create a new thread and pass the method to be executed by the thread
            Thread thread = new Thread(checkForAuthMessageThread);

            // Start the thread
            thread.Start();
        }

        private void refreshClick(object sender, EventArgs e)
        {
            scrollView.RemoveAllViews();
            string availableUsers = ServerCommunication.sendAndReceive("Gimmie Users");
            if (availableUsers != null && availableUsers != "Empty")
            {
                string[] users = availableUsers.Split(',');

                for (int i = 0; i < users.Length; i++)
                {
                    Button button = new Button(this);
                    button.Text = users[i];
                    LinearLayout.LayoutParams ivLLParams = new LinearLayout.LayoutParams(175, 175);
                    ivLLParams.TopMargin = 5;
                    ivLLParams.RightMargin = 15;
                    ivLLParams.Gravity = Android.Views.GravityFlags.Center;
                    button.SetBackgroundColor(Android.Graphics.Color.ParseColor("#2F72B5"));
                    button.SetTextColor(Android.Graphics.Color.ParseColor("#1F4B77"));
                    button.LayoutParameters = ivLLParams;
                    button.Click += chosenUserClick;
                    scrollView.AddView(button);
                }
            }
        }

        private void battleClick(object sender, EventArgs e)
        {
            if (!didClick)
            {
                if (chosenUsername != "")
                {

                    ServerCommunication.send(chosenUsername);
                    didClick = true;
                }
                else
                {
                    Toast.MakeText(this, "You need to select a user first...", ToastLength.Short).Show();
                }
            }
            else
            {
                Toast.MakeText(this, "You already started a battle, wait!", ToastLength.Short).Show();
            }

        }
        void checkForAuthMessageThread()
        {
            bool battleAcceptedSent = false;
            while (!didMoveToMatch)
            {
                string receivedData = ServerCommunication.receive(-1, 300);
                if (receivedData != null)
                {
                    if (receivedData.Equals("Battle Started") && !battleAcceptedSent)
                    {
                        ServerCommunication.send("Battle Accepted");
                        battleAcceptedSent = true;
                        currActivity.RunOnUiThread(() =>
                        {
                            didMoveToMatch = true;
                            Intent intent = new Intent(currActivity, typeof(MultiplayerActivity));
                            currActivity.StartActivity(intent);
                            ((BattleChooser)currActivity).closeChooser();
                        });
                        break;
                    }
                    else if(receivedData.Equals("User is offline"))
                    {
                        Console.WriteLine("OFFLINE");
                        didClick = false;
                    }
                }
                Thread.Sleep(20);
            }
        }
        private void closeChooser()
        {
            Finish();
        }
        private void chosenUserClick(object sender, EventArgs e)
        {
            if(lastButtonClicked != null)
            {
                lastButtonClicked.SetBackgroundColor(Android.Graphics.Color.ParseColor("#2F72B5"));
                lastButtonClicked.SetTextColor(Android.Graphics.Color.ParseColor("#1F4B77"));
            }
            ((Button) sender).SetBackgroundColor(Android.Graphics.Color.Gold);
            ((Button)sender).SetTextColor(Android.Graphics.Color.DarkGoldenrod);
            lastButtonClicked = ((Button)sender);
            chosenUsername = ((Button)sender).Text;
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (!didMoveToMatch)
            {
                ServerCommunication.closeSocket();
                base.OnDestroy();
            }
        }
    }
}