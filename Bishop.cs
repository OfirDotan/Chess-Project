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
    public class Bishop : Piece
    {
        public Bishop(string color, int row, int col, Piece[,] board) : base("Bishop", color, row, col, 3, board)
        {
            this.name = "Bishop";
        }
        public override bool isLegalMove(int newRow, int newCol)
        {
            //Makes sure that no piece can eat a king or eat a piece of its own color
            if (board[newRow, newCol] != null && board[newRow, newCol].color == this.color)
            {
                return false;
            }
            if (newRow == row || newCol == col) { return false; }

            if (Math.Abs(newRow - row) != Math.Abs(newCol - col))
            {
                return false;
            }
            int k = -1;
            if (newRow > row)
            {
                k = 1;
            }
            int j = -1;
            if (newCol > col)
            {
                j = 1;
            }
            int tempRow = newRow;
            int tempCol = newCol;

            int big = newRow;
            int small = row;
            if (row > newRow) {
                big = row;
                small = newRow;
            }
            for (int i = small - 1; i < big; i++)
            {
                tempRow = tempRow - k;
                tempCol = tempCol - j;
                if (tempRow == row && tempCol == col) { return true; }
                if (tempRow == row && tempCol == col) { return true; }
                if (board[tempRow, tempCol] != null) { return false; }
            }
            return false;
        }
    }
}