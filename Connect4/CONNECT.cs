using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Connect4
{
    public partial class CONNECT : Form
    {
        private int T1 = 0, T2 = 0, T3 = 0, T4 = 0;
        private int PORT = 0;
        private TextBox uname, pass;
        private string IP = null, u, p;

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (int)Keys.Back && T3 != 0)
                T3--;

            else if (e.KeyChar == (int)Keys.Tab)
            { }

            if (e.KeyChar >= 48 && e.KeyChar <= 57)
                T3++;
            else
                e.Handled = true;

            if (T3 == 3)
                textBox4.Focus();
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (int)Keys.Back && T4 != 0)
                T4--;

            else if (e.KeyChar == (int)Keys.Tab)
            { }

            if (e.KeyChar >= 48 && e.KeyChar <= 57)
                T4++;
            else
                e.Handled = true;

            if (T4 == 3)
                textBox5.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int a, b, c, d;

            label6.Text = "";
            a = Convert.ToInt32(textBox1.Text);
            b = Convert.ToInt32(textBox3.Text);
            c = Convert.ToInt32(textBox2.Text);
            d = Convert.ToInt32(textBox4.Text);

            if (a >= 256)
            {
                label6.Text = "IP Subsection 1 is out of range.";
                textBox1.Text = "";
                T1 = 0;
            }

            else if (b >= 256)
            {
                label6.Text = "IP Subsection 2 is out of range.";
                textBox3.Text = "";
                T2 = 0;
            }

            else if (c >= 256)
            {
                label6.Text = "IP Subsection 3 is out of range.";
                textBox2.Text = "";
                T3 = 0;
            }

            else if (d >= 256)
            {
                label6.Text = "IP Subsection 4 is out of range.";
                textBox4.Text = "";
                T4 = 0;
            }

            else
                IP = a + "." + b + "." + c + "." + d;

            try
            {
                PORT = Convert.ToInt32(textBox5.Text);
                if (PORT <= 0 || PORT > 65535)
                    throw new Exception("invalid");
            }
            catch (Exception nn)
            {
                if (label6.Text == "")
                {
                    label6.Text = "Port is invalid.";
                    textBox5.Text = "";
                }
            }

            if (IP != null && PORT > 0)
            {
                Button newb = new Button();
                newb.Text = "Finish";
                newb.Location = button1.Location;
                newb.Click += new System.EventHandler(this.newb_click);

                Controls.Remove(button1);
                Controls.Add(newb);

                Controls.Remove(textBox1);
                Controls.Remove(textBox2);
                Controls.Remove(textBox3);
                Controls.Remove(textBox4);
                Controls.Remove(textBox5);
                Controls.Remove(label1);
                Controls.Remove(label2);
                Controls.Remove(label3);
                Controls.Remove(label4);
                Controls.Remove(label5);

                Label user = new Label();
                user.Text = "Username: ";
                user.Location = new Point(15, 15);
                Controls.Add(user);

                uname = new TextBox();
                uname.Location = new Point(20, 40);
                Controls.Add(uname);

                Label password = new Label();
                password.Text = "Password: ";
                password.Location = new Point(160, 15);
                Controls.Add(password);

                pass = new TextBox();
                pass.Location = new Point(165, 40);
                pass.PasswordChar = '•';
                Controls.Add(pass);
            }

        }

        private void newb_click(object sender, EventArgs e)
        {
            if (uname.Text != "" && pass.Text != "")
            {
                u = uname.Text;
                p = pass.Text;
                this.Close();
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (int)Keys.Back && T2 != 0)
                T2--;

            else if (e.KeyChar == (int)Keys.Tab)
            { }

            if (e.KeyChar >= 48 && e.KeyChar <= 57)
                T2++;
            else
                e.Handled = true;

            if (T2 == 3)
                textBox2.Focus();
        }

        public CONNECT()
        {
            InitializeComponent();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (int)Keys.Back && T1 != 0)
                T1--;

            else if (e.KeyChar == (int)Keys.Tab)
            { }

            if (e.KeyChar >= 48 && e.KeyChar <= 57)
                T1++;
            else
                e.Handled = true;

            if (T1 == 3)
                textBox3.Focus();
        }

        public int Port
        {
            get
            {
                return PORT;
            }
        }

        public string Username
        {
            get
            {
                return u;
            }
        }

        public string Password
        {
            get
            {
                return p;
            }
        }

        public string Address
        {
            get
            {
                return IP;
            }
        }
    }
}
