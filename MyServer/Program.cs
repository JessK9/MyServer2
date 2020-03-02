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
        static Dictionary<string, string> dictionary = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            dictionary.Add("jess", "fenner");
            runServer();

        }
        static void runServer()
        {
            TcpListener listener;
            Socket connection;
            NetworkStream socketStream;

            try
            {
                listener = new TcpListener(IPAddress.Any, 43);
                listener.Start();
                while (true)
                {
                    connection = listener.AcceptSocket();
                    socketStream = new NetworkStream(connection);
                    socketStream.ReadTimeout = 500;
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
                string line;
                try
                {
                    line = sr.ReadLine();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }
                string[] split = line.Split(' ');
                if (split.Length == 1)                   // get the person out of the dictionary as there is no location
                {
                    string person = split[0];
                    if (dictionary.ContainsKey(person))
                    {
                        sw.WriteLine(dictionary[person]);
                        sw.Flush();
                    }
                    else
                    {
                        sw.WriteLine("ERROR: no entries found");
                        sw.Flush();
                    }
                }

                else
                {
                    string person = split[0];
                    string location = string.Join(" ", split.Skip(1).ToArray());
                    dictionary[person] = location;
                    
                    sw.WriteLine("OK");
                    sw.Flush();
                   
                }
                string protocol = "whois";

                switch (protocol)
                {
                    case "whois": //whois protocol
                        {
                            
                        }
                    case "POST": //HTTP 1.1
                        {

                        }
                    case "PUT": // HTTP 0.9
                        {

                        }
                    case "GET":  // HTTP 0.9
                }
                

            }

            catch
            {
                Console.WriteLine("Something went wrong");
            }
        }
    }
}

