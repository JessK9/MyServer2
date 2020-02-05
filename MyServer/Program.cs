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
          
        }
        public void runServer()
        {
            TcpListener listener;
            Socket connection;
            NetworkStream socketStream;
            try{
                listener = new TcpListener(43);
                while(true)
                {
                    connection = listener.AcceptSocket();
                    socketStream = new NetworkStream(connection);
                    doRequest(socketStream);
                    socketStream.Close();
                    connection.Close();
                }
            }
            catch(Exception e)
            {
                log.log(e.ToString());
            }
        }
    }
}
