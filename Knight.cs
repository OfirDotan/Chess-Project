using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Security.Cert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinalProjectChess
{
    public class Knight : Piece
    {
        public Knight(string color, int row, int col, Piece[,] board) : base("Knight", color, row, col, 3, board)
        {
            this.name = "Knight";
        }
        public override bool isLegalMove(int newRow, int newCol)
        {
            //Makes sure that no piece can eat a king or eat a piece of its own color
            if (board[newRow, newCol] != null && board[newRow, newCol].color == this.color)
            {
                return false;
            }
            //Upper & Bottom Right Positions
            if (newCol == col + 1 && (newRow == row + 2 || newRow == row - 2)) return true;
            //Upper & Bottom Left Positions
            if (newCol == col - 1 && (newRow == row + 2 || newRow == row - 2)) return true;
            //Right Side Positions
            if (newCol == col + 2 && (newRow == row + 1 || newRow == row - 1)) return true;
            //Left Side Positions
            if (newCol == col - 2 && (newRow == row + 1 || newRow == row - 1)) return true;

            return false;
        }
    }
}