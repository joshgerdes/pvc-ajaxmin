using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcPlugins
{
    public class SourcemapStreamWriter: StreamWriter
    {
        public SourcemapStreamWriter(Stream stream, Encoding encoding) : base(stream, encoding)
        {

        }

        public override void Close()
        {
           
        }
    }
}
