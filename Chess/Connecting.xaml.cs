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
            this.MouseDown += delegate { DragMove(); };
            this.game = l;
            this.Owner = game.mWindow;
            waitForData();
        }

        private async void waitForData()
        {
            string player1;
            byte[] bArray = new byte[1];

            if (game.opponent == "dark")
            {
                player1 = "light";
            }
            else
            {
                player1 = "dark";
            }

            while (!game.client.Connected){}
            statusBlk.Text = "Connected to server\nLooking for opponent . . .";
            //wait for data to come in
            int bytes = await game.nwStream.ReadAsync(bArray, 0, 1);
            if (bytes == 0)
            {
                
            }
            //start game
            game.setBoardForNewGame();
            if (bArray[0] == 1)
            {
                game.offensiveTeam = player1;
            }
            else if (bArray[0] == 2)
            {
                game.offensiveTeam = game.opponent;
                game.ready = false;
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
