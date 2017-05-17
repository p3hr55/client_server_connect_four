using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class INFO
    {
        public NetworkStream CONNECTION;
        public Socket SOCKET;
        public bool CONNECTED;
        public int which;
        public string ip;
        public string name;
        public int port;
    }
}
