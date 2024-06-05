using Android.App;
using Android.Content;
using System.Threading;

namespace FinalProjectChess
{
    public class LoadingDialog : Java.Lang.Object
    {
        private DialogHandler handler;
        private Dialog loadingDialog;
        private Context context;

        public LoadingDialog(DialogHandler handler, Context context)
        {
            this.handler = handler;
            this.context = context;
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
                loadingDialog = new Dialog(context);
                loadingDialog.SetContentView(Resource.Layout.loading);
                loadingDialog.SetTitle("Choose Promotion");
                loadingDialog.SetCancelable(false);
                loadingDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.Transparent);
                loadingDialog.Show();
            });
        }
        public void Dismiss()
        {
            handler.Post(() =>
            {
                if (loadingDialog != null && loadingDialog.IsShowing)
                {
                    loadingDialog.Dismiss();
                }
            });
        }
    }
}
