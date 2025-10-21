using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Monopoly
{
    public partial class SingleConfig : Form
    {
        public SingleConfig()
        {
            InitializeComponent();
            pictureBox1.Image = Properties.Resources.plr1;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void SingleConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            Forms.Dispose();
        }
    }
}
