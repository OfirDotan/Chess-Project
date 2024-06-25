using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Thread = System.Threading.Thread;

namespace FinalProjectChess
{
    [Activity(Label = "MultiplayerActivity")]
    public class MultiplayerActivity : Activity
    {
        volatile bool playerTurn;
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
        bool shouldExitThread;
        static Android.OS.Handler handler = new Android.OS.Handler();

        enum turns
        {
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

        [System.Obsolete]
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_multiplayer);
            imageViews = new ImageView[8, 8];
            board = new Piece[8, 8];
            eatBars = new EatBar[2];
            selected = "empty";
            currentTurn = turns.White;
            currentPromotionState = promotionState.noPromotionNeeded;
            didGameEnd = false;
            isDone = false;
            TableLayout table = FindViewById<TableLayout>(Resource.Id.tableLayout);
            SetBoardSize(table);

            BoardSetupActions.resetBoard(board);
            whiteKing = ((King)board[0, 4]);
            blackKing = ((King)board[7, 4]);
            turnView = FindViewById<TextView>(Resource.Id.tvTurn);

            string message = ServerCommunication.receive(-1, 350);
            while(message == null)
            {
                message = ServerCommunication.receive(-1, 350);
            }
            if (message.Equals("Starting as White"))
            {
                playerTurn = true;
            }
            else
            {
                playerTurn = false;
            }
            Toast.MakeText(this, message, ToastLength.Short).Show();
            for (int row = 0; row < 8; row++)
            {
                for (char col = 'A'; col < 'I'; col++)
                {
                    int intCol = col - 'A';
                    ImageView pieceImage = FindViewById<ImageView>(Resources.GetIdentifier(col + "" + (row + 1), "id", PackageName));
                    imageViews[row, intCol] = pieceImage;
                    imageViews[row, intCol].Click += select;
                }
            }
            // Create a new thread and pass the method to be executed by the thread
            Thread thread = new Thread(checkForOppMove);
            shouldExitThread = false;

            // Start the thread
            thread.Start();
        }
        public void SetBoardSize(TableLayout tableLayout)
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
        protected override void OnDestroy()
        {
            if (ServerCommunication.socket.Connected)
            {
                ServerCommunication.send("Abandoned");

            }
            ServerCommunication.socket.Close();
            ServerCommunication.socket = null;
            shouldExitThread = true;
            base.OnDestroy();
        }
        void checkForOppMove()
        {
            while (!shouldExitThread)
            {
                if (!playerTurn)
                {
                    string receivedData = ServerCommunication.receive(-1, 300);
                    if (receivedData != null)
                    {
                        if(receivedData == "Abandoned")
                        {
                            handler.Post(() =>
                            {
                                Toast.MakeText(this, "The other player abandoned the match", ToastLength.Short).Show();
                                ServerCommunication.socket.Close();
                                Finish();
                                shouldExitThread = true;
                            });
                        }
                        else
                        {
                            //MovedPieceName#NewRow#NewCol#OldRow#OldCol
                            //Queen#6#4#2#2
                            string[] oppMove = receivedData.Split("#");
                            int newRow = Integer.ParseInt(oppMove[1]);
                            int newCol = Integer.ParseInt(oppMove[2]);
                            int oldRow = Integer.ParseInt(oppMove[3]);
                            int oldCol = Integer.ParseInt(oppMove[4]);
                            Piece eatenPiece = board[newRow, newCol];
                            board[newRow, newCol] = board[oldRow, oldCol];
                            board[newRow, newCol].setCoords(newRow, newCol);
                            board[oldRow, oldCol] = null;
                            handler.Post(() =>
                            {
                                if (eatenPiece != null)
                                {
                                    addToEat(eatenPiece.name, eatenPiece.pointWorth);
                                }
                                imageViews[newRow, newCol].SetImageDrawable(imageViews[oldRow, oldCol].Drawable);
                                imageViews[oldRow, oldCol].SetImageResource(Resource.Drawable.EmptyTile);
                                turnView.Text = currentTurn.ToString();
                                switchTurns();
                                turnView.Text = currentTurn.ToString();
                                playerTurn = !playerTurn;
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
                            });

                        }
                    }
                }
                Thread.Sleep(10);
            }
        }

        private void select(object sender, System.EventArgs e)
        {
            if (playerTurn) { 
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
                            //Changed the selected to the new ID, and gives it the green highlight
                            selected = id;
                            imageViews[id[1] - '1', id[0] - 'A'].SetBackgroundColor(new Color(62, 138, 250, 160));
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
                        string sendTurn = "";
                        Piece eatenPiece = null;
                        string newPos = ((ImageView)sender).ContentDescription;
                        int selCol = selected[0] - 'A';
                        int selRow = selected[1] - '1';
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
                                        restorePositions(save, selRow, selCol, newRow, newCol);
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
                                            addToEat(eatenPiece.name, eatenPiece.pointWorth);
                                        }

                                        imageViews[newRow, newCol].SetImageDrawable(imageViews[selRow, selCol].Drawable);
                                        imageViews[selRow, selCol].SetImageResource(Resource.Drawable.EmptyTile);
                                        //Message should look like this Queen#6#4#2#2
                                        sendTurn = "move#" + newRow + "#" + newCol + '#' + selRow + '#' + selCol;
                                        switchTurns();
                                        playerTurn = !playerTurn;
                                    }
                                }
                            }
                            //If the player isn't checked
                            else
                            {
                                if (board[selRow, selCol].isLegalMove(newRow, newCol) || (board[selRow, selCol] is King && board[newRow, newCol] is Rook && board[selRow, selCol].color == board[newRow, newCol].color))
                                {
                                    Piece save = board[newRow, newCol];
                                    //Makes sure the selected new move is either null or a place with a piece that is not a king
                                    if (board[newRow, newCol] == null || (board[newRow, newCol] != null && !(board[newRow, newCol] is King)))
                                    {
                                        eatenPiece = board[newRow, newCol];
                                        board[newRow, newCol] = board[selRow, selCol];
                                        board[newRow, newCol].setCoords(newRow, newCol);
                                        board[selRow, selCol] = null;
                                    }
                                    check = isKingChecked(currentTurn.ToString());
                                    if (check)
                                    {
                                        //Restoring the positions to their original positions before the move was made and resets en passent
                                        restorePositions(save, selRow, selCol, newRow, newCol);
                                        eatenPiece = null;
                                        Toast.MakeText(this, "Sorry, this move will result in you being checked", ToastLength.Short).Show();
                                    }
                                    else
                                    {
                                        //If did a move that was not en passent, it will remove the option to do the last en passent
                                        if ((board[newRow, newCol] is Pawn) == false || !((Pawn)board[newRow, newCol]).didEnPassent)
                                        {
                                            string color = "White";
                                            if (board[newRow, newCol].color == "White")
                                            {
                                                color = "Black";
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

                                        //Message should look like this Queen#6#4#2#2
                                        sendTurn = "move#" + newRow + "#" + newCol + '#' + selRow + '#' + selCol;
                                        if (eatenPiece != null)
                                        {
                                            addToEat(eatenPiece.name, eatenPiece.pointWorth);
                                        }
                                        imageViews[newRow, newCol].SetImageDrawable(imageViews[selRow, selCol].Drawable);
                                        imageViews[selRow, selCol].SetImageResource(Resource.Drawable.EmptyTile);
                                        switchTurns();
                                        playerTurn = !playerTurn;
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
                        bool checkForCheck;
                        if (currentTurn.ToString() == "White")
                        {
                            checkForCheck = whiteKing.isChecked();
                            if (!checkForCheck && !canPreventCheckOrOnePieceMove("White", false))
                            {
                                //Stalemate
                                didGameEnd = true;
                                sendTurn = "GameEnded%" + sendTurn;
                                Toast.MakeText(this, "White king is Stalemated", ToastLength.Short).Show();
                                endGame("Black", "Stalemate");
                            }
                            //Needs to check if any piece can prevent the check, and if so then it isn't a checkmate
                            if (checkForCheck && whiteKing.isKingStuck() && !canPreventCheckOrOnePieceMove("White", true))
                            {
                                //Checkmate
                                didGameEnd = true;
                                sendTurn = "GameEnded%" + sendTurn;
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
                                sendTurn = "GameEnded%" + sendTurn;
                                Toast.MakeText(this, "Black king is Stalemated", ToastLength.Short).Show();
                                endGame("White", "Stalemate");
                            }
                            //Needs to check if any piece can prevent the check, and if so then it isn't a checkmate
                            if (checkForCheck && blackKing.isKingStuck() && !canPreventCheckOrOnePieceMove("Black", true))
                            {
                                //Checkmate
                                didGameEnd = true;
                                sendTurn = "GameEnded%" + sendTurn;
                                Toast.MakeText(this, "Black king is Checkmated", ToastLength.Short).Show();
                                endGame("White", "Checkmate");
                            }
                        }
                        ServerCommunication.send(sendTurn);
                    }
                }
            }
            else
            {
                Toast.MakeText(this, "The other player hasn't played yet!", ToastLength.Short).Show();
            }
        }
        public void endGame(string color, string wayOfEnding)
        {
            DialogHandler handler = new DialogHandler(this);
            EndGameDialog endGame = new EndGameDialog(handler, this, color, wayOfEnding, false);
            endGame.Start();
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
            if (currentTurn == turns.White)
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
            if (!checkForCheck && !isKingChecked(color) && hasLegalMove)
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
                    }
                    else if (newRow == selRow - 1)
                    {
                        ((Pawn)board[newRow, newCol]).pawnMoved(false);
                    }
                }
            }
        }
        private void restorePositions(Piece save, int selRow, int selCol, int newRow, int newCol)
        {
            //Restoring the positions to their original positions before the move was made
            board[selRow, selCol] = board[newRow, newCol];
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
        private void handlePromotePawn(int row, int col, string color)
        {
            if ((color == "White" && row == 7) || (color == "Black" && row == 0))
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
    }
}