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
            game.tryDlls();
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
    }
}
