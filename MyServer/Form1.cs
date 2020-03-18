using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            timeoutTB.Text = Program.timeoutServer.ToString();
            timeoutTB.Show();
        }

        private void filenameTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Program.filename = filenameTB.Text;
            int i;
            if (!int.TryParse(timeoutTB.Text, out i))
            {
                MessageBox.Show("You can only enter numbers into this field, please try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                int timeoutInt = int.Parse(timeoutTB.Text);
                Program.timeoutServer = timeoutInt;
                
                Application.Exit();
            }
        }

        private void button2_Click(object sender, EventArgs e)          // debug button
        {
            Program.debug = true;
        }

        private void timeoutTB_TextChanged(object sender, EventArgs e)
        {
           
           
        }
    }
}
