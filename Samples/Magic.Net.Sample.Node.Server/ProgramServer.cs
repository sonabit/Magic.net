using System;
using Magic.Net.Server;

namespace Magic.Net.Sample.Node.Server
{
    internal  static class ProgramServer 
    {
        private static NodeSystem _nodeSystem;

        private static void Main(string[] args)
        {
            // URI  magic://hostname:port/SystemName

            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddService(() => (IMyDataService)new MyDataServiceImpl());

            _nodeSystem = new NodeSystem("TestSystem", serviceCollection);

            NamedPipeServerNetConnection pipeConnection = new NamedPipeServerNetConnection(_nodeSystem);
            pipeConnection.ConnectionAccepted += pipeConnectionOnConnectionAccepted;
            _nodeSystem.AddConnection(pipeConnection);
            _nodeSystem.Start();
            

            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(false);

            } while (key.Key != ConsoleKey.Enter);

            _nodeSystem.Stop();
            
        }

        private static void pipeConnectionOnConnectionAccepted(object sender, INetConnection e)
        {
            Console.WriteLine("Accepted "+ e.GetType().Name + " from " + e.RemoteAddress);
        }
    }
}
