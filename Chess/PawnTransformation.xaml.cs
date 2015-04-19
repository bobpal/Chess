using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Chess
{
    public partial class PawnTransformation : Window
    {
        private Logic.coordinate spot;
        private Logic game;

        public PawnTransformation(Logic.coordinate c, Logic l)
        {
            InitializeComponent();
            this.spot = c;
            this.game = l;
        }

        private void QueenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveTeam == "light")
            {
                game.displayArray[spot.x, spot.y].top = game.lQueen;
            }
            else
            {
                game.displayArray[spot.x, spot.y].top = game.dQueen;
            }
            game.pieceArray[spot.x, spot.y].job = "Queen";
            this.Close();
        }

        private void RookBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveTeam == "light")
            {
                game.displayArray[spot.x, spot.y].top = game.lRook;
            }
            else
            {
                game.displayArray[spot.x, spot.y].top = game.dRook;
            }
            game.pieceArray[spot.x, spot.y].job = "Queen";
            this.Close();
        }

        private void BishopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveTeam == "light")
            {
                game.displayArray[spot.x, spot.y].top = game.lBishop;
            }
            else
            {
                game.displayArray[spot.x, spot.y].top = game.dBishop;
            }
            game.pieceArray[spot.x, spot.y].job = "Queen";
            this.Close();
        }

        private void KnightBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveTeam == "light")
            {
                game.displayArray[spot.x, spot.y].top = game.lKnight;
            }
            else
            {
                game.displayArray[spot.x, spot.y].top = game.dKnight;
            }
            game.pieceArray[spot.x, spot.y].job = "Queen";
            this.Close();
        }
    }
}
