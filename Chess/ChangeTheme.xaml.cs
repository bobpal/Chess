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
            previewBox = game.lKing;

            for (int i = 0; i < game.themeList.Count(); i++)
            {
                themeBox.Items.Add(game.themeList[i].GetName().Name);

                if (i == index)
                {
                    themeBox.Text = game.themeList[i].GetName().Name;
                }
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
            this.Close();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
