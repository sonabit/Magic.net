using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Magic.Net
{
    public static class UriExtension
    {
        [NotNull]
        public static string GetStringOfSegment([NotNull] this Uri uri, int segmentIdx)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (segmentIdx < 0) throw new ArgumentOutOfRangeException("segmentIdx");

            if (segmentIdx >= uri.Segments.Length)
                return  string.Empty;

            var result = uri.Segments[segmentIdx];
            if (result[result.Length - 1] == '/')
            {
                result = result.Substring(0, result.Length - 1);
            }
            return result;
        }
    }
}
