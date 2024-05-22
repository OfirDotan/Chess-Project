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
    public class BoardSetupActions
    {
        public static void resetBoard(Piece[,] board)
        {
            int evenOdd = 0;
            //Makes the board by using two for loops, that checks the row and acts accordingly
            //The even odd counter switches for each column and whenever a row changes it resets to the same parity of the last column in the row before
            //During rows 1,2,7 and 8 the even odd counter does matter, so it gets ruined until the next row
            for (int rows = 0; rows < 8; rows++)
            {
                for (int columns = 0; columns < 8; columns++)
                {
                    if (rows == 0 || rows == 7)
                    {
                        //A temp int to decide what color to fill the pawns based on the row number; 0 - White, 1 - Black
                        string tempColor = "White";
                        if (rows == 7)
                        {
                            tempColor = "Black";
                        }
                        fillFirstRow(tempColor, columns, rows, board);
                    }
                    else if (rows == 1 || rows == 6)
                    {
                        //A temp int to decide what color to fill the pawns based on the row number; 0 - White, 1 - Black
                        string tempColor = "White";
                        if (rows == 6)
                        {
                            tempColor = "Black";
                        }
                        board[rows, columns] = new Pawn(tempColor, rows, columns, board);
                    }
                    else {
                        board[rows, columns] = null;
                    }

                }
                if (evenOdd == 0)
                {
                    evenOdd = 1;
                }
                else
                {
                    evenOdd = 0;
                }
            }
        }
        //Creates a piece and puts a type by the given name according to the even odd / white black and returns 1 if the even needs to be turned to odd or 0 if odd needs to be returned to even
        public static int fillPawn(int color, int columns, int rows, Piece[,] board)
        {
            if (color == 0)
            {
                board[rows,columns] = new Pawn("White", rows, columns, board);
                return 1;
            }
            else
            {
                board[rows,columns] = new Pawn("Black", rows, columns, board);
                return 0;
            }
        }
        public static void fillFirstRow(string color, int columns, int rows, Piece[,] board)
        {
            if (columns == 0 || columns == 7)
            {
                board[rows,columns] = new Rook(color, rows, columns, board);
            }
            else if (columns == 1 || columns == 6)
            {
                board[rows,columns] = new Knight(color, rows, columns, board);
            }
            else if (columns == 2 || columns == 5)
            {
                board[rows,columns] = new Bishop(color, rows, columns, board);
            }
            else if (columns == 3)
            {
                board[rows,columns] = new Queen(color, rows, columns, board);
            }
            else if (columns == 4)
            {
                board[rows,columns] = new King(color, rows, columns, board);
            }
        }
    }
}