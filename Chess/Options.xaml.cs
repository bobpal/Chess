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
            this.Owner = game.mWindow;
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
            durationSlider.IsEnabled = game.twoPlayer == true && game.rotate == true;
            rotateBtn.IsEnabled = game.twoPlayer == true;
            rotateBtn.IsChecked = game.rotate;
            lastMoveBtn.IsChecked = game.lastMove;
            saveGameBtn.IsChecked = game.saveGame;
            durationSlider.Value = game.rotationDuration;

            if(game.networkGame == true)
            {
                rotateBtn.IsEnabled = false;
                saveGameBtn.IsEnabled = false;
                durationSlider.IsEnabled = false;
            }
        }

        private void ThemeBox_Changed(object sender, SelectionChangedEventArgs args)
        {
            index = themeBox.SelectedIndex;
            if(index == 0)
            {
                previewBox.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/lKing.png"));
            }
            else
            {
                previewBox.Source = new BitmapImage(new Uri("pack://application:,,,/" + themeBox.SelectedItem.ToString() + ";component/lKing.png"));
            }
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
            durationSlider.IsEnabled = rotateBtn.IsChecked.Value;
        }

        private void Duration_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            rDuration = durationSlider.Value;
        }
    }
}
