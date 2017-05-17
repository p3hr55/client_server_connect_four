using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Server
{
    public partial class Form1 : Form
    {
        private delegate void DELEGATE(object send, int id);
        private static BinaryFormatter FORMAT = new BinaryFormatter();
        private TcpListener LISTENER;
        private Socket CONNECTION;
        private Thread CONNECTION_WAIT = null;
        Thread[] THREADS;
        INFO[] CONNECTION_INFO;
        private string PASSWORD = null;
        private int KEY_INDEX = 0;
        private int PORT_NUM = -1;
        int LOCATION = 0;
        string[] GAMES = new string[4];

        [DllImport("user32.dll")]
        static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);
        [DllImport("user32.dll")]
        static extern bool ShowCaret(IntPtr hWnd);

        public Form1()
        {
            InitializeComponent();

            THREADS = new Thread[5];
            CONNECTION_INFO = new INFO[5];
        }

        public void CREATE_CARROT(Control ctrl)
        {
            CreateCaret(ctrl.Handle, IntPtr.Zero, 8, 18);
            textBox2.SelectionStart = textBox2.Text.Length;
            ShowCaret(ctrl.Handle);
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (int)Keys.Enter)
            {
                e.Handled = true;
                string commands = textBox2.Text;
                COMMAND(commands.Substring(9, KEY_INDEX));
                textBox2.Text = "COMMAND: ";
                textBox2.SelectionStart = textBox2.Text.Length;
                CREATE_CARROT(textBox2);
                KEY_INDEX = 0;
            }

            else if (e.KeyChar == (int)Keys.Tab)
            { }

            else if (e.KeyChar == (int)Keys.Back)
            {
                if (KEY_INDEX > 0)
                    KEY_INDEX--;
                else
                    e.Handled = true;
            }

            else
                KEY_INDEX++;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox2.Text = "COMMAND: ";
            textBox2.SelectionStart = textBox2.Text.Length;
            CREATE_CARROT(textBox2);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0006)
            {
                textBox2.SelectionStart = textBox2.Text.Length;
                CREATE_CARROT(textBox2);
            }
        
        }

        public int locationav()
        {
            for (int i = 0; i < 5; i++)
            {
                if (CONNECTION_INFO[i] == null)
                {
                    LOCATION = i;
                    return i;
                }
            }

            return -1;
        }

        public void WAITING_CONN()
        {
            LISTENER = new TcpListener(IPAddress.Any, PORT_NUM);
            LISTENER.Start();

            try
            {
                while (locationav() != -1)
                {
                    CONNECTION = LISTENER.AcceptSocket();

                    CONNECTION_INFO[LOCATION] = new INFO();
                    CONNECTION_INFO[LOCATION].SOCKET = CONNECTION;
                   
                    CONNECTION_INFO[LOCATION].CONNECTED = true;
                    CONNECTION_INFO[LOCATION].which = LOCATION;
                    CONNECTION_INFO[LOCATION].ip = CONNECTION.RemoteEndPoint.ToString();

                    CONNECTION_INFO[LOCATION].CONNECTION = new NetworkStream(CONNECTION);
                    object temp = FORMAT.Deserialize(CONNECTION_INFO[LOCATION].CONNECTION);
                    PACKET.IPinfo inf = (PACKET.IPinfo)temp;
                    CONNECTION_INFO[LOCATION].ip = inf.ip;

                    if (!CONNECTION_INFO[LOCATION].SOCKET.RemoteEndPoint.ToString().Contains("127.0.0.1"))
                    {
                        string[] ss = CONNECTION_INFO[LOCATION].SOCKET.RemoteEndPoint.ToString().Split(':');
                        CONNECTION_INFO[LOCATION].ip = ss[0];
                    }
                    


                    temp = FORMAT.Deserialize(CONNECTION_INFO[LOCATION].CONNECTION);
                    PACKET.Name na = (PACKET.Name)temp;
                    CONNECTION_INFO[LOCATION].name = na.m_name;

                    THREADS[LOCATION] = new Thread(new ParameterizedThreadStart(CLIENT_COMM));
                    THREADS[LOCATION].Start(CONNECTION_INFO[LOCATION]);
                    LOCATION++;

                    string gub = CONNECTION_INFO[LOCATION - 1].name + " has connected @" + CONNECTION_INFO[LOCATION - 1].ip + "\r\n";
                    if (textBox1.InvokeRequired)
                        BeginInvoke(new DELEGATE(PRINT), gub, 69);
                    else
                        textBox1.Text += CONNECTION_INFO[LOCATION - 1].name + " has connected @" + CONNECTION_INFO[LOCATION - 1].ip + "\r\n";

                    PACKET.Name n = new PACKET.Name();
                    n.m_name = CONNECTION_INFO[LOCATION - 1].name;

                    for (int x = 0; x < 5; x++)
                        if (CONNECTION_INFO[x] != null)
                        {
                            if (x == LOCATION - 1)
                            {
                                for (int j = 0; j < 5; j++)
                                {
                                    if (CONNECTION_INFO[j] != null)
                                    {
                                        PACKET.Name te = new PACKET.Name();
                                        te.m_name = CONNECTION_INFO[j].name;
                                        FORMAT.Serialize(CONNECTION_INFO[x].CONNECTION, te);
                                    }
                                }
                            }
                            else
                                FORMAT.Serialize(CONNECTION_INFO[x].CONNECTION, n);
                        }
                }
            }

            catch (SocketException e) { }
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
                        temp = FORMAT.Deserialize(id.CONNECTION);

                        if (temp is PACKET.Request)
                        {
                            PACKET.Request r = (PACKET.Request)temp;

                            if (textBox1.InvokeRequired)
                                BeginInvoke(new DELEGATE(PRINT), temp, id.which + 1);

                            else
                                textBox1.Text += "Client " + r.requster + " requested a game with " + r.destination + "\r\n";

                            for (int i = 0; i < 5; i++)
                            {
                                if (CONNECTION_INFO[i] != null && CONNECTION_INFO[i].name == r.destination)
                                    FORMAT.Serialize(CONNECTION_INFO[i].CONNECTION, r);
                            }
                        }

                        else if (temp is PACKET.AcceptRequest)
                        {
                            PACKET.AcceptRequest r = (PACKET.AcceptRequest)temp;

                            if (textBox1.InvokeRequired)
                                BeginInvoke(new DELEGATE(PRINT), temp, id.which + 1);

                            else
                                textBox1.Text += id.name + " accepted game with " + r.requester + "\r\n";

                            PACKET.IPinfo i = new PACKET.IPinfo();
                            string inf = id.SOCKET.RemoteEndPoint.ToString();

                            for (int x = 0; x < 5; x++)
                            {
                                if (CONNECTION_INFO[x] != null && CONNECTION_INFO[x].name == r.requester)
                                    i.ip = CONNECTION_INFO[x].ip;
                            }

                            string[] sp = inf.Split(':');
                            i.port = Convert.ToInt32(sp[1]);
                            r.port = i.port;

                            FORMAT.Serialize(id.CONNECTION, i);

                            for (int w = 0; w < 5; w++)
                            {
                                if (CONNECTION_INFO[w] != null && CONNECTION_INFO[w].name == r.requester)
                                    FORMAT.Serialize(CONNECTION_INFO[w].CONNECTION, r);
                            }
                        }

                        else if (temp is PACKET.Terminate)
                        {
                            PACKET.Terminate b = (PACKET.Terminate)temp;
                            PACKET.Terminate terminator = new PACKET.Terminate();
                            terminator.requester = b.requester;

                            for (int i = 0; i < 5; i++)
                            {
                                if (CONNECTION_INFO[i] != null && CONNECTION_INFO[i].name == b.requester)
                                {
                                    CONNECTION_INFO[i].CONNECTION.Close();
                                    CONNECTION_INFO[i].CONNECTED = false;
                                    CONNECTION_INFO[i].SOCKET.Close();
                                    CONNECTION_INFO[i] = null;
                                }
                            }
                        }

                        else
                        {
                            if (textBox1.InvokeRequired)
                                BeginInvoke(new DELEGATE(PRINT), temp, id.which + 1);

                            for (int x = 0; x < 5; x++)
                                if (CONNECTION_INFO[x] != null)
                                    FORMAT.Serialize(CONNECTION_INFO[x].CONNECTION, temp); 
                        }
                    }
                }
            }
            catch (System.Runtime.Serialization.SerializationException e)
            {
                object t = id.name + " disconnected from server\r\n";
                PACKET.Terminate terminator = new PACKET.Terminate();
                terminator.requester = id.name;

                if (textBox1.InvokeRequired)
                   BeginInvoke(new DELEGATE(PRINT), t, id.which + 1);

                for (int i = 0; i < 5; i++)
                {
                    if (CONNECTION_INFO[i] != null && CONNECTION_INFO[i].name == id.name)
                    {
                        CONNECTION_INFO[i].CONNECTION.Close();
                        CONNECTION_INFO[i].SOCKET.Close();
                        CONNECTION_INFO[i] = null;
                    }
                    else if (CONNECTION_INFO[i] != null && CONNECTION_INFO[i].name != id.name)
                        FORMAT.Serialize(CONNECTION_INFO[i].CONNECTION, terminator);
                }

            }
            catch (System.IO.IOException e)
            {
            }
        }

        private void PRINT(object send, int id)
        {
            if (send is PACKET.Message)
            {
                PACKET.Message m = (PACKET.Message)send;
                textBox1.Text += "Message Recieved from Client #" + id.ToString() + ": " + m.m_message + "\r\n";
            }

            else if (send is PACKET.Request)
            {
                PACKET.Request r = (PACKET.Request)send;
                textBox1.Text += "Client " + r.requster + " requested a game with " + r.destination + "\r\n";
            }

            else if (send is PACKET.AcceptRequest)
            {
                PACKET.AcceptRequest r = (PACKET.AcceptRequest)send;
                textBox1.Text += id + " accepted game with " + r.requester + "\r\n";
            }

            else if (send is string)
                textBox1.Text += send;
        }

        private void COMMAND(string s)
        {
            string[] SUBCOMMANDS = null;

            if (s.Contains(' '))
                SUBCOMMANDS = s.Split(' ');

            if (SUBCOMMANDS != null && SUBCOMMANDS[0].ToLower() == "opacity")
            {
                if (Convert.ToInt32(SUBCOMMANDS[1]) <= 100 && Convert.ToInt32(SUBCOMMANDS[1]) > 1)
                {
                    this.Opacity = Convert.ToDouble(SUBCOMMANDS[1]) / 100;
                    textBox1.Text += "COMMAND ENTERED: Opacity Set to " + SUBCOMMANDS[1] + "%\r\n";
                }
            }

            else if (SUBCOMMANDS != null && SUBCOMMANDS[0].ToUpper() == "PORT")
            {
                PORT_NUM = Convert.ToInt32(SUBCOMMANDS[1]);
                textBox1.Text += "COMMAND ENTERED: Port has been set to " + PORT_NUM + "\r\n";
            }

            else if (SUBCOMMANDS != null && SUBCOMMANDS[0].ToUpper() == "PASSWORD")
            {
                PASSWORD = SUBCOMMANDS[1];
                textBox1.Text += "COMMAND ENTERED: Server password has been set to " + PASSWORD + "\r\n";
            }

            else if (s.ToUpper() == "START")
            {
                if (CONNECTION_WAIT == null || !CONNECTION_WAIT.IsAlive)
                {
                    CONNECTION_WAIT = new Thread(new ThreadStart(WAITING_CONN));
                    CONNECTION_WAIT.Start();
                    textBox1.Text += "COMMAND ENTERED: Server has been started on PORT #" + PORT_NUM + "\r\n";
                }
            }

            else if (s.ToUpper() == "STOP")
            {
                if (CONNECTION_WAIT != null && CONNECTION_WAIT.IsAlive)
                {
                    LISTENER.Stop();
                    LOCATION = 0;
                }

                for (int x = 0; x < 5; x++)
                {
                    if (CONNECTION_INFO[x] != null && THREADS[x] != null && THREADS[x].IsAlive)
                    {
                        CONNECTION_INFO[x].SOCKET.Close();
                        THREADS[x].Join();
                        THREADS[x] = null;
                        THREADS[x] = null;
                    }
                }

                textBox1.Text += "COMMAND ENTERED: Server is being stopped.\r\n";
            }
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            textBox2.Focus();
            CREATE_CARROT(textBox2);
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            textBox2.Focus();
            CREATE_CARROT(textBox2);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CONNECTION != null && CONNECTION_WAIT != null)
            {
                LISTENER.Stop();
                CONNECTION.Close();
                CONNECTION_WAIT.Abort();

                for (int i = 0; i < 5; i++)
                {
                    if (THREADS[i] != null && THREADS[i].IsAlive)
                        THREADS[i].Abort();

                }
            }  
        }
    }
}
