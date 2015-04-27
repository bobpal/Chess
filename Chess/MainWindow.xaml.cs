using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
            game.newGame();
        }

        private void undoMenu_Click(object sender, RoutedEventArgs e)
        {
            game.undo();
        }

        private void exitMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void themeMenu_Click(object sender, RoutedEventArgs e)
        {
            ChangeTheme change = new ChangeTheme(game);
            change.ShowDialog();
        }

        private void sizeMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Height = 721;
            this.Width = 728;
        }

        private void lastMoveMenu_Click(object sender, RoutedEventArgs e)
        {
            if(lastMoveMenu.IsChecked == false)
            {
                game.clearToAndFrom();
            }
            game.lastMove = lastMoveMenu.IsChecked;
        }

        private void rotateMenu_Click(object sender, RoutedEventArgs e)
        {
            game.rotateMenuOption();
        }

        private void saveMenu_Click(object sender, RoutedEventArgs e)
        {
            game.saveGame = saveMenu.IsChecked;
        }

        private void cell_Click(object sender, RoutedEventArgs e)
        {
            Canvas cell = (sender as Canvas);
            int row = Grid.GetRow(cell);
            int col = Grid.GetColumn(cell);
            game.clicker(new Logic.coordinate(col, row));
        }

        private void cell_MouseMove(object sender, RoutedEventArgs e)
        {
            if (game.ready == true)  //if a game is being or has been played
            {
                Canvas cell = (sender as Canvas);
                int row = Grid.GetRow(cell);
                int col = Grid.GetColumn(cell);

                if (cell.Background == Brushes.LawnGreen)
                {
                    
                    cell.Cursor = Cursors.Hand;
                }

                else if (game.pieceArray[col, row].color == game.offensiveTeam)
                {
                    cell.Cursor = Cursors.Hand;
                }

                else if (cell.Background == Brushes.DarkOrange)
                {
                    cell.Cursor = Cursors.Hand;
                }

                else
                {
                    cell.Cursor = Cursors.Arrow;
                }
            }
        }

        private void Board_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            game.saveState();
        }
    }
}
