using System;
using System.Globalization;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace Chess
{
    public partial class NewGame : Window
    {
        private Logic game;

        public NewGame(Logic l)
        {
            InitializeComponent();
            game = l;
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
                        game.mWindow.addChat();
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
                game.difficulty = (int)AI.Value;
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

        private void portBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }

    public class SliderToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((int)(double)value)
            {
                case 1:
                    return "Novice";
                case 2:
                    return "Apprentice";
                case 3:
                    return "Veteran";
                case 4:
                    return "Expert";
                case 5:
                    return "Master";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((string)value)
            {
                case "Novice":
                    return 1.0;
                case "Apprentice":
                    return 2.0;
                case "Veteran":
                    return 3.0;
                case "Expert":
                    return 4.0;
                case "Master":
                    return 5.0;
                default:
                    return 0.0;
            }
        }
    }
}
