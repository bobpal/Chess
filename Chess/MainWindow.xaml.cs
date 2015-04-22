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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chess
{
    public partial class MainWindow : Window
    {
        private Logic game;
        
        public MainWindow()
        {
            InitializeComponent();
            game = new Logic(this);
            game.createDisplayArray();
            game.initializeDlls();
            this.Show();

            if (System.IO.File.Exists(game.filePath))
            {
                game.loadState();
            }

            else
            {
                game.newGame();
            }
        }

        private void newGameMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void undoMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void exitMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void themeMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void sizeMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void lastMoveMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rotateMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void saveMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cell_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cell_MouseMove(object sender, RoutedEventArgs e)
        {

        }

        private void Board_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
