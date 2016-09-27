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
            var pipeName = _uri.Segments[1];
            if (pipeName[pipeName.Length - 1] == '/')
            {
                pipeName = pipeName.Substring(0, pipeName.Length - 1);
            }
            _pipeName = pipeName;
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