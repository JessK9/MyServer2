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
                string name;
                string location;
                
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
               string[] protocol = line.Split(new char[] { ' ' }, 2);

                switch (protocol[0])
                {
                    case "GET": // QUERY
                        if(name == split[3])     // In 0.9, name is the 3rd 
                        {
                            
                               if (dictionary.ContainsKey(name))
                                {
                                    sw.WriteLine("HTTP/0.9 200 OK");
                                    sw.WriteLine("Content-Type: text/plain");
                                    sw.WriteLine();
                                    sw.WriteLine(dictionary[name]);  // is dictionary[name] the actual name or location?
                                    sw.Flush();
                                }
                                else
                                {
                                    sw.WriteLine("HTTP/0.9 404 NOT FOUND");
                                    sw.WriteLine("Content-Type: text/plain");
                                    sw.WriteLine();
                                    sw.WriteLine(dictionary[name]); //name or location??
                                    sw.Flush();
                                }
                        }
                        
                        else if(name == split[4])     // In 1.0, name is the 4th
                        {
                            name = split[4];
                            if (dictionary.ContainsKey(name))
                            {
                                sw.WriteLine("HTTP/1.0 200 OK");
                                sw.WriteLine("Content-Type text/plain");
                                sw.WriteLine();
                                sw.WriteLine(dictionary[name]);
                                sw.Flush();
                            }
                            else
                            {
                                sw.WriteLine("HTTP/1.0 404 Not Found");
                                sw.WriteLine("Content-Type: text/plain");
                                sw.WriteLine();
                                sw.Flush();
                            }
                        }

                        else if(name == split[6])       //In 1.1, name is the 6th
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
                                sw.WriteLine("HTTP/1.0 404 Not Found");
                                sw.WriteLine("Content-Type: text/plain");
                                sw.WriteLine();
                                sw.Flush();
                            }
                        }
                        
                        break;
                        
                    case "PUT":
                        name = split[3];
                        do
                        {
                            line = sr.ReadLine();
                        } while (line.Length != 0);

                        location = sr.ReadToEnd();
                        dictionary[name] = location;

                        sw.WriteLine("HTTP/0.9 200 OK");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
                        sw.Flush();
                        break;
                   
                    case "POST":    
                        if(name == split[3])   //This is for 1.0
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

                        else            //This is for 1.1, as it is the only possible other POST
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
                        break;

                    default:                                //This is the whois protocol
                        if (split.Length == 1)                   // get the person out of the dictionary as there is no location
                        {
                            name = split[0];
                            if (dictionary.ContainsKey(name))
                            {
                                sw.WriteLine(dictionary[name]);             // is this for the location or name? see step 3 in whois
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
                        break;
                }

                //whois protocol starts
                if (split.Length == 1)                   // get the person out of the dictionary as there is no location
                {
                     name = split[0];
                    if (dictionary.ContainsKey(name))
                    {
                        sw.WriteLine(dictionary[name]);             // is this for the location or name? see step 3 in whois
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
                //whois protocol ends

                //HTTP/0.9 protocol starts
                if(split[0] == "GET")
                {
                    name = split[3];
                    if (dictionary.ContainsKey(name))
                    {
                        sw.WriteLine("HTTP/0.9 200 OK");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
                        sw.WriteLine(dictionary[name]);  // is dictionary[name] the actual name or location?
                        sw.Flush();
                    }
                    else
                    {
                        sw.WriteLine("HTTP/0.9 404 NOT FOUND");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
                        sw.WriteLine(dictionary[name]); //name or location??
                        sw.Flush();
                    }
                }
                else if(split[0] == "PUT")
                {
                    name = split[3];
                    do
                    {
                        line = sr.ReadLine();
                    } while (line.Length != 0);

                     location = sr.ReadToEnd();
                    dictionary[name] = location;

                    sw.WriteLine("HTTP/0.9 200 OK");
                    sw.WriteLine("Content-Type: text/plain");
                    sw.WriteLine();
                    sw.Flush();
                }
                // HTTP/0.9 protocol ends

                // HTTP/1.0 protocol starts

                if (split[0] == "GET")
                {
                    name = split[4];
                    if (dictionary.ContainsKey(name))
                    {
                        sw.WriteLine("HTTP/1.0 200 OK");
                        sw.WriteLine("Content-Type text/plain");
                        sw.WriteLine();
                        sw.WriteLine(dictionary[name]);
                        sw.Flush();
                    }
                    else
                    {
                        sw.WriteLine("HTTP/1.0 404 Not Found");
                        sw.WriteLine("Content-Type: text/plain");
                        sw.WriteLine();
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

                // HTTP 1.0 protocol ends

                // HTTO/1.1 protocol starts
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
                        sw.WriteLine("HTTP/1.0 404 Not Found");
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
                //HTTP/1.1 protocol ends

            }

            catch (Exception e)
            {
                Console.WriteLine("Something went wrong: " + e.ToString());
            }
        }
    }
}

