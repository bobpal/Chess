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
            if (game.offensiveTeam == "light")
            {
                game.displayArray[spot.x, 7].top.Source = game.lQueen;
                game.pieceArray[spot.x, spot.y] = new Queen(spot, "light");
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dQueen;
                game.pieceArray[spot.x, spot.y] = new Queen(spot, "dark");
            }
            this.Close();
        }

        private void RookBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveTeam == "light")
            {
                game.displayArray[spot.x, 7].top.Source = game.lRook;
                game.pieceArray[spot.x, spot.y] = new Rook(spot, "light", false);
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dRook;
                game.pieceArray[spot.x, spot.y] = new Rook(spot, "dark", false);
            }
            this.Close();
        }

        private void BishopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveTeam == "light")
            {
                game.displayArray[spot.x, 7].top.Source = game.lBishop;
                game.pieceArray[spot.x, spot.y] = new Bishop(spot, "light");
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dBishop;
                game.pieceArray[spot.x, spot.y] = new Bishop(spot, "dark");
            }
            this.Close();
        }

        private void KnightBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveTeam == "light")
            {
                game.displayArray[spot.x, 7].top.Source = game.lKnight;
                game.pieceArray[spot.x, spot.y] = new Knight(spot, "light");
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dKnight;
                game.pieceArray[spot.x, spot.y] = new Knight(spot, "dark");
            }
            this.Close();
        }
    }
}
