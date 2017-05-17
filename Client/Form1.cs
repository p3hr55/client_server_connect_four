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
    public partial class Form1 : Form
    {
        private static BinaryFormatter formatter = new BinaryFormatter();
        private NetworkStream Connectstream = null;
        private delegate void DELEGATE(object temp);
        private Thread TalkThread = null;
        private Thread GameThread = null;
        private int tport = 0;
        private string tip = null;
        private string IP = null, USERNAME = null;
        private int PORT = 0;

        public Form1()
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
                        BeginInvoke(new DELEGATE(SendToListbox), temp);

                    else
                        SendToListbox(temp);
                }
            }
            catch (Exception e)
            {
                
                textBox1.Text += e.Message;
            }

        }
        private void button2_Click(object sender, EventArgs e)
        {
            IP = textBox5.Text;
            PORT = Convert.ToInt32(textBox6.Text);
            USERNAME = textBox7.Text;

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

                PACKET.IPinfo u = new PACKET.IPinfo();
                IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
                u.ip = IPHost.AddressList[0].ToString();
                formatter.Serialize(Connectstream, u);
                PACKET.Name n = new PACKET.Name();
                n.m_name = USERNAME;

                formatter.Serialize(Connectstream, n);

                textBox2.Enabled = button1.Enabled = true;
                textBox5.Enabled = textBox6.Enabled = textBox7.Enabled = button2.Enabled = false;
                textBox2.Focus();
            }
            catch (SocketException error)
            {
                MessageBox.Show("Server is down."); 
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string msg = USERNAME + ": " + textBox2.Text;
            textBox2.Text = "";
            try
            {
                if (Connectstream != null && Connectstream.CanWrite)
                {
                    PACKET.Message m = new PACKET.Message();
                    m.m_name = USERNAME;
                    m.m_message = msg;

                    formatter.Serialize(Connectstream, m); 
                }

            }
            catch (System.IO.IOException error)
            {
                MessageBox.Show("error"); 
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                // do nothing
            }
            else if (listBox1.SelectedItem != null && listBox1.SelectedItem.ToString() != USERNAME)
            {
                PACKET.Request m = new PACKET.Request();
                m.destination = listBox1.SelectedItem.ToString();
                m.requster = USERNAME;
                formatter.Serialize(Connectstream, m);
            }
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem == null)
            {
                // do nothing
            }

            else if (listBox2.SelectedItem != null && listBox2.SelectedItem.ToString() != USERNAME)
            {
                PACKET.AcceptRequest m = new PACKET.AcceptRequest();
                m.requester = listBox2.SelectedItem.ToString();
                m.nombre = USERNAME;
                formatter.Serialize(Connectstream, m);
            }
        }

        private void SendToListbox(object temp)
        {
            if (temp is PACKET.Message)
            {
                PACKET.Message m = (PACKET.Message)temp;
                textBox1.Text += m.m_message + "\r\n";
            }

            else if (temp is PACKET.Name)
            {
                PACKET.Name w = (PACKET.Name)temp;
                listBox1.Items.Add(w.m_name);
            }

            else if (temp is PACKET.Request)
            {
                PACKET.Request r = (PACKET.Request)temp;
                listBox2.Items.Add(r.requster);
            }

            else if (temp is PACKET.AcceptRequest)
            {
                PACKET.AcceptRequest a = (PACKET.AcceptRequest)temp;
                if (GameThread == null || !GameThread.IsAlive)
                {
                    tport = a.port;
                    GameThread = new Thread(new ThreadStart(GameHost));
                    GameThread.Start();
                }
            }

            else if (temp is PACKET.IPinfo)
            {
                if (GameThread == null || !GameThread.IsAlive)
                {
                    PACKET.IPinfo i = (PACKET.IPinfo)temp;
                    tport = i.port;
                    tip = i.ip;
                    GameThread = new Thread(new ThreadStart(GameP));
                    GameThread.Start();
                }
            }

            else if (temp is PACKET.Terminate)
            {
                PACKET.Terminate terminator = (PACKET.Terminate)temp;

                foreach (var item in listBox1.Items.Cast<string>().ToList())
                {
                    if (item.Contains(terminator.requester))
                        listBox1.Items.Remove(item);
                }
            }
        }

        public void GameHost()
        {
            Game g = new Client.Game();
            g.Port = tport;
            g.Oponent = "CLIENT";
            g.host();
            g.ShowDialog();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (TalkThread != null)
            {
                TalkThread.Abort();

                if (GameThread != null)
                    GameThread.Abort();

                Connectstream.Close();
            }
            }

        public void GameP()
        {
            Game g = new Client.Game();
            g.Port = tport;
            g.Address = tip;
            g.Oponent = "HOST";
            g.nonhost();
            g.ShowDialog();
        }
    }
}
