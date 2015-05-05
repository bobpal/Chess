using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Chess
{
    public partial class NewGame : Window
    {
        private Logic game;

        public NewGame(Logic l)
        {
            InitializeComponent();
            this.game = l;
            this.Owner = game.mWindow;
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
            if (game.onePlayer == false && onePlayerBtn.IsChecked.Value == true &&
                game.offensiveTeam == game.opponent && game.rotate == true)
            {
                game.rotateBoard(true, 0);
            }

            if (darkBtn.IsChecked == true)
            {
                game.offensiveTeam = "dark";
                game.opponent = "light";
            }
            else
            {
                game.offensiveTeam = "light";
                game.opponent = "dark";
            }
            
            if (networkBtn.IsChecked == true)
            {
                if (game.client == null)
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
                addChat();
                game.continuousReader();
            }
            //always unless clicked cancel on Connecting
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

                if (game.buffer[0] != 2)
                {
                    game.ready = true;
                }
                this.Close();
            }
        }

        private void addChat()
        {
            game.mWindow.Board.Width += 300;
            game.mWindow.chat.Visibility = Visibility.Visible;
            game.mWindow.split.Visibility = Visibility.Visible;

            ColumnDefinition c1 = new ColumnDefinition();
            c1.Width = new GridLength(5, GridUnitType.Pixel);
            game.mWindow.space.ColumnDefinitions.Add(c1);

            ColumnDefinition c2 = new ColumnDefinition();
            c2.Width = new GridLength(295, GridUnitType.Star);
            game.mWindow.space.ColumnDefinitions.Add(c2);
        }

        private void portBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
