using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;


namespace MyServer
{
    class Program
    {
        static Dictionary<string, string> dictionary = new Dictionary<string, string>();
        
        

        static void Main(string[] args)
        {
           
            runServer();

        }
        
        static void runServer()
        {
            TcpListener listener;
            Socket connection;
            Handler RequestHandler;
            

            try
            {
                listener = new TcpListener(IPAddress.Any, 43);
                listener.Start();
                Console.WriteLine("Server started listening");
                while (true)
                {
                    connection = listener.AcceptSocket();
                    RequestHandler = new Handler();
                    Thread t = new Thread(() => RequestHandler.doRequest(connection));
                    t.Start();
                   
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
            }
        }

        class Handler
        {

            public void doRequest(Socket connection)
            {
                string line;
                string name;
                string location;

                NetworkStream socketStream;
                socketStream = new NetworkStream(connection);
                Console.WriteLine("Connection Recieved");
                                                                                                 socketStream.ReadTimeout = 500;
            
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
                        if (split[0] == "GET" || (split[0] == "GET" && split[1].StartsWith("/")))
                        {
                            name = split[1].Substring(2);
                            if (dictionary.ContainsKey(name))
                            {
                                sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\nis being tested\r\n");
                                sw.Flush();
                            }
                            else if (!dictionary.ContainsKey(name))
                            {
                                sw.WriteLine("HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                                sw.Flush();
                            }
                        }
                        else if (split[0] == "POST")
                        {
                            name = split[1].Substring(1);
                            
                                
                            
                            dictionary[name] = line;

                            sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                            sw.Flush();
                        }

                    }
                    else if (line.EndsWith("HTTP/1.1"))
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

                            string line2 = sr.ReadLine();
                            string line3 = sr.ReadLine();
                            
                            do
                            {
                                line = sr.ReadLine();
                            } while (line.Length == 0);
                            

                            /*for ( int i= 0; i < split.Length; i ++)
                             {

                                   
                                 if (line.Length == 0)
                                 {
                                     continue;
                                 }

                             }*/



                            string[] split1 = line.Split('=', '&');
                            location = split1.Last();
                            name = split1[1];
                            dictionary[name] = location;

                            sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                            sw.Flush();
                        }
                    }

                    else         //0.9 or whois
                    {     
                        
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
                        else if (split[0] == "GET" && split[1].StartsWith("/"))     // server query for the name for 0.9
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


                        else if (split[0] == "PUT" && split[1].StartsWith("/"))  // if the protocol is 0.9 and it is an update
                                                                                  // however hits this if its a whois where the name is PUT and location starts with a /
                        {
                            name = split[1].Substring(1);
                            do
                            {
                                line = sr.ReadLine();
                            } while (line.Length == 1);
                            
                            dictionary[name] = line;

                            sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                            sw.Flush();
                        }

                        else if (split.Length == 1)         //whois          
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

                        else  //this is for the update of whois protocol
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
                    Console.WriteLine("Something went wrong: " + e.ToString());
                    return;
                }

                finally
                {
                    socketStream.Close();
                    connection.Close();
                }
            }
        }
    }
}

