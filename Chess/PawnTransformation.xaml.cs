using System.Windows;
using static Chess.Logic;

namespace Chess
{
    public partial class PawnTransformation : Window
    {
        private Piece.coordinate spot;
        private Logic game;
        private int index;

        public PawnTransformation(Piece.coordinate c, Logic l)
        {
            InitializeComponent();
            this.MouseDown += delegate { DragMove(); };
            this.spot = c;
            this.game = l;
            this.Owner = game.mWindow;
            this.index = getIndex(c, l.pieceArray);
        }

        private void QueenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveTeam == Color.Light)
            {
                game.displayArray[spot.x, 7].top.Source = game.lQueen;
                game.pieceArray[index] = new Queen(spot, Color.Light);
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dQueen;
                game.pieceArray[index] = new Queen(spot, Color.Dark);
            }
            this.Close();
        }

        private void RookBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveTeam == Color.Light)
            {
                game.displayArray[spot.x, 7].top.Source = game.lRook;
                game.pieceArray[index] = new Rook(spot, Color.Light, false);
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dRook;
                game.pieceArray[index] = new Rook(spot, Color.Dark, false);
            }
            this.Close();
        }

        private void BishopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveTeam == Color.Light)
            {
                game.displayArray[spot.x, 7].top.Source = game.lBishop;
                game.pieceArray[index] = new Bishop(spot, Color.Light);
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dBishop;
                game.pieceArray[index] = new Bishop(spot, Color.Dark);
            }
            this.Close();
        }

        private void KnightBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveTeam == Color.Light)
            {
                game.displayArray[spot.x, 7].top.Source = game.lKnight;
                game.pieceArray[index] = new Knight(spot, Color.Light);
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dKnight;
                game.pieceArray[index] = new Knight(spot, Color.Dark);
            }
            this.Close();
        }
    }
}
