using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinalProjectChess
{
    public class DialogHandler : Handler
    {
        Context context;

        public DialogHandler(Context context)
        {
            this.context = context;
        }
        public override void HandleMessage(Message msg)
        {
            base.HandleMessage(msg);
        }
    }
}