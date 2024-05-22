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
    public class EatBar{
        private FrameLayout eatBarLayout;
        private TextView tvPoint;
        private int pointCount;
        private int marginToAdd;
        private string barColor;
        public EatBar(FrameLayout eatBarLayout, TextView tvPoint, string barColor)
        {
            this.eatBarLayout = eatBarLayout;
            this.tvPoint = tvPoint;
            this.pointCount = 0;
            this.marginToAdd = 0;
            this.barColor = barColor;
            this.tvPoint.Text = "";
        }
        public void addEat(String pieceName, int piecePoint, ImageView newPieceImage)
        {
            int imageID = getImageID(pieceName);

            ViewGroup.MarginLayoutParams pieceParams = (new ViewGroup.MarginLayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));
            pieceParams.LeftMargin = marginToAdd;
            newPieceImage.LayoutParameters = pieceParams;
            newPieceImage.SetBackgroundResource(imageID);
            eatBarLayout.AddView(newPieceImage);

            marginToAdd += 20;
            pointCount += piecePoint;

            ViewGroup.MarginLayoutParams tvParams = (ViewGroup.MarginLayoutParams)tvPoint.LayoutParameters;
            tvParams.LeftMargin = marginToAdd + 90;
            tvPoint.LayoutParameters = tvParams;
            tvPoint.Text = "+" + pointCount.ToString();
        }
        public int getImageID(string pieceName)
        {
            if (this.barColor.Equals("White")){
                switch(pieceName)
                {
                    case "Pawn":
                        return Resource.Drawable.BlackPawn;
                    case "Knight":
                        return Resource.Drawable.BlackKnight;
                    case "Bishop":
                        return Resource.Drawable.BlackBishop;
                    case "Rook":
                        return Resource.Drawable.BlackRook;
                    case "Queen":
                        return Resource.Drawable.BlackQueen;
                }
                return 0;
            }
            else
            {
                switch(pieceName)
                {
                    case "Pawn":
                        return Resource.Drawable.WhitePawn;
                    case "Knight":
                        return Resource.Drawable.WhiteKnight;
                    case "Bishop":
                        return Resource.Drawable.WhiteBishop;
                    case "Rook":
                        return Resource.Drawable.WhiteRook;
                    case "Queen":
                        return Resource.Drawable.WhiteQueen;
                }
                return 0;
            }
        }
    }
}