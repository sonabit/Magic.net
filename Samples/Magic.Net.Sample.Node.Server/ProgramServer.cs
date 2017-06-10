using System;
using System.Diagnostics;
using Magic.Net.Server;

namespace Magic.Net.Sample.Node.Server
{
    internal static class ProgramServer
    {
        private static NodeSystem _nodeSystem;

        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            // URI  magic://hostname:port/SystemName

            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddService(() => (IMyDataService) new MyDataServiceImpl());

            _nodeSystem = new NodeSystem("TestSystem", serviceCollection);

            NamedPipeServerNetConnection pipeConnection = new NamedPipeServerNetConnection();
            pipeConnection.LinkTo(_nodeSystem);
            pipeConnection.ConnectionAccepted += PipeConnectionOnConnectionAccepted;
            _nodeSystem.Start();
            Console.WriteLine("Hub started: " + pipeConnection.LocalAddress);

            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(false);
            } while (key.Key != ConsoleKey.Enter);

            _nodeSystem.Stop();
        }

        private static void PipeConnectionOnConnectionAccepted(object sender, INetConnection e)
        {
            Console.WriteLine("Accepted " + e.GetType().Name + " from " + e.RemoteAddress);
        }
    }
}