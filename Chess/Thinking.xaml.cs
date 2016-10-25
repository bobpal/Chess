using System;
using System.Threading.Tasks;
using System.Windows;

namespace Chess
{
    public partial class Thinking : Window
    {
        private Logic game;
        private Progress<int> progress;

        public Thinking(Logic l, int r)
        {
            InitializeComponent();
            this.MouseDown += delegate { DragMove(); };
            writeMessage(r);
            this.game = l;
            progress = new Progress<int>();
            progress.ProgressChanged += (sender, e) => { update(e); };
        }

        private void writeMessage(int i)
        {
            string[] message = new string[41];

            message[0] = "obliterate";
            message[1] = "crush";
            message[2] = "destroy";
            message[3] = "conquer";
            message[4] = "PWN";
            message[5] = "massacre";
            message[6] = "thrash";
            message[7] = "exterminate";
            message[8] = "slaughter";
            message[9] = "vanquish";
            message[10] = "foil";
            message[11] = "thwart";
            message[12] = "overthrow";
            message[13] = "annihilate";
            message[14] = "decimate";
            message[15] = "pulverize";
            message[16] = "whip";
            message[17] = "clobber";
            message[18] = "defeat";
            message[19] = "eliminate";
            message[20] = "eradicate";
            message[21] = "waste";
            message[22] = "end";
            message[23] = "ruin";
            message[24] = "neutralize";
            message[25] = "curb stomp";
            message[26] = "dominate";
            message[27] = "school";
            message[28] = "outclass";
            message[29] = "humiliate";
            message[30] = "disgrace";
            message[31] = "break";
            message[32] = "ashame";
            message[33] = "silence";
            message[34] = "wreak";
            message[35] = "surpass";
            message[36] = "cream";
            message[37] = "wipe the floor with";
            message[38] = "dispense";
            message[39] = "outdo";
            message[40] = "trounce";

            taunt.Content = "Determining the best way to " + message[i] + " you...";
        }

        public void update(double percent)
        {
            thinkBar.Value = percent;

            if(percent == 100)
            {
                this.Close();
            }
        }

        private async void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            await Task.Run(() => game.minimax(game.pieceArray, game.offensiveTeam, -1, true, -30, 30, progress));
        }
    }
}
