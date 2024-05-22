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
    public class Queen : Piece
    {
        public Queen(string color, int row, int col, Piece[,] board) : base("Queen", color, row, col, 9, board)
        {
            this.name = "Queen";
        }
        public override bool isLegalMove(int newRow, int newCol)
        {
            //Makes sure that no piece can eat a king or eat a piece of its own color
            if (board[newRow, newCol] != null && board[newRow, newCol].color == this.color)
            {
                return false;
            }
            //Checks that there is no row in between that has a piece so the rook won't jump over pieces
            if (newCol == this.col)
            {
                if (newRow > this.row)
                {
                    for (int i = row + 1; i < newRow; i++)
                    {
                        if (board[i, col] != null)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    for (int i = row - 1; newRow < i; i--)
                    {
                        if (board[i, col] != null)
                        {
                            return false;
                        }
                    }
                }
            }
            //Checks that there is no col in between that has a piece so the rook won't jump over pieces
            if (newRow == this.row)
            {
                if (newCol > this.col)
                {
                    for (int i = col + 1; i < newCol; i++)
                    {
                        if (board[row, i] != null)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    for (int i = col - 1; newCol < i; i--)
                    {
                        if (board[row, i] != null)
                        {
                            return false;
                        }
                    }
                }
            }
            if (newRow == row || newCol == col)
            {
                return true;
            }

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
            if (row > newRow)
            {
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