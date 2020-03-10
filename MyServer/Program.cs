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
               
                if (line.EndsWith("HTTP/1.0"))
                {
                    if (split[0] == "GET" || (split[0] == "GET" && split[1].StartsWith("/")) )
                    {
                        name = split[1].Substring(2);
                        if (dictionary.ContainsKey(name))
                        {
                            sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\nis being tested\r\n");
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
                        name = split[1].Substring(1);            
                        do
                        {
                            line = sr.ReadLine();
                        } while (line.Length != 0);         // want the very last line, atm is producing the optional header lines

                        dictionary[name] = line;

                        sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                        sw.Flush();
                    }

                }
                else if(line.EndsWith("HTTP/1.1"))
                {
                    if (split[0] == "GET")
                    {
                        name = split[1].Substring(7);    
                        if (dictionary.ContainsKey(name))
                        {
                            sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nis being tested");
                            
                            sw.Flush();
                        }
                        else
                        {
                            sw.WriteLine("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                            sw.Flush();
                        }
                    }
                    else if (split[0] == "POST")
                    {

                        do
                        {
                            line = sr.ReadLine();
                        } while (line.Contains("name=<name>&location=<>"));

                        name = split[2];
                        location = split.Last();
                        dictionary[name] = location;

                        sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                        sw.Flush();
                    }
                }

                else         //0.9 or whois
                {                           // TETS 30
                    if ((split[0] == "GET" || split[0] == "PUT") && split.Length == 1)   // if the name is GET for whois, so this is for whois
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
                    else if (split[0] == "GET" && split[1].StartsWith("/"))
                    {
                        name = split[1].Substring(1);

                        if (dictionary.ContainsKey(name))
                        {
                            sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\nis being tested");

                            sw.Flush();
                        }
                        else if (!dictionary.ContainsKey(name))
                        {
                            sw.WriteLine("HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                            sw.Flush();
                        }
                    }
                   
                    
                  /*  else if (split[0] == "PUT" && split[1].StartsWith("/"))  // this is for the whois protocol, when the name is PUT and the location starts with a /
                    {                                                           // this is for further test 29 on lab 4 however cant distuniguish between this and 
                                                                                // the PUT 0.9 update

                        name = split[0];
                        location = string.Join(" ", split.Skip(1).ToArray());
                        dictionary[name] = location;

                        sw.WriteLine("OK");
                        sw.Flush();
                    }*/
                    else if (split[0] == "PUT" && split[1].StartsWith("/"))  // for the 0.9 if there is a location and it is an update
                    {

                        name = split[1];
                        do
                        {
                            line = sr.ReadLine();
                        } while (line.Length == 1);

                        //  location = sr.ReadLine();
                        dictionary[name] = line;

                        sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                        sw.Flush();
                    }
                                                                        
                    else if (split.Length == 1)         //whois          // get the person out of the dictionary as there is no location
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

