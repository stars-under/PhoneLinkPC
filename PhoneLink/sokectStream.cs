using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PhoneLink
{
    internal class SocketStream
    {
        private Socket socket;

        ~SocketStream()
        {
            socket.Close();
        }
        public SocketStream(string host,int port,int key,string direction)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress address = IPAddress.Parse(host);
            IPEndPoint endPoint = new IPEndPoint(address, port);
            socket.Connect(endPoint);
            SendKey(key, direction);
        }
        public string? SendKey(int key,string direction)
        {
            byte[] dataKey = BitConverter.GetBytes(key);
            byte[] dataCRC = BitConverter.GetBytes(~key);
            socket.Send(dataKey);
            socket.Send(dataCRC);
            if (direction == "IN")
            {
                socket.Send(BitConverter.GetBytes(0));
            }
            else
            {
                socket.Send(BitConverter.GetBytes(1));
            }
            return this.ReadString();
        }
        public void SendString(string str)
        {
            str += '\0';
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            this.Send(bytes,bytes.Length);
        }
        public void Send(byte[] data,int len)
        {
            byte[] dataLen = BitConverter.GetBytes(len);
            byte[] dataCRC = BitConverter.GetBytes(~len);
            socket.Send(dataLen);
            socket.Send(dataCRC);
            socket.Send(data,0,len, SocketFlags.None);
        }
        public byte[]? Read(ref int len)
        {
            byte[] dataLen = new byte[4];
            byte[] dataCRC = new byte[4];
            socket.Receive(dataLen, 0, dataLen.Length, SocketFlags.None);
            socket.Receive(dataCRC, 0, dataCRC.Length, SocketFlags.None);
            if(BitConverter.ToInt32(dataLen) != ~BitConverter.ToInt32(dataCRC))
            {
                return null;
            }
            len = BitConverter.ToInt32(dataLen);
            byte[] data = new byte[len];
            socket.Receive(data);
            return data;
        }
        public string? ReadString()
        {
            int len = 0;
            byte[]? vs = Read(ref len);

            if (vs != null)
                return Encoding.UTF8.GetString(vs, 0, len - 1);
            return null;
        }
    }
}
