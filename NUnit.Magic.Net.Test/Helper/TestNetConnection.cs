using System;
using Magic.Net;

namespace NUnit.Magic.Net.Test.Helper
{
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TestNetConnection : NetConnectionAbstract
    {
        public virtual NetCommandPackage WrapedReadData()
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

        
        protected override NetCommandPackage ReadData()
        {
            return WrapedReadData();
        }

        protected override void SendData(byte[] data)
        {
            WrapedWriteData(data);
        }

        #endregion

        public override void OnReceivedData(NetCommandPackage buffer)
        {
            

        }
        
        public void AddAddToReceivedDataQueue(NetCommandPackage package)
        {
            base.AddToReceivedDataQueue(package);
        }
    }
}
