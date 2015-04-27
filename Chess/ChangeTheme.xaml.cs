using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Chess
{
    public partial class ChangeTheme : Window
    {
        private Logic game;
        private int index;

        public ChangeTheme(Logic l)
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
        }

        private void ComboBox_Changed(object sender, SelectionChangedEventArgs args)
        {
            index = themeBox.SelectedIndex;
            previewBox.Source = new BitmapImage(new Uri("pack://application:,,,/" + themeBox.SelectedItem.ToString() + ";component/lKing.png"));
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            game.themeIndex = index;
            game.changeThemeInternally();

            if (game.pieceArray != null)
            {
                game.changeThemeVisually();
            }
            this.Close();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
