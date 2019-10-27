using System;
using System.Globalization;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using static Chess.Logic;

namespace Chess
{
    public partial class NewGame : Window
    {
        private Logic game;
        private Color colorChecked;

        public NewGame(Logic l)
        {
            InitializeComponent();
            game = l;
            this.Owner = game.mWindow;
            ipBox.Text = game.IP;
            portBox.Text = game.port.ToString();
            NewGameWindow.horse.FlowDirection = FlowDirection.RightToLeft;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void okBtn_Click(object sender, RoutedEventArgs e)
        {
            bool? canceled = false;
            
            if (networkBtn.IsChecked == true)
            {
                if (game.client == null || game.client.Connected == false)
                {
                    game.IP = ipBox.Text;

                    try
                    {
                        game.port = System.Convert.ToInt32(portBox.Text);
                        game.client = new TcpClient(game.IP, game.port);
                        game.nwStream = game.client.GetStream();
                    }
                    catch(Exception)
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
            }
            else //Not Network Game
            {
                game.setBoardForNewGame();
            }

            //Confirmed new game starting
            if (canceled == false)
            {
                if (lightBtn.IsChecked == true)
                {
                    colorChecked = Chess.Color.Light;
                }
                else
                {
                    colorChecked = Chess.Color.Dark;
                }

                game.onePlayer = onePlayerBtn.IsChecked.Value;
                game.twoPlayer = twoPlayerBtn.IsChecked.Value;
                game.networkGame = networkBtn.IsChecked.Value;
                game.difficulty = (int)AI.Value;
                game.offensiveColor = Chess.Color.Light;
                game.mWindow.undoMenu.IsEnabled = false;
                game.history.Clear();
                game.clearToAndFrom();
                game.clearSelectedAndPossible();
                game.movablePieceSelected = false;
                opponentAndRotate();
                game.onBottomColor = game.switchTeam(game.opponentColor);

                if (game.opponentColor == Chess.Color.Dark)
                {
                    game.ready = true;
                }
                else if(game.onePlayer == true)
                {
                    await game.compTurn();
                    game.offensiveColor = Chess.Color.Dark;
                }
                this.Close();
            }
        }

        private void opponentAndRotate()
        {
            //Determines opponent and whether to rotate

            //Allows for rotation
            game.ready = true;

            if (game.onePlayer == true)
            {
                game.opponentColor = game.switchTeam(colorChecked);

                if (game.onBottomColor == game.opponentColor)
                {
                    if (game.onBottomColor == Chess.Color.Light)
                    {
                        game.rotateBoard(true, 0);
                    }
                    else
                    {
                        game.rotateBoard(false, 0);
                    }
                }
            }
            else if (game.twoPlayer == true)
            {
                game.opponentColor = Chess.Color.Dark;

                if (game.onBottomColor != Chess.Color.Light)
                {
                    game.rotateBoard(false, 0);
                }
            }
            else //Network Game
            {
                if (game.onBottomColor == game.opponentColor)
                {
                    if (game.onBottomColor == Chess.Color.Light)
                    {
                        game.rotateBoard(true, 0);
                    }
                    else
                    {
                        game.rotateBoard(false, 0);
                    }
                }
            }
            game.ready = false;
        }

        private void portBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[0-9]");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void Mouse_Move(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(NewGameWindow);

            if (mousePosition.X > 265)
            {
                NewGameWindow.horse.FlowDirection = FlowDirection.LeftToRight;
            }
            else
            {
                NewGameWindow.horse.FlowDirection = FlowDirection.RightToLeft;
            }
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
