using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
/* 
 * To do:
 * - Lab4 server - test 9 bung rested?
 */

namespace MyServer
{
    class Program
    {
        static Dictionary<string, string> dictionary = new Dictionary<string, string>();
        public static Logging Log;
        public static int timeoutServer = 500;

        static void Main(string[] args)
        {
            dictionary.Add("jess", "fenner");
            String filename = null;
            int timeoutServer = 500;
            for (int i = 0; i < args.Length; i ++)
            {
                switch (args[i])
                {
                    case "-l":
                        filename = args[++i];
                        break;
                    case "-t":
                        timeoutServer = int.Parse(args[++i]);
                        Console.WriteLine("Timeout for the server has been changed to: " + timeoutServer);
                        break;
                    default:
                        Console.WriteLine("Unknown Option: " + args[i]);
                        break;
                }
            }
            Log = new Logging(filename);
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
                string line = null;
                string name;
                string location;
                string lineMerge = null;

                String Host = ((IPEndPoint)connection.RemoteEndPoint).Address.ToString();
                NetworkStream socketStream;
                socketStream = new NetworkStream(connection);
                Console.WriteLine("Connection Recieved");
                
                String Status = "OK";
                
                socketStream.ReadTimeout = Program.timeoutServer;
                if (debug == true)  // do debug!
                {
                    Console.WriteLine("This is look-up for protocol whois and the username is: " + username + " and the location is: " + location);
                }


                try
                {

                    StreamWriter sw = new StreamWriter(socketStream);
                    StreamReader sr = new StreamReader(socketStream);
                    

                        line = sr.ReadLine();
                        
                    
                    string[] split = line.Split(' ');


                    if (line.EndsWith("HTTP/1.0"))
                    {
                        if (split[0] == "GET" || (split[0] == "GET" && split[1].StartsWith("/")))
                        {
                            name = split[1].Substring(2);
                            if (dictionary.ContainsKey(name))
                            {
                                sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n" + dictionary[name] + "\r\n");
                                sw.Flush();
                                lineMerge = "GET " + name + " is " + dictionary[name];
                                Status = "OK";
                            }
                            else if (!dictionary.ContainsKey(name))
                            {
                                sw.WriteLine("HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                                sw.Flush();
                                Status = "UNKNOWN";
                            }
                        }
                        else if (split[0] == "POST")
                        {
                            name = split[1].Substring(1);
                            string line2 = sr.ReadLine();
                            string line1;
                            do
                            {
                                line1 = sr.ReadLine();
                            } while (line1.Length == 0);

                           
                            dictionary[name] = line1;

                            sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                            sw.Flush();
                            lineMerge = "POST " + name + " location changed to be " + line1;
                            Status = "OK";
                        }

                    }
                    else if (line.EndsWith("HTTP/1.1"))
                    {
                        if (split[0] == "GET")
                        {
                            name = split[1].Substring(7);
                            if (dictionary.ContainsKey(name))
                            {
                                sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n" + dictionary[name]);
                                sw.Flush();
                                lineMerge = "GET " + name + " is " + dictionary[name];
                                Status = "OK";
                            }
                            else
                            {
                                sw.WriteLine("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                                sw.Flush();
                                Status = "UNKNOWN";
                            }
                        }
                        else if (split[0] == "POST")
                        {

                            string line2 = sr.ReadLine();
                            string line3 = sr.ReadLine();
                            string line1;
                            do
                            {
                                line1 = sr.ReadLine();
                            } while (line1.Length == 0);

                            string[] split1 = line1.Split('=', '&');
                            location = split1.Last();
                            name = split1[1];
                            dictionary[name] = location;

                            sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                            sw.Flush();
                            lineMerge = "POST " + name + " location changed to be " + line1;
                            Status = "OK";
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
                                lineMerge =  "GET " + name + " is " + dictionary[name];
                                Status = "OK";
                            }
                            else
                            {
                                sw.WriteLine("ERROR: no entries found");
                                sw.Flush();
                                Status = "UNKNOWN";
                            }

                        }
                        else if (split[0] == "GET" && split[1].StartsWith("/"))     // server query for the name for 0.9
                        {
                            name = split[1].Substring(1);

                            if (dictionary.ContainsKey(name))
                            {
                                sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n" + dictionary[name]);
                                sw.Flush();
                                lineMerge = "GET " + name + " is " + dictionary[name];
                                Status = "OK";
                            }
                            else if (!dictionary.ContainsKey(name))
                            {
                                sw.WriteLine("HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                                sw.Flush();
                                Status = "UNKNOWN";
                            }
                        }



                        else if (split[0] == "PUT" && split[1].StartsWith("/"))  // if the protocol is 0.9 and it is an update
                                                                                 // however hits this if its a whois where the name is PUT and location starts with a /
                        {
                            name = split[1].Substring(1);
                            string line1;
                            do
                            {
                               line1 = sr.ReadLine();
                            } while (line1.Length == 0);

                            dictionary[name] = line1;
                            lineMerge = "PUT " + line + " location changed to be " + line1;

                            sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                            sw.Flush();
                            Status = "OK";

                        }

                        else if (split.Length == 1)         //whois   search        
                        {
                            name = split[0];
                            if (dictionary.ContainsKey(name))
                            {
                                sw.WriteLine(dictionary[name]);
                                sw.Flush();
                                lineMerge = name + " is " + dictionary[name];
                                Status = "OK";
                            }
                            else
                            {
                                sw.WriteLine("ERROR: no entries found");
                                sw.Flush();
                                Status = "UNKNOWN";
                            }
                        }

                        else  //this is for the update of whois protocol
                        {
                            name = split[0];
                            location = string.Join(" ", split.Skip(1).ToArray());
                            dictionary[name] = location;
                            sw.WriteLine("OK");
                            sw.Flush();
                            lineMerge = name + " location changed to be " + location;
                            Status = "OK";

                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Something went wrong: " + e.ToString());
                    Status = "EXCEPTION";

                }

                finally
                {
                    socketStream.Close();
                    connection.Close();
                    Log.WriteToLog(Host, lineMerge, Status);
                }
            }
        }
    }
}

    public class Logging
    {
        public static String LogFile = null;

        public Logging(String filename)
        {
            LogFile = filename;
        }
        private static readonly object locker = new object();

        public void WriteToLog(String hostname, String lineMerge, String Status)
        {
            String Logline = hostname + " - - " + DateTime.Now.ToString("'['dd'/'MM'/'yyyy':'HH':'mm':'ss zz00']'") + " \"" + lineMerge + "\" " + Status;
            lock (locker)
            {
                Console.WriteLine(Logline);
                if (LogFile == null)
                    return;
                try
                {
                    StreamWriter SW;
                    SW = File.AppendText(LogFile);
                    SW.WriteLine(Logline);
                    SW.Close();
                }
                catch
                {
                    Console.WriteLine("Unable to write to the log file " + LogFile);
                }
            }
        }
    }
    


