using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace MyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener listener;
            Socket connection;
            NetworkStream socketStream;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1"); //Got from micrsoft site
            try
            {
                listener = new TcpListener(localAddr, 43);
                while (true)
                {
                    connection = listener.AcceptSocket();
                    socketStream = new NetworkStream(connection);
                    doRequest(socketStream);
                    socketStream.Close();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                log.log(e.ToString());
            }
        }
        
    }
}
