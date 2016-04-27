using System;
using Magic.Net;

namespace NUnit.Magic.Net.Test.Helper
{
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TestNetConnection : NetConnectionAbstract
    {

        public TestNetConnection(IDataPackageHandler netCommandHandler)
            :base(netCommandHandler)
        {
            
        }

        public virtual NetDataPackage WrapedReadData()
        {
             throw new NotImplementedException();
        }

        public virtual void WrapedWriteData(byte[] data)
        {
            SendData(data);
        }

        #region Overrides of NetConnectionAbstract

        public override void Connect()
        {
            throw new NotImplementedException();
        }

        public override bool IsConnected
        {
            get { throw new NotImplementedException(); }
        }

        
        protected override NetDataPackage ReadData()
        {
            return WrapedReadData();
        }

        protected override void SendData(byte[] data)
        {
            WrapedWriteData(data);
        }

        #endregion

        public void OnReceivedData(NetDataPackage buffer)
        {
            

        }
        
        public void AddAddToReceivedDataQueue(NetDataPackage package)
        {
            base.AddToReceivedDataQueue(package);
        }

        public void CallDequeueReceivedData()
        {
            base.DequeueReceivedData();
        }
    }
}
