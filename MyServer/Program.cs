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
            runServer();
        }
        static void runServer()
        {
            TcpListener listener;
            Socket connection;
            NetworkStream socketStream;

            try
            {
                listener = new TcpListener(IPAddress.Any,43);  //deprecated means its old fashioned but still works
                listener.Start();
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
                Console.WriteLine("Exception: " + e.ToString());
            }
        }


        static void doRequest(NetworkStream socketStream)
        {

            try
            {
                StreamWriter sw = new StreamWriter(socketStream);
                StreamReader sr = new StreamReader(socketStream);


                //  sw.WriteLine(args[0]);
                //    sw.Flush(); 
                    Console.WriteLine(sr.ReadToEnd());
            }

            catch
            {
                Console.WriteLine("Something went wrong");
            }
         }       
    }
}

