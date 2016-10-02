using System;
using JetBrains.Annotations;

namespace Magic.Net.Server
{
    public class PipeSettings
    {
        private readonly Uri _uri;
        private readonly string _pipeName;

        public PipeSettings([NotNull]Uri uri)
        {
            _uri = uri;
            _pipeName = uri.GetStringOfSegment(1); ;
        }
        
        public Uri Uri
        {
            get { return _uri; }
        }

        public string PipeName
        {
            get { return _pipeName; }
        }
    }
}