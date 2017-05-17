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
    public partial class Waiting : Form
    {
        private string IP = null;
        private int PORT = 0;

        private delegate void kdelegate(object temp);
        private Thread TalkThread = null;
        NetworkStream Connectstream = null;
        private static BinaryFormatter formatter = new BinaryFormatter();
        public Waiting()
        {
            InitializeComponent();
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

        public void setup()
        {
            TcpClient client = new TcpClient();

            try
            {
                client.Connect(IP, PORT);
                Connectstream = client.GetStream();

            }
            catch (SocketException error) { MessageBox.Show("Server Must be down"); }

            if (TalkThread == null || !TalkThread.IsAlive)
            {
                TalkThread = new Thread(new ThreadStart(TalkProcedure));
                TalkThread.Start();
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

        private void button1_Click(object sender, EventArgs e)
        {
            string msg = textBox4.Text;

            try
            {
                if (Connectstream != null && Connectstream.CanWrite)
                {
                    PACKET.Message m = new PACKET.Message();
                    m.m_message = msg;

                    formatter.Serialize(Connectstream, m);  // send data
                }
                else
                    MessageBox.Show("error");
            }
            catch (System.IO.IOException error) { MessageBox.Show("error"); }
        }

        public string Address
        {
            set
            {
                IP = value;
            }
        }

        public int Port
        {
            set
            {
                PORT = value;
            }
        }

    }
}
