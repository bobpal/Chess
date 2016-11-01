using System.Windows;

namespace Chess
{
    public partial class PawnTransformation : Window
    {
        private Piece.coordinate spot;
        private Logic game;

        public PawnTransformation(Piece.coordinate c, Logic l)
        {
            InitializeComponent();
            this.MouseDown += delegate { DragMove(); };
            this.spot = c;
            this.game = l;
            this.Owner = game.mWindow;
        }

        private void QueenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveTeam == Color.Light)
            {
                game.displayArray[spot.x, 7].top.Source = game.lQueen;
                game.pieceArray[spot.x, spot.y] = new Queen(spot, Color.Light);
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dQueen;
                game.pieceArray[spot.x, spot.y] = new Queen(spot, Color.Dark);
            }
            this.Close();
        }

        private void RookBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveTeam == Color.Light)
            {
                game.displayArray[spot.x, 7].top.Source = game.lRook;
                game.pieceArray[spot.x, spot.y] = new Rook(spot, Color.Light, false);
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dRook;
                game.pieceArray[spot.x, spot.y] = new Rook(spot, Color.Dark, false);
            }
            this.Close();
        }

        private void BishopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveTeam == Color.Light)
            {
                game.displayArray[spot.x, 7].top.Source = game.lBishop;
                game.pieceArray[spot.x, spot.y] = new Bishop(spot, Color.Light);
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dBishop;
                game.pieceArray[spot.x, spot.y] = new Bishop(spot, Color.Dark);
            }
            this.Close();
        }

        private void KnightBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveTeam == Color.Light)
            {
                game.displayArray[spot.x, 7].top.Source = game.lKnight;
                game.pieceArray[spot.x, spot.y] = new Knight(spot, Color.Light);
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dKnight;
                game.pieceArray[spot.x, spot.y] = new Knight(spot, Color.Dark);
            }
            this.Close();
        }
    }
}
