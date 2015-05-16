using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    public partial class Thinking : Window
    {
        public Thinking()
        {
            InitializeComponent();
            this.MouseDown += delegate { DragMove(); };
        }

        public void update(double percent)
        {
            thinkBar.Value = percent;

            if(percent == 100)
            {
                this.Close();
            }
        }
    }
}
