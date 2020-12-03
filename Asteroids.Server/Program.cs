using RKDnet;
using System;
using System.Threading;

namespace Asteroids.Server
{
    class Program
    {
        private static NetworkManager networkManager;
        private static bool networkRunning;

        private static void InterruptHandler(object sender, ConsoleCancelEventArgs args)
        {
            // Do a graceful shutdown
            args.Cancel = true;
            networkRunning = false;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Kadro.Logger.Start();

#if DEBUG
            networkManager = new NetworkManager("Asteroids-dev", 12345);
#else
            networkManager = new NetworkManager("Asteroids-dev", 24816);
#endif

            RoomManager<ServerGame> roomManager = new RoomManager<ServerGame>(networkManager)
            {
                AdminToken = new Random().Next()
            };
            Console.WriteLine("Token for admin access: " + roomManager.AdminToken);


            // Add the Ctrl-C handler
            Console.CancelKeyPress += InterruptHandler;

            // Run it
            networkManager.Start();
            networkRunning = true;

            while (networkRunning)
            {
                networkManager.ReadData(roomManager);
                Thread.Sleep(1);
            }

            roomManager.CloseRooms();
            networkManager.Shutdown();
        }
    }
}
