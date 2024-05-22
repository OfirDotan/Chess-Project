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
    public class Rook : Piece
    {
        public bool didMove { get; set; }
        public Rook(string color, int row, int col, Piece[,] board) : base("Rook", color, row, col, 5, board)
        {
            this.name = "Rook";
            this.didMove = false;
        }
        public override bool isLegalMove(int newRow, int newCol)
        {
            //Makes sure that no piece can eat a king or eat a piece of its own color
            if (board[newRow, newCol] != null && board[newRow, newCol].color == this.color)
            {
                return false;
            }
            //Makes sure that the selected position isn't diagonal
            if (newRow != this.row && newCol != this.col) { return false; }
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
                else {
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

            return true;
        }
    }
}