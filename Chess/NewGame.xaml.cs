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
            bool? canceled = false;
            whoIsOnBottom();
            
            if (networkBtn.IsChecked == true)
            {
                if (game.client == null || game.client.Connected == false)
                {
                    game.IP = ipBox.Text;
                    game.port = System.Convert.ToInt32(portBox.Text);
                    try
                    {
                        game.client = new TcpClient(game.IP, game.port);
                        game.nwStream = game.client.GetStream();
                    }
                    catch(SocketException)
                    {
                        canceled = true;
                        MessageBox.Show("Could not find Server\nCheck the IP Address and port and try again", "Could not find Server",
                            MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                    }
                }
                
                if(canceled == false)
                {
                    Connecting connect = new Connecting(game);
                    canceled = connect.ShowDialog();
                    if (canceled == false)
                    {
                        addChat();
                        game.continuousReader();
                    }
                }
                game.mWindow.undoMenu.IsEnabled = false;
            }
            else
            {
                game.setBoardForNewGame();
            }
            //always unless clicked cancel on Connecting
            if (canceled == false)
            {
                game.onePlayer = onePlayerBtn.IsChecked.Value;
                game.networkGame = networkBtn.IsChecked.Value;
                game.medMode = mediumBtn.IsChecked.Value;
                game.hardMode = hardBtn.IsChecked.Value;
                game.history.Clear();
                game.clearToAndFrom();
                game.clearSelectedAndPossible();
                game.movablePieceSelected = false;

                if (game.offensiveTeam != game.opponent)
                {
                    game.ready = true;
                }
                this.Close();
            }
        }

        private void whoIsOnBottom()
        {
            game.ready = true;

            if (darkBtn.IsChecked == true)
            {
                //if bottom != dark, rotate

                //if no game has been started
                if (game.pieceArray == null)
                {
                    game.rotateBoard(true, 0);
                }
                //coming from 1Player or networkGame
                else if (game.onePlayer == true || game.networkGame == true)
                {
                    if (game.opponent == "dark")
                    {
                        game.rotateBoard(true, 0);
                    }
                }
                //coming from 2Player local
                else
                {
                    if (game.offensiveTeam == game.opponent)
                    {
                        if (game.rotate == true)
                        {
                            if (game.offensiveTeam == "light")
                            {
                                game.rotateBoard(true, 0);
                            }
                        }
                        else
                        {
                            if (game.opponent == "dark")
                            {
                                game.rotateBoard(true, 0);
                            }
                        }
                    }
                    //if firstPlayer's turn
                    else
                    {
                        if (game.offensiveTeam == "light")
                        {
                            game.rotateBoard(true, 0);
                        }
                    }
                }
                game.offensiveTeam = "dark";
                game.opponent = "light";
            }
            else
            {
                //if bottom != light, rotate

                //coming from 1Player or networkGame
                if (game.onePlayer == true || game.networkGame == true)
                {
                    if (game.opponent == "light")
                    {
                        game.rotateBoard(false, 0);
                    }
                }
                //coming from 2Player local
                else
                {
                    if (game.offensiveTeam == game.opponent)
                    {
                        if (game.rotate == true)
                        {
                            if (game.offensiveTeam == "dark")
                            {
                                game.rotateBoard(false, 0);
                            }
                        }
                        else
                        {
                            if (game.opponent == "light")
                            {
                                game.rotateBoard(false, 0);
                            }
                        }
                    }
                    //if firstPlayer's turn
                    else
                    {
                        if (game.offensiveTeam == "dark")
                        {
                            game.rotateBoard(false, 0);
                        }
                    }
                }
                game.offensiveTeam = "light";
                game.opponent = "dark";
            }
            game.ready = false;
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
