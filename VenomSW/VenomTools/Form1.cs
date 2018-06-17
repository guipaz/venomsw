using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VenomTools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void configurerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configurer c = new Configurer();
            c.ShowDialog();
        }
    }
}
