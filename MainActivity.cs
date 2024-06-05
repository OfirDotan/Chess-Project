using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

namespace FinalProjectChess
{
    [Activity(Theme = "@style/Theme.Design", MainLauncher = false)]
    public class MainActivity : AppCompatActivity
    {
        public static Piece[,] board;
        public static ImageView[,] imageViews;
        //0 - White, 1 - Black
        public EatBar[] eatBars; 
        string selected;
        bool isDone;
        King blackKing;
        King whiteKing;
        TextView turnView;
        bool didGameEnd;
        string technicalEnPassent;
        string castlingSatus;
        Button bestMoveBtn;
        bool lockBestMoveButton;

        enum turns {
            White,
            Black
        }
        turns currentTurn;
        enum promotionState
        {
            noPromotionNeeded,
            waitingForInput,
            inputAccepted
        }
        promotionState currentPromotionState;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            imageViews = new ImageView[8, 8];
            board = new Piece[8, 8];
            eatBars = new EatBar[2];
            selected = "empty";
            currentTurn = turns.White;
            currentPromotionState = promotionState.noPromotionNeeded;
            lockBestMoveButton = false;
            didGameEnd = false;
            technicalEnPassent = "nothing";
            castlingSatus = "nothing";
            isDone = false;
            TableLayout table = FindViewById<TableLayout>(Resource.Id.tableLayout);
            bestMoveBtn = FindViewById<Button>(Resource.Id.btnBestMove);
            bestMoveBtn.Click += bestMoveClick;
            setBoardSize(table);

            for (int row = 0; row < 8; row++)
            {
                for (char col = 'A'; col < 'I'; col++)
                {
                    int intCol = col - 'A';
                    ImageView d = FindViewById<ImageView>(Resources.GetIdentifier(col + "" + (row + 1), "id", PackageName));
                    imageViews[row, intCol] = d;
                    imageViews[row, intCol].Click += select;
                }
            }
            BoardSetupActions.resetBoard(board);
            whiteKing = ((King)board[0, 4]);
            blackKing = ((King)board[7, 4]);
            updateCastlingString();
            turnView = FindViewById<TextView>(Resource.Id.tvTurn);
        }

        private async void bestMoveClick(object sender, System.EventArgs e)
        {
            if(!lockBestMoveButton)
            {
                lockBestMoveButton = true;
                DialogHandler handler = new DialogHandler(this);
                LoadingDialog loadingDialog = new LoadingDialog(handler, this);
                loadingDialog.Start();

                HttpClient client = new HttpClient();
                try
                {
                    string apiUrl = "https://stockfish.online/api/s/v2.php";
                    apiUrl += "?fen=" + convertBoardToFEN();
                    apiUrl += "&depth=15";

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content as a string
                        string responseBody = await response.Content.ReadAsStringAsync();

                        JObject jsonResponse = JObject.Parse(responseBody);
                        string bestMove = jsonResponse["bestmove"].ToString();
                        string[] parts = bestMove.Split(' ');
                        bestMove = parts[1];


                        string fromSquare = bestMove.Substring(0, 2);
                        string toSquare = bestMove.Substring(2, 2);

                        int fromCol = fromSquare[0] - 'a';
                        int fromRow = fromSquare[1] - '1';
                        int toCol = toSquare[0] - 'a';
                        int toRow = toSquare[1] - '1';

                        // Dismiss loading dialog immediately after receiving the response
                        loadingDialog.Dismiss();

                        // Change background color to flash color
                        RunOnUiThread(() =>
                        {
                            // Change background color to flash color
                            imageViews[fromRow, fromCol].SetBackgroundColor(new Color(245, 237, 2, 120));
                            imageViews[toRow, toCol].SetBackgroundColor(new Color(255, 222, 8, 230));
         
               });
                        // Post a delayed task to revert back to original color
                        new Android.OS.Handler().PostDelayed(() =>
                        {
                            // Change background color back to original
                            RunOnUiThread(() =>
                            {
                                // Change background color to flash color
                                imageViews[fromRow, fromCol].SetBackgroundColor(new Color(0, 0, 0, 0));
                                imageViews[toRow, toCol].SetBackgroundColor(new Color(0, 0, 0, 0));
                            });
                        }, 1000);
                    }
                    else
                    {
                        // Handle the error response
                        Toast.MakeText(this, "ERROR", ToastLength.Short).Show();
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions
                    Console.WriteLine("Exception: " + ex.Message);
                }
                lockBestMoveButton = false;
            }
        }

        public void setBoardSize(TableLayout tableLayout)
        {
            // Calculate desired width based on the screen width
            DisplayMetrics displayMetrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
            int screenWidth = displayMetrics.WidthPixels;
            int desiredSize = (int)(screenWidth * 0.87);

            // Set the calculated width as the layout parameters for the TableLayout
            ViewGroup.LayoutParams layoutParams = tableLayout.LayoutParameters;
            layoutParams.Width = desiredSize;
            layoutParams.Height = desiredSize;
            tableLayout.LayoutParameters = layoutParams;

            for (int i = 1; i < 9; i++)
            {
                for (char j = 'A'; j < 'I'; j++)
                {
                    int imageKey = Resources.GetIdentifier(j + i.ToString(), "id", this.PackageName);
                    ImageView img = FindViewById<ImageView>(imageKey);

                    layoutParams = img.LayoutParameters;
                    layoutParams.Width = desiredSize / 8;
                    layoutParams.Height = desiredSize / 8;
                    img.LayoutParameters = layoutParams;
                }
            }

            int eatMargin = (int)(screenWidth * 0.13) / 2;
            FrameLayout blackEatLayout = FindViewById<FrameLayout>(Resource.Id.blackEatLayout);
            FrameLayout whiteEatLayout = FindViewById<FrameLayout>(Resource.Id.whiteEatLayout);

            ViewGroup.MarginLayoutParams frameLayoutParams = (ViewGroup.MarginLayoutParams)blackEatLayout.LayoutParameters;
            frameLayoutParams.LeftMargin = eatMargin;
            blackEatLayout.LayoutParameters = frameLayoutParams;

            frameLayoutParams = (ViewGroup.MarginLayoutParams)whiteEatLayout.LayoutParameters;
            frameLayoutParams.LeftMargin = eatMargin;
            whiteEatLayout.LayoutParameters = frameLayoutParams;

            TextView tvWhiteEat = FindViewById<TextView>(Resource.Id.tvWhiteEat);
            TextView tvBlackEat = FindViewById<TextView>(Resource.Id.tvBlackEat);
            eatBars[0] = new EatBar(whiteEatLayout, tvWhiteEat, "White");
            eatBars[1] = new EatBar(blackEatLayout, tvBlackEat, "Black");

        }

        private void select(object sender, System.EventArgs e)
        {
            if (!didGameEnd)
            {
                //Makes sure that the it's the selecting part of the turn
                if (!isDone)
                {
                    //Gets the locations of the position that was clicked.
                    string id = ((ImageView)sender).ContentDescription;
                    int col = id[0] - 'A';
                    int row = id[1] - '1';
                    //Makes sure that the position that was clicked on by the player isn't null and is the same color as the player that is playing
                    if (!(board[row, col] == null) && board[row, col].color == currentTurn.ToString())
                    {
                        //The selected variable is for saving the last position the player clicked
                        if (selected != "empty")
                        {
                            //If selected isn't empty we will make sure to reset its background color so that we won't have any unselected positions with the green selected hover
                            int selCol = selected[0] - 'A';
                            int selRow = selected[1] - '1';
                            imageViews[selRow, selCol].SetBackgroundColor(new Color(0, 0, 0, 0));
                        }
                        //Changed the selected to the new ID, and gives it the blueish highlight
                        selected = id;
                        imageViews[row, col].SetBackgroundColor(new Color(62, 138, 250, 160));
                        isDone = true;
                    }
                    else
                    {
                        //Toast if the position has a color of the other piece
                        if (!(board[row, col] is null))
                        {
                            Toast.MakeText(this, "Sorry, this isn't " + board[row, col].color.ToLower() + "'s turn", ToastLength.Short).Show();
                        }
                        //Toast if the position is null
                        else
                        {
                            Toast.MakeText(this, "Sorry, you can't select an empty space", ToastLength.Short).Show();
                        }
                    }
                }
                //This is the submitting part of the turn
                else
                {
                    Piece eatenPiece = null;
                    string newPos = ((ImageView)sender).ContentDescription;
                    int selCol = selected[0] - 'A';
                    int selRow = selected[1] - '1';
                    technicalEnPassent = "nothing";
                    //Makes sure that the new position is not the same as the selected position
                    if (newPos != selected)
                    {
                        //Converts the position to ints
                        int newCol = newPos[0] - 'A';
                        int newRow = newPos[1] - '1';

                        bool check;
                        //Checks to see if the King of the same color is checked before making any move
                        check = isKingChecked(currentTurn.ToString());
                        if (check)
                        {
                            Piece save = board[newRow, newCol];
                            if (board[selRow, selCol].isLegalMove(newRow, newCol))
                            {
                                //Makes sure the selected new move is either null or a place with a piece that is not a king
                                if (board[newRow, newCol] == null || (board[newRow, newCol] != null && !(board[newRow, newCol] is King)))
                                {
                                    //Trying the move the player tried while saving the old piece that was in newRow, newCol
                                    eatenPiece = board[newRow, newCol];
                                    board[newRow, newCol] = board[selRow, selCol];
                                    board[newRow, newCol].setCoords(newRow, newCol);
                                    board[selRow, selCol] = null;

                                }
                                check = isKingChecked(currentTurn.ToString());
                                //Checks again to see if the move resolved the check
                                if (check)
                                {
                                    //Restoring the positions to their original positions before the move was made and resets en passent
                                    restorePositions(save, selRow, selCol, newRow, newCol, false);
                                    eatenPiece = null;
                                    //Because it is not a legal move and the king is already checked, we will post a message to the user telling him that it's because his king is checked
                                    if (board[selRow, selCol] is King)
                                    {
                                        Toast.MakeText(this, "Sorry, this move will not resolve your check", ToastLength.Short).Show();
                                    }
                                    //Because it is not a legal move and the user didn't try to protect the king, we will inform him that the king isn't checked
                                    else
                                    {
                                        Toast.MakeText(this, "Sorry, the king is checked", ToastLength.Short).Show();
                                    }

                                }
                                //If the move resolved the check, the program will just update the changed that were made in lines 131-135 graphically
                                else
                                {

                                    if (board[newRow, newCol] is Pawn)
                                    {
                                        updateIfPawnMoved(currentTurn.ToString(), newRow, newCol, selRow);
                                        handlePromotePawn(newRow, newCol, "White");
                                    }
                                    //If did a move that was not en passent, it will remove the option to do the last en passent
                                    if ((board[newRow, newCol] is Pawn) == false)
                                    {
                                        resetJustPawnJumped(board[newRow, newCol].color);
                                    }
                                    if (board[newRow, newCol] is Pawn && ((Pawn)board[newRow, newCol]).didEnPassent)
                                    {
                                        ((Pawn)board[newRow, newCol]).didEnPassent = false;
                                        if (board[newRow, newCol].color == "White")
                                        {
                                            eatenPiece = board[newRow - 1, newCol];
                                            board[newRow - 1, newCol] = null;
                                            imageViews[newRow - 1, newCol].SetImageResource(Resource.Drawable.EmptyTile);
                                        }
                                        else
                                        {
                                            eatenPiece = board[newRow + 1, newCol];
                                            board[newRow + 1, newCol] = null;
                                            imageViews[newRow + 1, newCol].SetImageResource(Resource.Drawable.EmptyTile);
                                        }
                                    }
                                    if (eatenPiece != null)
                                    {
                                        addToEat(eatenPiece.name,eatenPiece.pointWorth);
                                    }
                                    imageViews[newRow, newCol].SetImageDrawable(imageViews[selRow, selCol].Drawable);
                                    imageViews[selRow, selCol].SetImageResource(Resource.Drawable.EmptyTile);
                                    switchTurns();
                                }
                            }
                        }
                        //If the player isn't checked
                        else
                        {
                            if (board[selRow, selCol].isLegalMove(newRow, newCol) || (board[selRow, selCol] is King && board[newRow, newCol] is Rook && board[selRow, selCol].color == board[newRow, newCol].color))
                            {
                                bool didCastle = false;
                                bool couldntCastle = false;
                                Drawable castleImage = null;
                                Piece save = board[newRow, newCol];
                                if (board[newRow, newCol] is Rook && board[selRow, selCol] is King && !((King)board[selRow,selCol]).didMove && !((Rook)board[newRow, newCol]).didMove)
                                {
                                    if(((King)board[selRow, selCol]).canCastle(newRow, newCol))
                                    {
                                        ((King)board[selRow, selCol]).didMove = true;
                                        ((Rook)board[newRow, newCol]).didMove = true;
                                        didCastle = true;
                                        //In castling selRow == newRow
                                        eatenPiece = null;
                                        castleImage = imageViews[newRow, newCol].Drawable;
                                        if (newCol > selCol)
                                        {
                                            board[newRow, selCol + 2] = board[selRow, selCol];
                                            board[newRow, selCol + 2].setCoords(newRow, selCol + 2);
                                            board[newRow, selCol + 1] = board[newRow, newCol];
                                            board[newRow, selCol + 1].setCoords(newRow, selCol + 1);

                                            board[newRow, newCol] = null;
                                            board[selRow, selCol] = null;
                                        }
                                        else
                                        {
                                            board[newRow, selCol - 2] = board[selRow, selCol];
                                            board[newRow, selCol - 2].setCoords(newRow, selCol - 2);
                                            board[newRow, selCol - 1] = board[newRow, newCol];
                                            board[newRow, selCol - 1].setCoords(newRow, selCol - 1);

                                            board[newRow, newCol] = null;
                                            board[selRow, selCol] = null;
                                        }
                                    }
                                    else
                                    {
                                        couldntCastle = true;
                                    }
                                }
                                else
                                {
                                    //Makes sure the selected new move is either null or a place with a piece that is not a king
                                    if (board[newRow, newCol] == null || (board[newRow, newCol] != null && !(board[newRow, newCol] is King)))
                                    {
                                        eatenPiece = board[newRow, newCol];
                                        board[newRow, newCol] = board[selRow, selCol];
                                        board[newRow, newCol].setCoords(newRow, newCol);
                                        board[selRow, selCol] = null;
                                    }
                                }
                                check = isKingChecked(currentTurn.ToString());
                                if (check || couldntCastle)
                                {
                                    if (couldntCastle)
                                    {
                                        Toast.MakeText(this, "Sorry, you can't castle like that", ToastLength.Short).Show();
                                    }
                                    else
                                    {
                                        //Restoring the positions to their original positions before the move was made and resets en passent
                                        restorePositions(save, selRow, selCol, newRow, newCol, didCastle);
                                        if (didCastle)
                                        {
                                            ((King)board[selRow, selCol]).didMove = false;
                                            ((Rook)board[newRow, newCol]).didMove = false;
                                        }
                                        Toast.MakeText(this, "Sorry, this move will result in you being checked", ToastLength.Short).Show();
                                    }
                                }
                                else
                                {
                                    if (!didCastle && board[newRow, newCol] is King)
                                    {
                                        ((King)board[newRow, newCol]).didMove = true;
                                    }
                                    if (!didCastle && board[newRow, newCol] is Rook)
                                    {
                                        ((Rook)board[newRow, newCol]).didMove = true;
                                    }
                                    //If did a move that was not en passent, it will remove the option to do the last en passent
                                    if ((board[newRow, newCol] is Pawn) == false || !((Pawn)board[newRow, newCol]).didEnPassent)
                                    {
                                        string color = "White";

                                        if (!didCastle)
                                        {
                                            if (board[newRow, newCol].color == "White")
                                            {
                                                color = "Black";
                                            }
                                        }
                                        else
                                        {
                                            if (newCol > selCol)
                                            {
                                                if (board[newRow, selCol + 2].color == "White")
                                                {
                                                    color = "Black";
                                                }
                                            }
                                            else
                                            {
                                                if (board[newRow, selCol - 2].color == "White")
                                                {
                                                    color = "Black";
                                                }
                                            }
                                        }
                                        resetJustPawnJumped(color);
                                    }
                                    if (board[newRow, newCol] is Pawn)
                                    {
                                        updateIfPawnMoved(currentTurn.ToString(), newRow, newCol, selRow);
                                        handlePromotePawn(newRow, newCol, "White");
                                    }
                                    if (board[newRow, newCol] is Pawn && ((Pawn)board[newRow, newCol]).didEnPassent)
                                    {
                                        ((Pawn)board[newRow, newCol]).didEnPassent = false;
                                        if (board[newRow, newCol].color == "White")
                                        {
                                            board[newRow - 1, newCol] = null;
                                            imageViews[newRow - 1, newCol].SetImageResource(Resource.Drawable.EmptyTile);
                                        }
                                        else
                                        {
                                            board[newRow + 1, newCol] = null;
                                            imageViews[newRow + 1, newCol].SetImageResource(Resource.Drawable.EmptyTile);
                                        }
                                    }
                                    if (eatenPiece != null)
                                    {
                                        addToEat(eatenPiece.name, eatenPiece.pointWorth);
                                    }

                                    //Updates the image views
                                    if (!didCastle)
                                    {
                                        imageViews[newRow, newCol].SetImageDrawable(imageViews[selRow, selCol].Drawable);
                                        imageViews[selRow, selCol].SetImageResource(Resource.Drawable.EmptyTile);
                                    }
                                    else
                                    {
                                        if(newCol > selCol)
                                        {
                                            imageViews[newRow, selCol + 2].SetImageDrawable(imageViews[selRow, selCol].Drawable);
                                            imageViews[newRow, selCol + 1].SetImageDrawable(castleImage);
                                        }
                                        else
                                        {
                                            imageViews[newRow, selCol - 2].SetImageDrawable(imageViews[selRow, selCol].Drawable);
                                            imageViews[newRow, selCol - 1].SetImageDrawable(castleImage);
                                        }
                                        imageViews[newRow, newCol].SetImageResource(Resource.Drawable.EmptyTile);
                                        imageViews[selRow, selCol].SetImageResource(Resource.Drawable.EmptyTile);
                                    }
                                    switchTurns();
                                }
                            }
                        }
                        turnView.Text = currentTurn.ToString();
                        isDone = false;
                        imageViews[selRow, selCol].SetBackgroundColor(Color.Transparent);
                    }
                    //Resets the selected to be unselected if the player clicked on the same tile as the selected one
                    else
                    {
                        isDone = false;
                        imageViews[selRow, selCol].SetBackgroundColor(new Color(0, 0, 0, 0));
                        selected = "empty";
                        return;
                    }
                    convertBoardToFEN();
                    bool checkForCheck;
                    if (currentTurn.ToString() == "White")
                    {
                        checkForCheck = whiteKing.isChecked();
                        if (!checkForCheck && !canPreventCheckOrOnePieceMove("White", false))
                        {
                            //Stalemate
                            didGameEnd = true;
                            Toast.MakeText(this, "White king is Stalemated", ToastLength.Short).Show();
                            endGame("Black", "Stalemate");
                        }
                        //Needs to check if any piece can prevent the check, and if so then it isn't a checkmate
                        if (checkForCheck && whiteKing.isKingStuck() && !canPreventCheckOrOnePieceMove("White", true))
                        {
                            //Checkmate
                            didGameEnd = true;
                            Toast.MakeText(this, "White king is Checkmated", ToastLength.Short).Show();
                            endGame("Black", "Checkmate");
                        }
                    }
                    else
                    {
                        checkForCheck = blackKing.isChecked();
                        if (!checkForCheck && !canPreventCheckOrOnePieceMove("Black", false))
                        {
                            //Stalemate
                            didGameEnd = true;
                            Toast.MakeText(this, "Black king is Stalemated", ToastLength.Short).Show();
                            endGame("White", "Stalemate");
                        }
                        //Needs to check if any piece can prevent the check, and if so then it isn't a checkmate
                        if (checkForCheck && blackKing.isKingStuck() && !canPreventCheckOrOnePieceMove("Black", true))
                        {
                            //Checkmate
                            didGameEnd = true;
                            Toast.MakeText(this, "Black king is Checkmated", ToastLength.Short).Show();
                            endGame("White", "Checkmate");
                        }
                    }
                }
            }
        }
        public void endGame(string color, string wayOfEnding)
        {
            DialogHandler handler = new DialogHandler(this);
            EndGameDialog endGame = new EndGameDialog(handler, this, color, wayOfEnding, true);
            endGame.Start();
        }
        public void updateCastlingString()
        {
            castlingSatus = "";
            if (whiteKing.castleKingSide())
            {
                castlingSatus += "K";
            }
            if (whiteKing.castleQueenSide())
            {
                castlingSatus += "Q";
            }
            if (blackKing.castleKingSide())
            {
                castlingSatus += "k";
            }
            if (blackKing.castleQueenSide())
            {
                castlingSatus += "q";
            }
            if (castlingSatus.Equals(""))
            {
                castlingSatus = "nothing";
            }
        }
        public void switchTurns()
        {
            if (currentTurn == turns.White)
            {
                currentTurn = turns.Black;
            }
            else
            {
                currentTurn = turns.White;
            }
        }

        public void addToEat(string pieceName, int pointWorth)
        {
            if(currentTurn == turns.White)
            {
                eatBars[0].addEat(pieceName, pointWorth, new ImageView(this));
            }
            else
            {
                eatBars[1].addEat(pieceName, pointWorth, new ImageView(this));
            }
        }
        public bool canPreventCheckOrOnePieceMove(string color, bool checkForCheck)
        {
            bool hasLegalMove = false; ;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (board[row, col] != null && board[row, col].color == color)
                    {
                        // Check all legal moves for this piece
                        for (int newRow = 0; newRow < 8; newRow++)
                        {
                            for (int newCol = 0; newCol < 8; newCol++)
                            {
                                // Check if the move is legal
                                if (board[row, col].isLegalMove(newRow, newCol))
                                {

                                    // Make a temporary move to simulate the effect
                                    Piece tempPiece = board[newRow, newCol];
                                    board[newRow, newCol] = board[row, col];
                                    board[newRow, newCol].setCoords(newRow, newCol);
                                    board[row, col] = null;

                                    bool isCheckAfterMove = isKingChecked(color);

                                    // Undo the temporary move
                                    board[row, col] = board[newRow, newCol];
                                    board[row, col].setCoords(row, col);
                                    board[newRow, newCol] = tempPiece;

                                    // If the move prevents check, set the flag as true
                                    if (!isCheckAfterMove)
                                    {
                                        hasLegalMove = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // If no legal move and the king is not in check, it's checkmate
            if (checkForCheck && isKingChecked(color) && hasLegalMove)
            {
                // Checkmate
                return true;
            }

            // If the king is in check and no legal move to get out, it's stalemate
            if (!checkForCheck && !isKingChecked(color)  && hasLegalMove)
            {
                // Stalemate
                return true;
            }

            // Neither stalemate nor checkmate
            return false;
        }
        private bool isKingChecked(string color)
        {
            if (color == "White") return whiteKing.isChecked();
            return blackKing.isChecked();
        }
        private void updateIfPawnMoved(string color, int newRow, int newCol, int selRow)
        {
            if (((Pawn)board[newRow, newCol]).turnState == Pawn.TurnState.didntDo)
            {
                if (color == "White")
                {
                    if (newRow == selRow + 2)
                    {
                        ((Pawn)board[newRow, newCol]).pawnMoved(true);
                        //newRow because we need the row before the one the pawn moved to. (The API counts from 1-8)
                        technicalEnPassent = (char)('a' + newCol) + "" + (newRow);
                    }
                    else if (newRow == selRow + 1)
                    {
                        ((Pawn)board[newRow, newCol]).pawnMoved(false);
                    }
                }
                else if (color == "Black")
                {
                    if (newRow == selRow - 2)
                    {
                        ((Pawn)board[newRow, newCol]).pawnMoved(true);
                        //newRow + 2 because we need the row before the one the pawn moved to. (The API counts from 1-8)
                        technicalEnPassent = (char)('a' + newCol) + "" + (newRow + 2);
                    }
                    else if (newRow == selRow - 1)
                    {
                        ((Pawn)board[newRow, newCol]).pawnMoved(false);
                    }
                }
            }
        }
        private void restorePositions(Piece save, int selRow, int selCol, int newRow, int newCol, bool didCastle) {
            //Restoring the positions to their original positions before the move was made
            if (!didCastle)
            {
                board[selRow, selCol] = board[newRow, newCol];
            }
            else
            {
                if (newCol > selCol)
                {
                    board[selRow, selCol] = board[newRow, selCol + 2];
                    board[newRow, selCol + 2] = null;
                    board[newRow, selCol + 1] = null;
                }
                else
                {
                    board[selRow, selCol] = board[newRow, selCol - 2];
                    board[newRow, selCol - 2] = null;
                    board[newRow, selCol - 1] = null;
                }
            }
            board[newRow, newCol] = save;
            //Resets the coords for both
            board[selRow, selCol].setCoords(selRow, selCol);
            //Resets En passent
            if (board[selRow, selCol] is Pawn && ((Pawn)board[selRow, selCol]).didEnPassent)
            {
                ((Pawn)board[selRow, selCol]).didEnPassent = false;
            }
            if (board[newRow, newCol] != null)
            {
                board[newRow, newCol].setCoords(newRow, newCol);
            }
        }
        private void handlePromotePawn(int row, int col,string color)
        {
            if((color == "White" && row == 7) ||(color == "Black" && row == 0))
            {
                DialogHandler handler = new DialogHandler(this);
                PawnPromotion promote = new PawnPromotion(handler, this, row, col, color, board);
                promote.Start();
            }
        }
        private void resetJustPawnJumped(string color)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (board[row, col] is Pawn && board[row, col].color == color && ((Pawn)board[row, col]).turnState == Pawn.TurnState.justJumped)
                    {
                        ((Pawn)board[row, col]).turnState = Pawn.TurnState.cantJump;
                    }
                }
            }
        }
        public bool canOnePieceMove(string color)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (board[row, col] != null && board[row, col].color == color)
                    {
                        // Check all legal moves for this piece
                        for (int newRow = 0; newRow < 8; newRow++)
                        {
                            for (int newCol = 0; newCol < 8; newCol++)
                            {
                                // Check if the move is legal
                                if (board[row, col].isLegalMove(newRow, newCol))
                                {
                                    // Make a temporary move to simulate the effect
                                    Piece tempPiece = board[newRow, newCol];
                                    board[newRow, newCol] = board[row, col];
                                    board[newRow, newCol].setCoords(newRow, newCol);
                                    board[row, col] = null;

                                    bool isCheckAfterMove = isKingChecked(color);

                                    // Undo the temporary move
                                    board[row, col] = board[newRow, newCol];
                                    board[row, col].setCoords(row, col);
                                    board[newRow, newCol] = tempPiece;

                                    if (isCheckAfterMove)
                                    {
                                        return false;
                                    }
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        public string convertBoardToFEN() {
            string FEN = "";
            for(int row = 7; row >= 0; row--)
            {
                int emptyCol = 0;
                for (int col = 0; col < 8; col++)
                {
                    if (board[row, col] != null) { 
                        if(emptyCol > 0)
                        {
                            FEN += emptyCol.ToString();
                            emptyCol = 0;
                        }
                        char pieceName;
                        if ((board[row, col].name) == "Knight")
                        {
                            if (board[row,col].color == "White")
                            {
                                pieceName = 'N';
                            }
                            else
                            {
                                pieceName = 'n';
                            }
                        }
                        else
                        {
                            if (board[row, col].color == "White")
                            {
                                pieceName = board[row, col].name[0];
                            }
                            else
                            {
                                pieceName = board[row, col].name.ToLower()[0];
                            }
                        }
                        FEN += pieceName;
                    }
                    else
                    {
                        emptyCol++;
                    }
                }
                if(emptyCol != 0)
                {
                    FEN += emptyCol;
                }
                if(row != 0)
                {
                    FEN += "/";
                }
            }
            if(currentTurn == turns.White)
            {
                FEN += " w";
            }
            else
            {
                FEN += " b";
            }
            //Castling
            updateCastlingString();
            if (castlingSatus.Equals("nothing"))
            {
                FEN += " - ";
            }
            else
            {
                FEN += " " + castlingSatus + " ";
            }
            //EnPassent
            if (technicalEnPassent.Equals("nothing"))
            {
                FEN += "- ";
            }
            else
            {
                FEN += technicalEnPassent + " ";
            }
            FEN += "0 1";
            return FEN;
        }
    }
}