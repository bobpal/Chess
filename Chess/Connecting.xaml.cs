using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace Chess
{
    public partial class Connecting : Window
    {
        private Logic game;

        public Connecting(Logic l)
        {
            InitializeComponent();
            this.game = l;
            string player1;

            while(!game.client.Connected)
            {

            }
            statusBlk.Text = "Connected to server\nLooking for opponent . . .";
            //wait for data to come in
            game.bytesRead = game.nwStream.Read(game.buffer, 0, 1);

            //connection failed
            if (game.bytesRead == 0)
            {
                game.nwStream.Close();
                game.client.Close();
                this.Close();
            }
            //start game
            statusBlk.Text = "Connected to server\nFound opponent";

            if(game.opponent == "dark")
            {
                player1 = "light";
            }
            else
            {
                player1 = "dark";
            }

            if(game.buffer[0] == 1)
            {
                game.offensiveTeam = player1;
            }
            else if (game.buffer[0] == 2)
            {
                game.offensiveTeam = game.opponent;
            }
            this.Close();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            game.nwStream.Close();
            game.client.Close();
            this.Close();
        }
    }
}
