using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Chess
{
    public partial class MainWindow : Window
    {
        private Logic game;
        public static RoutedCommand newGameCmd = new RoutedCommand();
        public static RoutedCommand undoCmd = new RoutedCommand();
        public static RoutedCommand themeCmd = new RoutedCommand();
        public static RoutedCommand sizeCmd = new RoutedCommand();
        
        public MainWindow()
        {
            InitializeComponent();
            game = new Logic(this);
            game.createDisplayArray();
            game.initializeDlls();
            newGameCmd.InputGestures.Add(new KeyGesture(Key.F2));
            undoCmd.InputGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Control));
            themeCmd.InputGestures.Add(new KeyGesture(Key.F3));
            sizeCmd.InputGestures.Add(new KeyGesture(Key.F4));
            CommandBindings.Add(new CommandBinding(newGameCmd, newGameMenu_Click));
            CommandBindings.Add(new CommandBinding(undoCmd, undoMenu_Click));
            CommandBindings.Add(new CommandBinding(themeCmd, optionMenu_Click));
            CommandBindings.Add(new CommandBinding(sizeCmd, sizeMenu_Click));
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
            if(undoMenu.IsEnabled == true)
            {
                game.undo();
            }
        }

        private void exitMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void optionMenu_Click(object sender, RoutedEventArgs e)
        {
            Options change = new Options(game);
            change.ShowDialog();
        }

        private void sizeMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Height = 721;
            this.Width = 728;
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
            if (game.ready == true)  //if a game is being played
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
