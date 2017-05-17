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

namespace Client
{
    public partial class Game : Form
    {
        private Image board = Client.Properties.Resources.Connect4Board;
        private Thread TalkThread = null;
        private NetworkStream Connectstream = null;
        private static BinaryFormatter formatter = new BinaryFormatter();
        private delegate void DELEGATE(object send, int id);
        private delegate void DEL2(object temp);
        private TcpListener LISTENER;
        private Socket CONNECTION;
        private Thread CONNECTION_WAIT = null;
        private Thread[] THREADS;
        private INFO[] CONNECTION_INFO;
        private int[,] game = new int[7, 6];
        private int[] col_heights = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
        private int PLAYER = 1;
        private int PORT = 0;
        private int LOCATION = 0;
        private string OPONENT = null;
        private string IP = null, USERNAME = null;
        private bool isHost = false;
        private bool move = false;

        public Game()
        {
            InitializeComponent();
        }

        public void TalkProcedure()
        {
            object temp;
            try
            {
                while (true)
                {
                    temp = formatter.Deserialize(Connectstream);

                    if (textBox1.InvokeRequired)
                        BeginInvoke(new DEL2(SendToListbox), temp);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Lost Connection");
            }

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (!isHost)
            {
                string msg = textBox2.Text;

                try
                {
                    if (Connectstream != null && Connectstream.CanWrite)
                    {
                        PACKET.Message m = new PACKET.Message();
                        m.m_name = USERNAME;
                        m.m_message = msg;

                        formatter.Serialize(Connectstream, m);  
                    }
                    else
                    { MessageBox.Show("error"); }
                }
                catch (System.IO.IOException error)
                { 
                    MessageBox.Show("error");
                }
            }

            else 
            {
                    PACKET.Message mes = new PACKET.Message();
                    mes.m_message = "HOST" + ": " + textBox2.Text;

                if (textBox1.InvokeRequired)
                    BeginInvoke(new DELEGATE(PRINT), mes.m_message, 5);
                else
                    textBox1.Text += mes.m_message + "\r\n";

                    for (int x = 0; x < 5; x++)
                        if (CONNECTION_INFO[x] != null)
                            formatter.Serialize(CONNECTION_INFO[x].CONNECTION, mes);
                }

            textBox2.Text = "";
        }

        private void SendToListbox(object temp)
        {
            if (temp is PACKET.Message)
            {
                PACKET.Message m = (PACKET.Message)temp;
                textBox1.Text += m.m_message + "\r\n";
            }

            if (temp is PACKET.Move)
            {
                move = true;
                PACKET.Move m = (PACKET.Move)temp;
                game[m.row, col_heights[m.row]] = 2;
                col_heights[m.row]++;

                pictureBox1.Invalidate();
                pictureBox1.Update();
                pictureBox1.Refresh();
            }
        }

        public void nonhost()
        {
            try
            { 
            TcpClient client = new TcpClient();
                PLAYER = 1;
                client.Connect(IP, PORT);
                Connectstream = client.GetStream();
                PACKET.Name r = new PACKET.Name();
                r.m_name = "Client";

                if (TalkThread == null || !TalkThread.IsAlive)
                {
                    TalkThread = new Thread(new ThreadStart(TalkProcedure));
                    TalkThread.Start();
                }
            }

            catch (SocketException error) { MessageBox.Show("Server Must be down"); }
        }

        public void host()
        {
            if (TalkThread == null || !TalkThread.IsAlive)
            {
                PLAYER = 2;
                isHost = true;
                move = true;
                THREADS = new Thread[5];
                CONNECTION_INFO = new INFO[5];
                CONNECTION_WAIT = new Thread(new ThreadStart(WAITING_CONN));
                CONNECTION_WAIT.Start();
            }
        }

        public void WAITING_CONN()
        {
            LISTENER = new TcpListener(PORT);
            LISTENER.Start();

            CONNECTION = LISTENER.AcceptSocket();
            CONNECTION_INFO[LOCATION] = new INFO();
            CONNECTION_INFO[LOCATION].SOCKET = CONNECTION;

            CONNECTION_INFO[LOCATION].CONNECTED = true;
            CONNECTION_INFO[LOCATION].which = LOCATION;

            CONNECTION_INFO[LOCATION].CONNECTION = new NetworkStream(CONNECTION);

            THREADS[LOCATION] = new Thread(new ParameterizedThreadStart(CLIENT_COMM));
            THREADS[LOCATION].Start(CONNECTION_INFO[LOCATION]);
        }

        public void CLIENT_COMM(object obj)
        {
            INFO id = null;
            object temp;

            if (obj is INFO)
                id = (INFO)obj;

            try
            {
                while (true)
                {
                    if (id != null)
                    {

                        temp = formatter.Deserialize(id.CONNECTION);

                        if (temp is PACKET.Message)
                        {
                            PACKET.Message mes = new PACKET.Message();
                            PACKET.Message mes2 = (PACKET.Message)temp;
                            mes.m_message = OPONENT + ": " + mes2.m_message;
                            if (textBox1.InvokeRequired)
                                BeginInvoke(new DELEGATE(PRINT), temp, id.which + 1);

                            for (int x = 0; x < 5; x++)
                                if (CONNECTION_INFO[x] != null)
                                    formatter.Serialize(CONNECTION_INFO[x].CONNECTION, mes); 
                        }

                        else if (temp is PACKET.Move)
                        {
                            PACKET.Move m = (PACKET.Move)temp;
                            game[m.row, col_heights[m.row]] = 1;
                            col_heights[m.row]++;

                            if (pictureBox1.InvokeRequired)
                                BeginInvoke(new DEL2(revPB), pictureBox1);

                            else
                            {
                                pictureBox1.Invalidate();
                                pictureBox1.Update();
                                pictureBox1.Refresh();
                            }

                            if (CHECK_WINS(1) == true || CHECK_WINS(2) == true)
                                MessageBox.Show("You lose");

                            move = true;
                        }
                    }
                }
            }
            catch (System.IO.IOException e)
            { 
                MessageBox.Show("Client disconected.");
            }
        }

        private void PRINT(object send, int id)
        {
            if (send is string)
                textBox1.Text += send;

            if (send is PACKET.Message)
            {
                PACKET.Message m = (PACKET.Message)send;
                textBox1.Text += OPONENT + ": " + m.m_message + "\r\n";
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
                            Image im = Client.Properties.Resources.red;
                            e.Graphics.DrawImage(im, 111 + (90 * i) - 90, 412 - (80 * j), 81, 80);
                        }
                        else
                        {
                            Image im = Client.Properties.Resources.black;
                            e.Graphics.DrawImage(im, 111 + (90 * i) - 90, 412 - (80 * j), 81, 80);
                        }
                    }

                }
            }

        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int col = -1;

            if (move)
            {
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


                        if (pictureBox1.InvokeRequired)
                            BeginInvoke(new DEL2(revPB), pictureBox1);

                        else
                        {
                            pictureBox1.Invalidate();
                            pictureBox1.Update();
                            pictureBox1.Refresh();
                        }

                        if (OPONENT == "CLIENT")
                        {
                            PACKET.Move m = new PACKET.Move();
                            m.row = col;

                            for (int x = 0; x < 5; x++)
                                if (CONNECTION_INFO[x] != null)
                                    formatter.Serialize(CONNECTION_INFO[x].CONNECTION, m); 

                        }

                        else
                        {
                            try
                            {
                                if (Connectstream != null && Connectstream.CanWrite)
                                {
                                    PACKET.Move m = new PACKET.Move();
                                    m.row = col;

                                    formatter.Serialize(Connectstream, m);  
                                }
                            }
                            catch (System.IO.IOException error)
                            { 
                                MessageBox.Show("error"); 
                            }
                        }

                        if (CHECK_WINS(1) == true || CHECK_WINS(2) == true)
                            MessageBox.Show("YOU WIN");

                        move = false;
                    }
                }
            }
        }

        public int Port
        {
            set
            {
                PORT = value + 20;
            }
        }

        public string Address
        {
            set
            {
                IP = value;
            }
        }

        public string Oponent
        {
            set
            {
                OPONENT = value;
            }
        }

        public void revPB(object temp)
        {
            if (temp is PictureBox)
            {
                PictureBox p = (PictureBox)temp;
                p.Invalidate();
                p.Update();
                p.Refresh();
            }
        }
    }
}
