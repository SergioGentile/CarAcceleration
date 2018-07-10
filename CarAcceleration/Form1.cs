using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Newtonsoft.Json;

namespace CarAcceleration
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Car car = new Car(label1, label2, label3);
        }
        


        private void label1_Click(object sender, EventArgs e)
        {

        }
    
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(1);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
