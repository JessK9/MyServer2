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
                    handle(socketStream);
                    socketStream.Close();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
            }
        }
        
        static void handle (NetworkStream socketStream)
        {
            string line;
            string name;
            string location;

            try
            {
                StreamWriter sw = new StreamWriter(socketStream);
                StreamReader sr = new StreamReader(socketStream);
                try
                {
                    line = sr.ReadLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }
               
                string[] split = line.Split(' ');
               
                if (line.Contains("HTTP/1.0"))
                {
                    if (split[0] == "GET")
                    {
                        name = split[4];
                        if (dictionary.ContainsKey(name))
                        {
                            sw.WriteLine("HTTP/1.0 200 OK");
                            sw.WriteLine("Content-Type: text/plain");
                            sw.WriteLine();
                            sw.WriteLine(dictionary[name]);  
                            sw.Flush();
                        }
                        else if(!dictionary.ContainsKey(name))
                        {
                            sw.WriteLine("HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                            sw.Flush();
                        }
                    }
                    else if (split[0] == "POST")
                    {
                        name = split[3];             // unlike the GET, there is no ? before the name
                        do
                        {
                            line = sr.ReadLine();
                        } while (line.Length != 0);

                        location = sr.ReadToEnd();
                        dictionary[name] = location;

                        sw.WriteLine("HTTP/1.0 200 OK");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
                        sw.Flush();
                    }

                }
                else if(line.Contains("HTTP/1.1"))
                {
                    if (split[0] == "GET")
                    {
                        name = split[6];   // ideally, would not have to use split but would be pre assigned
                        if (dictionary.ContainsKey(name))
                        {
                            sw.WriteLine("HTTP/1.1 200 OK");
                            sw.WriteLine("Content-Type: text/plain");
                            sw.WriteLine();
                            sw.WriteLine(dictionary[name]);
                            sw.Flush();
                        }
                        else
                        {
                            sw.WriteLine("HTTP/1.1 404 Not Found");
                            sw.WriteLine("Content-Type: text/plain");
                            sw.WriteLine();
                            sw.Flush();
                        }
                    }
                    else if (split[0] == "POST")
                    {

                        do
                        {
                            line = sr.ReadLine();
                        } while (line.Length != 0);

                        name = split[2];
                        location = split.Last();
                        dictionary[name] = location;

                        sw.WriteLine("HTTP/1.1 200 OK");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
                        sw.Flush();
                    }
                }

                else        // Got to be either whois or 0.9 
                {
                    if (split[0] == "GET")
                    {
                        name = split[1];
                       
                        if (dictionary.ContainsKey(name))
                        {
                            sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\nis being tested");
                            
                            sw.Flush();
                        }
                        else if(!dictionary.ContainsKey(name))
                        {
                            sw.WriteLine("HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                            sw.Flush();
                        }
                    }
                    else if (split[0] == "PUT")
                    {
                        name = split[1];
                        do
                        {
                            line = sr.ReadLine();
                        } while (line.Length != 0);

                        location = sr.ReadToEnd();
                        dictionary[name] = location;

                        sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                        sw.Flush();
                    }

                   else if (split.Length == 1)                   // get the person out of the dictionary as there is no location
                    {
                        name = split[0];
                        if (dictionary.ContainsKey(name))
                        {
                            sw.WriteLine(dictionary[name]);            
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
                        name = split[0];
                        location = string.Join(" ", split.Skip(1).ToArray());
                        dictionary[name] = location;

                        sw.WriteLine("OK");
                        sw.Flush();

                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
        }
    }
}

