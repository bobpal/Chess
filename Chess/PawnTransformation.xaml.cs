﻿using System.Windows;
using static Chess.Logic;

namespace Chess
{
    public partial class PawnTransformation : Window
    {
        private Logic.coordinate spot;
        private Logic game;

        public PawnTransformation(Logic.coordinate c, Logic l)
        {
            InitializeComponent();
            this.MouseDown += delegate { DragMove(); };
            this.spot = c;
            this.game = l;
            this.Owner = game.mWindow;
        }

        private void QueenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveColor == Color.Light)
            {
                game.displayArray[spot.x, 7].top.Source = game.lQueen;
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dQueen;
            }
            game.pieceArray[spot.x, spot.y].job = Job.Queen;
            this.Close();
        }

        private void RookBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveColor == Color.Light)
            {
                game.displayArray[spot.x, 7].top.Source = game.lRook;
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dRook;
            }
            game.pieceArray[spot.x, spot.y].job = Job.Rook;
            this.Close();
        }

        private void BishopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveColor == Color.Light)
            {
                game.displayArray[spot.x, 7].top.Source = game.lBishop;
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dBishop;
            }
            game.pieceArray[spot.x, spot.y].job = Job.Bishop;
            this.Close();
        }

        private void KnightBtn_Click(object sender, RoutedEventArgs e)
        {
            if (game.offensiveColor == Color.Light)
            {
                game.displayArray[spot.x, 7].top.Source = game.lKnight;
            }
            else
            {
                game.displayArray[spot.x, 0].top.Source = game.dKnight;
            }
            game.pieceArray[spot.x, spot.y].job = Job.Knight;
            this.Close();
        }
    }
}
