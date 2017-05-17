using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PACKET
{
    [Serializable]
    public class Packet
    {
    }

    [Serializable]
    public class Message : Packet
    {
        public string m_message;
        public string m_name;
    }

    [Serializable]
    public class Name : Packet
    {
        public string m_name;
    }

    [Serializable]
    public class Move : Packet
    {
        public int row;
    }

    [Serializable]
    public class IPinfo : Packet
    {
        public string ip;
        public int port;
    }

    [Serializable]
    public class Request : Packet
    {
        public string requster;
        public string destination;
        public string oip;
    }

    [Serializable]
    public class AcceptRequest : Packet
    {
        public string requester;
        public string nombre;
        public int port;
    }

    [Serializable]
    public class Terminate : Packet
    {
        public string requester;
    }
}