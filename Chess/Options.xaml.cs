using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Chess
{
    public partial class Options : Window
    {
        private Logic game;
        private int index;
        private double rDuration;

        public Options(Logic l)
        {
            InitializeComponent();
            this.game = l;
            index = game.themeIndex;
            populate();
        }

        private void populate()
        {
            for (int i = 0; i < game.themeList.Count(); i++)
            {
                themeBox.Items.Add(game.themeList[i]);

                if (i == index)
                {
                    themeBox.Text = game.themeList[i];
                }
            }
            previewBox.Source = game.lKing;
            durationBox.IsEnabled = game.onePlayer == false && game.rotate == true;
            rotateBtn.IsEnabled = !game.onePlayer;
            rotateBtn.IsChecked = game.rotate;
            lastMoveBtn.IsChecked = game.lastMove;
            saveGameBtn.IsChecked = game.saveGame;
            durationBox.SelectedIndex = (int)game.rotationDuration;
        }

        private void ThemeBox_Changed(object sender, SelectionChangedEventArgs args)
        {
            index = themeBox.SelectedIndex;
            previewBox.Source = new BitmapImage(new Uri("pack://application:,,,/" + themeBox.SelectedItem.ToString() + ";component/lKing.png"));
        }

        private void DurationBox_Changed(object sender, SelectionChangedEventArgs args)
        {
            rDuration = durationBox.SelectedIndex;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            game.themeIndex = index;
            game.changeThemeInternally();

            if (game.pieceArray != null)
            {
                game.changeThemeVisually();
            }

            if(rotateBtn.IsChecked != game.rotate)
            {
                game.rotateOptionToggled();
            }

            if(lastMoveBtn.IsChecked == false)
            {
                game.clearToAndFrom();
            }
            
            game.saveGame = saveGameBtn.IsChecked.Value;
            game.rotate = rotateBtn.IsChecked.Value;
            game.lastMove = lastMoveBtn.IsChecked.Value;
            game.rotationDuration = rDuration;

            this.Close();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void rotateBtn_Click(object sender, RoutedEventArgs e)
        {
            durationBox.IsEnabled = rotateBtn.IsChecked.Value;
        }
    }
}
