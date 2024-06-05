using Android.App;
using Android.Content;
using Android.Widget;
using System;
using System.Threading;

namespace FinalProjectChess
{
    public class EndGameDialog : Java.Lang.Object
    {
        private DialogHandler handler;
        private Dialog endGameDialog;
        private Context context;

        private TextView tvWon;
        private TextView tvWayOfWinning;
        
        private Button btnRematch;
        private Button btnBackToMenu;

        private string wayOfWinning;
        private string winnerColor;
        private bool shouldDisplayRematch;

        public EndGameDialog(DialogHandler handler, Context context, string winnerColor, string wayOfWinning, bool shouldDisplayRematch)
        {
            this.handler = handler;
            this.context = context;
            this.winnerColor = winnerColor;
            this.wayOfWinning = wayOfWinning;
            this.shouldDisplayRematch = shouldDisplayRematch;
        }

        public void Start()
        {
            ThreadStart threadStart = new ThreadStart(Run);
            Thread t = new Thread(threadStart);
            t.Start();
        }
        public void Run()
        {
            handler.Post(() =>
            {
                endGameDialog = new Dialog(context);
                endGameDialog.SetContentView(Resource.Layout.game_end);
                endGameDialog.SetTitle("Choose Promotion");
                endGameDialog.SetCancelable(false);
                tvWon = endGameDialog.FindViewById<TextView>(Resource.Id.tvWon);

                if (this.wayOfWinning == "Checkmate")
                {
                    tvWon.Text = winnerColor + " has won!";
                }
                else
                {
                    tvWon.Text = "It's a tie!";
                }
    
                tvWayOfWinning = endGameDialog.FindViewById<TextView>(Resource.Id.tvWayOfWinning);
                tvWayOfWinning.Text = "By " + wayOfWinning;

                btnRematch = endGameDialog.FindViewById<Button>(Resource.Id.btnRematch);
                btnRematch.Click += buttonClick;
                btnBackToMenu = endGameDialog.FindViewById<Button>(Resource.Id.btnMainMenu);
                btnBackToMenu.Click += buttonClick;

                if (!shouldDisplayRematch)
                {
                    btnRematch.Visibility = Android.Views.ViewStates.Invisible;
                }
                endGameDialog.Show();
            });

        }

        private void buttonClick(object sender, EventArgs e)
        {
            if (btnRematch == ((Button)sender))
            {
                ((Activity)context).Finish();
                Intent intent = new Intent(context, typeof(MainActivity));
                context.StartActivity(intent);
            }
            else if(btnBackToMenu == (Button)sender)
            {
                ((Activity)context).Finish();
            }
        }
    }
}
