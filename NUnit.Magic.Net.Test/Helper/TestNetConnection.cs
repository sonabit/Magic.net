using System;
using Magic.Net;

namespace NUnit.Magic.Net.Test.Helper
{
    public class TestNetConnection : NetConnectionAbstract
    {
        public virtual byte[] WrapedReadData()
        {
            return ReadData();
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

        protected override byte[] ReadData()
        {
            throw new NotImplementedException();
        }

        protected override void SendData(byte[] data)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
