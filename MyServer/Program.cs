using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Forms;


namespace MyServer
{
    class Program
    {
        static Dictionary<string, string> dictionary = new Dictionary<string, string>();
        public static Logging Log;
        public static int timeoutServer = 1000;
        public static bool debug;
        public static string filename = null;
        

        static void Main(string[] args)
        {
            dictionary.Add("jess", "fenner");
            
            
            for (int i = 0; i < args.Length; i++)
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
                    case "-d":
                        debug = true;
                        break;
                    case "-w":
                       for(int h = 0; h < args.Length; h++)
                        {
                            if (args[h] == "-t")
                            {
                                timeoutServer = int.Parse(args[++h]);
                                Console.WriteLine("Timeout for the server has been changed to: " + timeoutServer);

                            }
                            
                        }
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new Form1());
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
                    connection.SendTimeout = 1000/*Program.timeoutServer*/;
                    connection.ReceiveTimeout = 1000/*Program.timeoutServer*/;
                    RequestHandler = new Handler();
                    Thread t = new Thread(() => RequestHandler.doRequest(connection, Log));
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

            public void doRequest(Socket connection, Logging Log)
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
                
                try
                {

                    StreamWriter sw = new StreamWriter(socketStream);
                    StreamReader sr = new StreamReader(socketStream);
                    

                        line = sr.ReadLine();
                        
                    
                    string[] split = line.Split(' ');


                    if (line.EndsWith("HTTP/1.0"))
                    {
                        if (split[0] == "GET" || (split[0] == "GET" && split[1].StartsWith("/")))               //this is the lookup for the HTTP/1.0 protocol  
                        {
                            name = split[1].Substring(2);
                            if (dictionary.ContainsKey(name))
                            {
                                sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n" + dictionary[name] + "\r\n");
                                sw.Flush();
                                lineMerge = "GET " + name + " is " + dictionary[name];
                                Status = "OK";
                                if (debug == true)  
                                {
                                    Console.WriteLine("This is look-up for protocol HTTP/1.0 and the username is: " + name + " and the location is: " + dictionary[name]);
                                }

                                
                            }
                            else if (!dictionary.ContainsKey(name))
                            {
                                sw.WriteLine("HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                                sw.Flush();
                                Status = "UNKNOWN";
                            }
                        }
                        else if (split[0] == "POST")                    //this is the update for the HTTP/1.0 protocol  
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
                            if (debug == true)
                            {
                                Console.WriteLine("This is an update location for protocol HTTP/1.0 and the username is: " + name + " and the location is: " + line1);
                            }
                        }

                    }
                    else if (line.EndsWith("HTTP/1.1"))
                    {
                        if (split[0] == "GET")                   //this is lookup for the HTTP/1.1 protocol    
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
                        else if (split[0] == "POST")             //this is the update for the HTTP/1.1 protocol    
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

                    else         //The following are for either HTTP/0.9 or whois
                    {

                        if ((split[0] == "GET" || split[0] == "PUT") && split.Length == 1)   // if the name is GET or PUT for protocol whois
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
                        else if (split[0] == "GET" && split[1].StartsWith("/"))     // this is lookup for the HTTP/0.9 protocol    
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



                        else if (split[0] == "PUT" && split[1].StartsWith("/"))  //  //this is for the update for the HTTP/0.9 protocol    
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

                        else if (split.Length == 1)         //this is lookup for the whois protocol    
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
/// <summary>
/// The below Logging code is taken from Brian Tompsett's logging example from youtube: Client and Server Networking in C# - Step 7
/// https://www.youtube.com/watch?v=_oEVZSE1x64&list=PL3czsVugafjPqF4dO2vQY8EPRp3mfd_4u&index=10 
/// </summary>

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
    


