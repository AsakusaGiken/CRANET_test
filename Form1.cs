using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CRANET_test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            serialPort1.BaudRate = 115200;
            
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {

        }

        private void button_Open_Click(object sender, EventArgs e)
        {
            serialPort1.PortName = textBox1.Text;
            serialPort1.Open();
            serialPort1.DiscardInBuffer();
        }
    }
}
