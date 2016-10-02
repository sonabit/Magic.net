using System;

namespace Magic.Net.Sample.Node.Client
{
   
    internal sealed class ProgramClient : SimpleConsoleProgram
    {
        // URI  magic://hostname:port/SystemName/ServiceType/MethodName?param1=value1,param2=ein%20wert
        private static readonly Uri RemotePipeChannelUri = new Uri("magic://localhost:4242/TestSystem");

        private NodeSystem _nodeSystem;

        static void Main(string[] args)
        {
            Run(args, new ProgramClient());
        }

        protected override void Start(string[] args)
        {
            _nodeSystem = new NodeSystem();

            INetConnection connection = new NamedPipeNetConnection(RemotePipeChannelUri, _nodeSystem);
            _nodeSystem.AddConnection(connection);


            MyData wert = new MyData() { Id = 666, Text = "gaaaannnnzzzzz viel Text" };
            
            _nodeSystem.Start();

            //string result2 = _nodeSystem.Exc2<IMyDataService, string>(RemotePipeChannelUri, proxy => proxy.RemoveDuplicates(wert, 6) );
            
            object result = _nodeSystem.Exc(RemotePipeChannelUri, () => Proxy<IMyDataService>.Target.RemoveDuplicates(wert, 6));
            
        }

        protected override void Stop(string[] args)
        {
            _nodeSystem.Stop();
        }
    }
    
}
