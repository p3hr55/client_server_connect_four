using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Connect4
{
    public partial class Form1 : Form
    {
        private Image board = Connect4.Properties.Resources.Connect4Board;
        int[,] game = new int[7, 6];
        int[] col_heights = new int[7] {0, 0, 0, 0, 0, 0, 0};
        private delegate void kdelegate(object temp);
        int PLAYER = 1;
        string IP = null, USERNAME = null, PASSWORD = null;
        int PORT = 0;
        private Thread TalkThread = null;
        NetworkStream Connectstream = null;
        private static BinaryFormatter formatter = new BinaryFormatter();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void SendToListbox(object temp)
        {
            if (temp is PACKET.Message)
            {
                PACKET.Message m = (PACKET.Message)temp;
                textBox1.Text += m.m_message + "\r\n";
            }

            if (temp is PACKET.IPinfo)
            {
                PACKET.IPinfo w = (PACKET.IPinfo)temp;
                MessageBox.Show("derp" + w.ip);
            }
        }

        public void TalkProcedure()
        {
            //Connectstream = new NetworkStream(CONNECTION);
            object temp;
            try
            {
                while (true)
                {
                    temp = formatter.Deserialize(Connectstream);
                    //SendToListbox(temp);

                    if (textBox1.InvokeRequired)
                    {
                        BeginInvoke(new kdelegate(SendToListbox), temp);
                    }
                }
            }
            catch (Exception e) { MessageBox.Show("Lost Connection"); }

        }

        private void insert(int col, int player)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CONNECT c = new CONNECT();
            c.ShowDialog();
            MessageBox.Show(c.Address);

            IP = c.Address;
            PORT = c.Port;
            USERNAME = c.Username;
            PASSWORD = c.Password;

            TcpClient client = new TcpClient();

            try
            {
                client.Connect(IP, PORT);
                Connectstream = client.GetStream();

                if (TalkThread == null || !TalkThread.IsAlive)
                {
                    TalkThread = new Thread(new ThreadStart(TalkProcedure));
                    TalkThread.Start();
                }

                PACKET.Name n = new PACKET.Name();
                n.m_name = USERNAME;

                formatter.Serialize(Connectstream, n);
            }
            catch (SocketException error) { MessageBox.Show("Server Must be down"); }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(board, 11, 11, 640, 480);

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (game[i, j] != 0)
                    {
                        if (game[i, j] == 1)
                        {
                            Image im = Connect4.Properties.Resources.red;
                            e.Graphics.DrawImage(im, 111 + (90 * i) - 90, 412 - (80 * j), 81, 80);
                        }
                        else
                        {
                            Image im = Connect4.Properties.Resources.black;
                            e.Graphics.DrawImage(im, 111 + (90 * i) - 90, 412 - (80 * j), 81, 80);
                        }
                    }

                }
            }
        }

        private bool CHECK_WINS(int player)
        {
            for (int r = 0; r < 6; r++)
                if (((game[0, r] == player) && (game[1, r] == player) && (game[2, r] == player) && (game[3, r] == player)) || ((game[1, r] == player) && (game[2, r] == player) && (game[3, r] == player) && (game[4, r] == player))
                    || ((game[2, r] == player) && (game[3, r] == player) && (game[4, r] == player) && (game[5, r] == player)) || ((game[3, r] == player) && (game[4, r] == player) && (game[5, r] == player) && (game[6, r] == player)))
                    return true;

            for (int c = 0; c < 7; c++)
                if (((game[c, 0] == player) && (game[c, 1] == player) && (game[c, 2] == player) && (game[c, 3] == player)) || ((game[c, 1] == player) && (game[c, 2] == player) && (game[c, 3] == player) && (game[c, 4] == player))
                    || ((game[c, 2] == player) && (game[c, 3] == player) && (game[c, 4] == player) && (game[c, 5] == player)))
                    return true;

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 3; j++)
                    if (game[i, j] != 0 && game[i, j] == player && game[i, j] == game[i + 1, j + 1] && game[i, j] == game[i + 2, j + 2] && game[i, j] == game[i + 3, j + 3])
                        return true;

            for (int i = 0; i < 4; i++)
                for (int j = 3; j < 6; j++)
                    if (game[i, j] != 0 && game[i, j] == player && game[i, j] == game[i + 1, j - 1] && game[i, j] == game[i + 2, j - 2] && game[i, j] == game[i + 3, j - 3])
                        return true;

            return false;
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            //1x = 27 - 94 1y = 17 - 87
            int col = -1;

            if ((e.Location.Y <= 87 && e.Location.Y >= 17))
            {
                if (e.Location.X >= 25 && e.Location.X <= 635)
                {
                    if (e.Location.X >= 25 && e.Location.X <= 95)
                        col = 0;
                    else if (e.Location.X >= 115 && e.Location.X <= 185)
                        col = 1;
                    else if (e.Location.X >= 205 && e.Location.X <= 275)
                        col = 2;
                    else if (e.Location.X >= 295 && e.Location.X <= 365)
                        col = 3;
                    else if (e.Location.X >= 385 && e.Location.X <= 455)
                        col = 4;
                    else if (e.Location.X >= 475 && e.Location.X <= 545)
                        col = 5;
                    else if (e.Location.X >= 565 && e.Location.X <= 635)
                        col = 6;

                    game[col, col_heights[col]] = PLAYER;
                    col_heights[col]++;

                    if (PLAYER == 1)
                        PLAYER = 2;
                    else
                        PLAYER = 1;

                    pictureBox1.Invalidate();
                    pictureBox1.Update();
                    pictureBox1.Refresh();

                    pictureBox1.SendToBack();
                    if (CHECK_WINS(1))
                        MessageBox.Show("You win");
                    else if (CHECK_WINS(2))
                        MessageBox.Show("You lose");
                }
            }

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string msg = USERNAME + ": " + textBox2.Text;

            try
            {
                if (Connectstream != null && Connectstream.CanWrite)
                {
                    PACKET.Message m = new PACKET.Message();
                    m.m_name = USERNAME;
                    m.m_message = msg;

                    formatter.Serialize(Connectstream, m);  // send data
                }
                else
                    MessageBox.Show("error");
            }
            catch (System.IO.IOException error) { MessageBox.Show("error"); }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(board, 11, 11, 640, 480);


            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (game[i, j] != 0)
                    {
                        if (game[i, j] == 1)
                        {
                            Image im = Connect4.Properties.Resources.red;
                            e.Graphics.DrawImage(im, 111 + (90 * i) - 90, 412 - (80 * j), 81, 80);
                        }
                        else
                        {
                            Image im = Connect4.Properties.Resources.black;
                            e.Graphics.DrawImage(im, 111 + (90 * i) - 90, 412 - (80 * j), 81, 80);
                        }
                    }

                }
            }

        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int col = -1;

            if ((e.Location.Y <= 87 && e.Location.Y >= 17))
            {
                if (e.Location.X >= 25 && e.Location.X <= 635)
                {
                    if (e.Location.X >= 25 && e.Location.X <= 95)
                        col = 0;
                    else if (e.Location.X >= 115 && e.Location.X <= 185)
                        col = 1;
                    else if (e.Location.X >= 205 && e.Location.X <= 275)
                        col = 2;
                    else if (e.Location.X >= 295 && e.Location.X <= 365)
                        col = 3;
                    else if (e.Location.X >= 385 && e.Location.X <= 455)
                        col = 4;
                    else if (e.Location.X >= 475 && e.Location.X <= 545)
                        col = 5;
                    else if (e.Location.X >= 565 && e.Location.X <= 635)
                        col = 6;

                    game[col, col_heights[col]] = PLAYER;
                    col_heights[col]++;

                    if (PLAYER == 1)
                        PLAYER = 2;
                    else
                        PLAYER = 1;

                    pictureBox1.Invalidate();
                    pictureBox1.Update();
                    pictureBox1.Refresh();

                    if (CHECK_WINS(1))
                        MessageBox.Show("You win");
                    else if (CHECK_WINS(2))
                        MessageBox.Show("You lose");
                }
            }
        }

    }
}
