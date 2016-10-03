using System;
using Magic.Net.Server;

namespace Magic.Net.Sample.Node.Server
{
    internal  static class ProgramServer 
    {
        private static NodeSystem _nodeSystem;

        private static void Main(string[] args)
        {
            // URI  magic://hostname:port/direction/SystemName
            var mySystemChannelUri1 = new Uri("magic://localhost:4242/local/TestSystem");
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddService(() => (IMyDataService)new MyDataServiceImpl());
            _nodeSystem = new NodeSystem(mySystemChannelUri1, serviceCollection);
            NamedPipeServerNetConnection pipeConnection = new NamedPipeServerNetConnection(mySystemChannelUri1, _nodeSystem);
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
            Console.WriteLine("Accepted Connection " + e.RemoteAddress + e.GetType().Name);
        }
    }
}
