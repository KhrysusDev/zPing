using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace zPing
{
    /*
     * Created by KhryusDev on GitHub
     * KhrysusDev/zPing
     * Thursday 4th March 2021, 21:22 (UTC+0)
     * Licensed under Common Development and Distribution License (CDDL)
     *
     * Hi RavelCros!
     * Enjoy this source code, because I know when you see this your 1000000% going to rip it
     * Accept it as a gift from me, but this is here so companies can see some of the stuff I can do.
     * And for all you kids that are probably going to rip it, just remember, ripping gets you nowhere in life.
     * If you rip source, its highly unlikely that you will ever learn how to actually program.
     */

    internal class Program
    {
        private static IPAddress _host;

        private static ushort _port;

        private static TcpClient _client;

        private static Ping _icmpClient;

        private static PingOptions _icmpOptions;

        private static Thread _mainThread;

        private static bool _threadStop;

        private static async void TcpEvent()
        {
            Stopwatch stopWatch = new();
            while (!_threadStop)
            {
                _client = new TcpClient()
                {
                    ReceiveTimeout = 2000,
                    SendTimeout = 2000,
                    SendBufferSize = 100,
                    ExclusiveAddressUse = true,
                    LingerState = new LingerOption(false, 0),
                    NoDelay = true,
                    Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        ReceiveTimeout = 2000,
                        SendTimeout = 2000,
                        SendBufferSize = 100
                    }
                };

                // This is the delay between each probe, we do this so the users internet does not
                // limit it like maaaaaad
                await Task.Delay(1000);

                // Starts the stopwatch so we can get the response time in milliseconds
                stopWatch.Start();
                try
                {
                    await Task.Run(() =>
                    {
                        /*
                         * I shouldn't have to do this, but C# TcpClient doesn't take into account the timeout time I specified under 'SendTimeout' whatever, I don't care
                         * This is stupid, but for now it will work.
                         */
                        if (!_client.ConnectAsync(_host, _port).Wait(2000))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($" Timed out on connecting to {_host} on port {_port} in {stopWatch.ElapsedMilliseconds}ms");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($" Connected to {_host} on port {_port} in {stopWatch.ElapsedMilliseconds}ms");
                        }
                    });
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($" Timed out on connecting to {_host} on port {_port} in {stopWatch.ElapsedMilliseconds}ms");
                }
                finally
                {
                    _client.Close();
                    _client.Dispose();
                    stopWatch.Stop();
                    stopWatch.Reset();
                    Console.ResetColor();
                }
            }
        }

        private static async void IcmpEvent()
        {
            while (!_threadStop)
            {
                try
                {
                    await Task.Delay(1000);
                    var reply = await _icmpClient.SendPingAsync(_host, 2000, Encoding.UTF8.GetBytes("zPing ICMP echo request"), _icmpOptions);

                    if (reply.Status == IPStatus.Success)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($" ICMP echo reply from {_host} in {reply.RoundtripTime}ms");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($" No ICMP echo reply recieved from {_host} in {reply.RoundtripTime}ms ({reply.Status})");
                    }
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($" No ICMP echo response recieved from {_host} in -ms (exception)");
                }
                finally
                {
                    Console.ResetColor();
                }
            }
        }

        private static int ThrowError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" {error}\n");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" zPing [host] [port]\n zPing i [host]");
            Console.ResetColor();
            return 1;
        }

        private static void ExitEvent(object sender, EventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n Terminating threads and exiting zPing...");
            Console.ResetColor();
        }

        private static int Main(string[] args)
        {
            // Hooks the ProcessExit event so we can notify the user that the program has recieved a
            // stop signal
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ExitEvent);

            // Configure the console to use UTF-8 encoding
            Console.OutputEncoding = Encoding.UTF8;

            // Prints the banner
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(
                "\n ·▄▄▄▄• ▄▄▄·▪   ▐ ▄  ▄▄ •" +
                "\n ▪▀·.█▌▐█ ▄███ •█▌▐█▐█ ▀ ▪" +
                "\n ▄█▀▀▀• ██▀·▐█·▐█▐▐▌▄█ ▀█▄" +
                "\n █▌▪▄█▀▐█▪·•▐█▌██▐█▌▐█▄▪▐█" +
                "\n ·▀▀▀ •.▀   ▀▀▀▀▀ █▪·▀▀▀▀ " +
                $"\n\n Started zPing v2.1.0.1, source available at KhrysusDev/zPing on GitHub\n" +
                " This program is free of charge, if you paid you have been scammed, and should demand your money back.\n" +
                " Licensed under Common Development and Distribution License (CDDL)\n\n");

            // Checks the arguments, if they are 0 (nothing) print the message No arguments specified!
            if (args.Length == 0)
            {
                return ThrowError("No arguments specified!");
            }
            else if (args.Length == 2) // If the arguments are 2, so it's a valid amount of arguments
            {
                // Loops through the args, probably a better way of doing this, but this is alright
                // for now
                foreach (var item in args)
                {
                    if (args.First().StartsWith("i"))
                    {
                        if (_host == null && item != "i")
                        {
                            if (!IPAddress.TryParse(item, out _host)) // Tries to parse the input args as an IP address, if it fails then throw an error
                            {
                                return ThrowError("Invalid host specified!");
                            }
                        }
                    }
                    else
                    {
                        if (_host == null)
                        {
                            if (!IPAddress.TryParse(item, out _host)) // Tries to parse the input args as an IP address, if it fails then throw an error
                            {
                                return ThrowError("Invalid host specified!");
                            }
                        }
                        else if (_port == 0)
                        {
                            if (!ushort.TryParse(item, out _port)) // Tries to parse the input args as a unsigned short (int16), if it fails then throw an error
                            {
                                return ThrowError("Invalid port specified!");
                            }
                        }
                    }
                }
            }
            else
            {
                return ThrowError("Too many arguments specified!");  // If arguments are not 2 or 0 we fall back here, shows this message
            }
            if (_port == 0) // Port will only be 0 if ICMP was the switch
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($" Preparing ICMP echo request to {_host} (Press any key to stop)...");
                Console.ResetColor();
                _icmpClient = new Ping();
                _icmpOptions = new PingOptions()
                {
                    DontFragment = true,
                    Ttl = 57
                };
                _mainThread = new Thread(IcmpEvent) // Starts up a thread for the ICMP stuff, good because efficient and also it can be cancelled.
                {
                    Name = "IcmpThread",
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal
                };
                _mainThread.Start();
                Console.ReadKey();
                Console.ForegroundColor = ConsoleColor.Cyan;
                _threadStop = true; // Sets this so the thread will stop looping
                _mainThread.Join(); // Joins the thread
                Console.ResetColor();
                return 0;
            }
            else
            {
                // We fall back here for TCP probing.
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($" Preparing TCP probe to {_host} on port {_port} (Press any key to stop)...");
                Console.ResetColor();

                // Have to reinitalize it everytime because I can't seem to figure out how to reuse
                // the existing TcpClient
                _mainThread = new Thread(TcpEvent) // Fires up a new thread for the TCP stuff
                {
                    Name = "TcpThread",
                    IsBackground = true,
                    Priority = ThreadPriority.AboveNormal
                };
                _mainThread.Start();
                Console.ReadKey();
                Console.ForegroundColor = ConsoleColor.Cyan;
                _threadStop = true; // Sets this so the thread will stop looping
                _mainThread.Join(); // Joins the thread
                Console.ResetColor();
                return 0;
            }
        }
    }
}