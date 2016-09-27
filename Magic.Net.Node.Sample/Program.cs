using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magic.Net.Server;

namespace Magic.Net.Node.Sample
{
    internal static class Program
    {
        private static NodeSystem _nodeSystem;

        private static void Main(string[] args)
        {
            var myPipeChannel = new Uri("mgpipe:///4738FBE3-B89F-4EAC-AE48-9012A393B633/TestSystem");
            _nodeSystem = new NodeSystem();

            INetConnection connection = new NamedPipeServerNetConnection(new PipeSettings(myPipeChannel), _nodeSystem);

            _nodeSystem.AddConnection(connection);

            _nodeSystem.Start();

            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(false);

            } while (key.Key != ConsoleKey.Enter);

            _nodeSystem.Stop();
            
        }
    }
}
