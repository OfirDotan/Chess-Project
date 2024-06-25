using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System;


namespace FinalProjectChess
{
    [Activity(Label = "LogInActivity")]
    public class LogInActivity : Activity
    {
        Android.Content.ISharedPreferences sp;
        ImageButton btnBackArrow;
        EditText etUsername;
        EditText etPassword;
        CheckBox cbRemember;
        TextView tvLoginNotif;
        Button btnMoveToSignup;
        Button btnLogin;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.login);

            sp = this.GetSharedPreferences("SavedAccounts", Android.Content.FileCreationMode.Private);

            btnBackArrow = FindViewById<ImageButton>(Resource.Id.btnBackArrow);
            etUsername = FindViewById<EditText>(Resource.Id.etUsername);
            etPassword = FindViewById<EditText>(Resource.Id.etPassword);
            cbRemember = FindViewById<CheckBox>(Resource.Id.cbRemember);
            tvLoginNotif = FindViewById<TextView>(Resource.Id.tvLoginNotif);
            btnMoveToSignup = FindViewById<Button>(Resource.Id.btnMoveToSignup);
            btnLogin = FindViewById<Button>(Resource.Id.btnLogin);

            btnBackArrow.Click += btnClick;
            btnMoveToSignup.Click += btnClick;
            btnLogin.Click += btnClick;
        }

        private void btnClick(object sender, EventArgs e)
        {
            if(sender is Button)
            {
                if (btnMoveToSignup == ((Button)sender))
                {
                    Intent intent = new Intent(this, typeof(SignUpActivity));
                    StartActivity(intent);
                }
                else if (btnLogin == ((Button)sender))
                {
                    if(etUsername.Text.Equals("")|| etPassword.Text.Equals(""))
                    {
                        tvLoginNotif.Text = "Fields can't be empty";
                    }
                    else
                    {
                        string output = ServerCommunication.authenticate(etUsername.Text, etPassword.Text);

                        if (output == null)
                        {
                            tvLoginNotif.Text = "Invalid credentials, or you're already logged in.";
                        }
                        else
                        {
                            //Remembering login details if user wants that
                            if (cbRemember.Checked)
                            {
                                var editor = sp.Edit();
                                editor.PutString("Username", etUsername.Text);
                                editor.PutString("Password", etPassword.Text);
                                editor.Commit();
                            }

                            //Move to multiplayer activity.
                            Finish();
                            if (output != "Empty")
                            {
                                Intent intent = new Intent(this, typeof(BattleChooser));
                                intent.PutExtra("availableUsers", output);
                                this.StartActivity(intent);
                            }
                            else
                            {
                                Toast.MakeText(this, "Connected! But you are the only user, try again later", ToastLength.Short).Show();

                            }
                        }
                    }

                }
            }
            else
            {
                //Must be btnBackArrow
                Finish();
            }
        }
    }
}