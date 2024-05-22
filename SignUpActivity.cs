using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

//serverIP change to actual IP line 60

namespace FinalProjectChess
{
    [Activity(Label = "SignUpActivity")]
    public class SignUpActivity : Activity
    {
        ImageButton btnSignUpBackArrow;
        EditText etSignUpUsername;
        EditText etSignUpPassword;
        EditText etSignUpPasswordConfirm;
        TextView tvSignUpNotif;
        Button btnSignup;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.signup);

            btnSignUpBackArrow = FindViewById<ImageButton>(Resource.Id.btnSignUpBackArrow);
            etSignUpUsername = FindViewById<EditText>(Resource.Id.etSignUpUsername);
            etSignUpPassword = FindViewById<EditText>(Resource.Id.etSignUpPassword);
            etSignUpPasswordConfirm = FindViewById<EditText>(Resource.Id.etSignUpPasswordConfirm);
            tvSignUpNotif = FindViewById<TextView>(Resource.Id.tvSignUpNotif);
            btnSignup = FindViewById<Button>(Resource.Id.btnSignup);

            btnSignUpBackArrow.Click += btnClick;
            btnSignup.Click += btnClick;
        }

        private void btnClick(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                //if (!etSignUpPassword.Text.Equals(etSignUpPasswordConfirm.Text))
                //{
                //    tvSignUpNotif.Text = "Your password fields must be identical.";
                //}
                //else if (!doesMeetPasswordRequirement(etSignUpPassword.Text))
                //{
                //    tvSignUpNotif.Text = "Your password does not meet the requirements.";
                //}
                //else
                //{
                    tvSignUpNotif.Text = "";

                    string signUpResult = ServerCommunication.signUp(etSignUpUsername.Text, etSignUpPassword.Text);

                    if (!signUpResult.Equals("AccountCreated"))
                    {
                        if (signUpResult.Equals("UsernameTaken"))
                        {
                            tvSignUpNotif.Text = "Username is taken!";
                        }
                        else
                        {
                            tvSignUpNotif.Text = "Sorry there was an error, try again later.";
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, "Account Created!", ToastLength.Short).Show();
                        Finish();
                    }
                //}
            }
            else
            {
                //Must be btnBackArrow
                Finish();
            }
        }
        public bool doesMeetPasswordRequirement(string password)
        {
            if (password.Length < 8)
            {
                return false;
            }
            bool doesHaveNumber = false;
            bool doesHaveUppercase = false;
            for (int i = 0; i < password.Length; i++)
            {
                if (password[i] >= '0' && password[i] <= '9')
                {
                    doesHaveNumber = true;
                }
                else if (password[i] >= 'A' && password[i] <= 'Z')
                {
                    doesHaveUppercase = true;
                }
                if (doesHaveNumber && doesHaveUppercase) return true;
            }
            return doesHaveNumber && doesHaveUppercase;
        }
    }
}