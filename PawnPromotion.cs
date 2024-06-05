using Android.App;
using Android.Content;
using Android.Widget;
using System.Threading;

namespace FinalProjectChess
{
    public class PawnPromotion : Java.Lang.Object, RadioGroup.IOnCheckedChangeListener
    {
        private DialogHandler promotionHandler;
        public string checkedPromotion = "";
        private Dialog promotionDialog;
        private RadioGroup promotionChoice;
        private Context context;
        private int pawnRow, pawnCol;
        private string color;
        private Piece[,] board;

        public PawnPromotion(DialogHandler handler, Context context, int pawnRow, int pawnCol, string color, Piece[,] board)
        {
            this.promotionHandler = handler;
            this.context = context;
            this.pawnRow = pawnRow;
            this.pawnCol = pawnCol;
            this.color = color;
            this.board = board;
        }

        public void OnCheckedChanged(RadioGroup group, int checkedId)
        {
            switch (checkedId)
            {
                case Resource.Id.promoteQueen:
                    checkedPromotion = "Queen";
                    break;
                case Resource.Id.promoteRook:
                    checkedPromotion = "Rook";
                    break;
                case Resource.Id.promoteBishop:
                    checkedPromotion = "Bishop";
                    break;
                case Resource.Id.promoteKnight:
                    checkedPromotion = "Knight";
                    break;
                    //// Add cases for other promotions if needed
            }
        }

        public void Start()
        {
            ThreadStart threadStart = new ThreadStart(Run);
            Thread t = new Thread(threadStart);
            t.Start();
        }
        public void Run()
        {
            promotionHandler.Post(() =>
            {
                promotionDialog = new Dialog(context);
                promotionDialog.SetContentView(Resource.Layout.pawn_promote);
                promotionDialog.SetTitle("Choose Promotion");
                promotionDialog.SetCancelable(false);

                promotionChoice = promotionDialog.FindViewById<RadioGroup>(Resource.Id.promotionGroup);
                promotionChoice.SetOnCheckedChangeListener(this);

                promotionDialog.Show();
            });

            while (checkedPromotion == "")
            {
                Thread.Sleep(500);
            }

            // Use PromotionHandler to post messages to the main thread
            promotionHandler.Post(() =>
            {
                promotionDialog.Dismiss();
                switch (checkedPromotion)
                {
                    case "Queen":
                        MainActivity.board[pawnRow, pawnCol] = new Queen(color, pawnRow, pawnCol, board);
                        MainActivity.imageViews[pawnRow, pawnCol].SetImageResource(Resource.Drawable.WhiteQueen);
                        break;
                    case "Rook":
                        MainActivity.board[pawnRow, pawnCol] = new Rook(color, pawnRow, pawnCol, board);
                        MainActivity.imageViews[pawnRow, pawnCol].SetImageResource(Resource.Drawable.WhiteRook);
                        checkedPromotion = "Rook";
                        break;
                    case "Bishop":
                        MainActivity.board[pawnRow, pawnCol] = new Bishop(color, pawnRow, pawnCol, board);
                        MainActivity.imageViews[pawnRow, pawnCol].SetImageResource(Resource.Drawable.WhiteBishop);
                        checkedPromotion = "Bishop";
                        break;
                    case "Knight":
                        MainActivity.board[pawnRow, pawnCol] = new Knight(color, pawnRow, pawnCol, board);
                        MainActivity.imageViews[pawnRow, pawnCol].SetImageResource(Resource.Drawable.WhiteKnight);
                        checkedPromotion = "Knight";
                        break;
                }
                MainActivity.board[pawnRow, pawnCol].setCoords(pawnRow, pawnCol);

            });
        }
    }
}
