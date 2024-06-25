namespace FinalProjectChess
{
    public abstract class Piece
    {
        //The color of a piece
        public string color { get; }
        public int row { get; set; }
        public int col { get; set; }
        public string name { get; set; }
        public int pointWorth { get; set; }
        public bool canCheck {  get; set; }
        public Piece[,] board;
        public abstract bool isLegalMove(int newRow, int newCol);
        public Piece(string name, string color, int row, int col, int pointWorth, Piece[,] board) { 
            this.color = color;
            this.row = row;
            this.col = col;
            this.pointWorth = pointWorth;
            this.board = board;
        }
        public void setCoords(int row, int col) {
            this.row = row;
            this.col = col;
        }
    }
}