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
    public class Pawn : Piece
    {
        public enum TurnState { didntDo, justJumped, cantJump}
        public TurnState turnState { get; set; }
        public bool didEnPassent { get; set; }
        public Pawn(string color, int row, int col, Piece[,] board) : base("Pawn", color, row, col, 1, board)
        {
            turnState = TurnState.didntDo;
            didEnPassent = false;
            this.name = "Pawn";
        }
        public override bool isLegalMove(int newRow, int newCol)
        {
            //No white pawn should exist on the 8th row since they were promoted and this check might have happened before the promotion
            if (this.color == "White" && this.row == 7)
            {
                return false;
            }
            //No black pawn should exist on the 1st row since they were promoted and this check might have happened before the promotion
            if (this.color == "Black" && this.row == 0)
            {
                return false;
            }

            //En Passent
            if (board[newRow, newCol] == null && this.col != newCol)
            {
                int aboveOrBelow = -1;
                if(this.color == "Black")
                {
                    aboveOrBelow = 1;
                }
                bool isInRangeOfEnPassent = false;
                if((newRow == 2 || newRow == 5) && newRow + aboveOrBelow == row)
                {
                    isInRangeOfEnPassent = true;
                }
                //newRow - 1 can never be 0
                if (isInRangeOfEnPassent && board[newRow + aboveOrBelow, newCol] is Pawn && (((Pawn)board[newRow + aboveOrBelow, newCol]).turnState == TurnState.justJumped) && board[newRow + aboveOrBelow, newCol].color != this.color)
                {
                    didEnPassent = true;
                    return true;
                }
            }
            //else if(this.color == "Black" && board[newRow, newCol] == null) {
            //{

            //}
            //Makes sure that no piece can eat a king or eat a piece of its own color
            if (board[newRow, newCol] != null && board[newRow, newCol].color == this.color) {
                return false;
            }
            if (this.color == "White") {
                //Makes sure that the pawn can't eat what's in front of it
                if (newCol == this.col && board[newRow, newCol] != null && board[newRow, newCol].color == "Black") { return false; }
                //No going back and no further than 2 spaces
                if (newRow < this.row || newRow > this.row + 2) { return false; }
                //Making sure the pawn can't go to the left or right
                if (newRow == row && (newCol == col - 1 || newCol == col + 1)) { return false; }
                //No more than one col to the sides
                if (newCol < this.col - 1 || newCol > this.col + 1) { return false; }
                //Checking if there is a piece on the col that is not the current one and keeping the pawn from eating the king
                if (newCol != this.col && board[newRow, newCol] == null) { return false; }
                //Making sure that a pawn can only jump two squares on the first turn
                if (!(turnState == TurnState.didntDo) && newRow > this.row + 1) { return false; }
                //Making sure that a pawn can't jump over a piece
                if (turnState == TurnState.didntDo && newRow > this.row + 1 && this.col == newCol && board[this.row + 1, col] != null) { return false; }
            }
            else
            {
                //Makes sure that the pawn can't eat what's in front of it
                if (newCol == this.col && board[newRow, newCol] != null && board[newRow, newCol].color == "White") { return false; }
                //No going back and no further than 2 spaces
                if (newRow > this.row || newRow < this.row - 2) { return false; }
                //Making sure the pawn can't go to the left or right
                if (newRow == row && (newCol == col - 1 || newCol == col + 1)) { return false; }
                //No more than one col to the sides
                if (newCol < this.col - 1 || newCol > this.col + 1) { return false; }
                //Checking if there is a piece on the col that is not the current one
                if (newCol != this.col && board[newRow, newCol] == null) { return false; }
                //Making sure that a player can only jump two squares on the first turn
                if (!(turnState == TurnState.didntDo) && newRow < this.row - 1) { return false; }
                //Making sure that a pawn can't jump over a piece
                if (turnState == TurnState.didntDo && newRow < this.row - 1 && this.col == newCol && board[this.row - 1, col] != null) { return false; }
            }
            return true;
        }
        public void pawnMoved(bool isAJump)
        {
            if (isAJump) {
                if (turnState == TurnState.didntDo)
                {
                    turnState = TurnState.justJumped;
                }
                else if (turnState == TurnState.justJumped)
                {
                    turnState = TurnState.cantJump;
                }
            }
            else
            {
                if (turnState == TurnState.didntDo)
                {
                    turnState = TurnState.cantJump;
                }
                else if (turnState == TurnState.justJumped)
                {
                    turnState = TurnState.cantJump;
                }
            }
        }
    }
}