using System;
using System.Collections.Generic;

namespace Magic.Net.Sample.Node.Client
{
   
    internal sealed class ProgramClient : SimpleConsoleProgram
    {
        // URI  magic://hostname:port/direction/SystemName/ServiceType/MethodName?param1=value1,param2=ein%20wert
        private static readonly Uri RemotePipeChannelUri = new Uri("magic://fake-pc/TestSystem", UriKind.Relative);

        private NodeSystem _nodeSystem;

        static void Main(string[] args)
        {
            Run(args, new ProgramClient());
        }

        protected override void Start(string[] args)
        {
            _nodeSystem = new NodeSystem();

            INetConnection connection = new NamedPipeNetConnection(RemotePipeChannelUri, _nodeSystem);
            
            MyData wert = new MyData() { Id = 666, Text = "gaaaannnnzzzzz viel Text" };
            
            _nodeSystem.Start();

            //string result2 = _nodeSystem.Exc2<IMyDataService, string>(RemotePipeChannelUri, proxy => proxy.ReverseString(wert, 6) );
            try
            {
                string result = _nodeSystem.Execute(RemotePipeChannelUri, () => Proxy<IMyDataService>.Target.ReverseString(wert, 6), 2000);
                Console.WriteLine("Result: " + result);
            }
            catch (TimeoutException exception)
            {
                Console.WriteLine(exception.Message);
            }


            IEnumerator<object> objects = _nodeSystem.CreateObjectStream<object>(RemotePipeChannelUri);

            using (objects)
            {
                while (objects.MoveNext())
                {
                    Console.WriteLine(objects.Current);
                }
            }

        }

        protected override void Stop(string[] args)
        {
            _nodeSystem.Stop();
        }
    }
    
}
