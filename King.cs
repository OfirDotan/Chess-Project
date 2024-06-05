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
    public class King : Piece {
        public bool didMove { get; set; }
        public King(string color, int row, int col, Piece[,] board) : base("King", color, row, col, 0, board)
        {
            this.name = "King";
            this.didMove = false;
        }
        public override bool isLegalMove(int newRow, int newCol)
        {
            //Makes sure that no piece can eat a king or eat a piece of its own color
            if (board[newRow, newCol] != null &&  board[newRow, newCol].color == this.color)
            {
                return false;
            }
            if (newRow > this.row + 1 || newRow < this.row - 1) { return false; }
            if (newCol > this.col + 1 || newCol < this.col - 1) { return false; }
            return true;
        }
        public bool isChecked(){
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board[i, j] != null && board[i,j].color != this.color && board[i, j].isLegalMove(row, col)) {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool canCastle(int newRow, int newCol)
        {
            if(newRow != this.row) { return false; }
            if (newCol > this.col)
            {
                for(int i = col + 1; i < newCol; i++)
                {
                    if (board[this.row, i] != null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (int i = newCol; i < col - 1; i++)
                {
                    if (board[this.row, i] != null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool castleQueenSide()
        {
            if(didMove == true)
            {
                return false;
            }
            if (!(board[row, 0] is Rook)){
                return false;
            }
            if (((Rook)board[row,0]).didMove == true)
            {
                return false;
            }
            return true;
        }
        public bool castleKingSide()
        {
            if (didMove == true)
            {
                return false;
            }
            if (!(board[row, 7] is Rook))
            {
                return false;
            }
            if (((Rook)board[row, 7]).didMove == true)
            {
                return false;
            }
            return true;
        }
        public bool isKingStuck()
        {
            for (int i = -1; i < 2;i++) { 
                for(int j = -1; j < 2; j++) {
                    //The if makes sure that the check function won't happen to the king's current location
                    if (!(i == 0 && j == 0)) {
                        int saveRow = row;
                        int saveCol = col;

                        int tempRow = saveRow + i;
                        int tempCol = saveCol + j;
                        if (tempRow < 8 && -1 < tempRow) { 
                            if(tempCol < 8 && -1 < tempCol)
                            {
                                if (board[tempRow, tempCol] == null || board[tempRow, tempCol].color != this.color) {
                                    Piece save = board[tempRow, tempCol];
                                    board[tempRow, tempCol] = board[saveRow, saveCol];
                                    board[tempRow, tempCol].setCoords(tempRow, tempCol);
                                    board[saveRow, saveCol] = null;
                                    if (!isChecked())
                                    {
                                        board[saveRow, saveCol] = board[tempRow, tempCol];
                                        board[saveRow, saveCol].setCoords(saveRow, saveCol);
                                        board[tempRow, tempCol] = save;
                                        return false;
                                    }


                                    board[saveRow, saveCol] = board[tempRow, tempCol];
                                    board[saveRow, saveCol].setCoords(saveRow, saveCol);
                                    board[tempRow, tempCol] = save;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}