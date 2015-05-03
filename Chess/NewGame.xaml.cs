using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace Chess
{
    public partial class NewGame : Window
    {
        private Logic game;

        public NewGame(Logic l)
        {
            InitializeComponent();
            this.game = l;
            ipBox.Text = game.IP;
            portBox.Text = game.port.ToString();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            //if going from 2Player game to 1Player game and opponent is on bottom
            if (game.onePlayer == false && onePlayerBtn.IsChecked.Value == true && game.offensiveTeam == game.opponent && game.rotate == true)
            {
                game.rotateBoard(true, 0);
            }

            if (darkBtn.IsChecked == true)
            {
                game.offensiveTeam = "dark";
                game.opponent = "light";
                game.setBoardForNewGame();
            }
            else
            {
                game.offensiveTeam = "light";
                game.opponent = "dark";
                game.setBoardForNewGame();
            }
            
            if (networkBtn.IsChecked == true)
            {
                if (game.client.Connected == false)
                {
                    game.IP = ipBox.Text;
                    game.port = System.Convert.ToInt32(portBox.Text);
                    game.client = new TcpClient(game.IP, game.port);
                    game.nwStream = game.client.GetStream();
                }
                //already connected to server
                else
                {
                    byte[] ng = new byte[1] { 2 };
                    game.nwStream.Write(ng, 0, 1);
                }
                Connecting connect = new Connecting(game);
                connect.ShowDialog();

                game.listenToServer();
            }

            if (networkBtn.IsChecked == false || game.client.Connected == true)
            {
                game.onePlayer = onePlayerBtn.IsChecked.Value;
                game.networkGame = networkBtn.IsChecked.Value;
                game.medMode = mediumBtn.IsChecked.Value;
                game.hardMode = hardBtn.IsChecked.Value;
                game.history.Clear();
                game.clearToAndFrom();
                game.clearSelectedAndPossible();
                game.movablePieceSelected = false;
                game.ready = true;
                this.Close();
            }
        }

        private void portBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
