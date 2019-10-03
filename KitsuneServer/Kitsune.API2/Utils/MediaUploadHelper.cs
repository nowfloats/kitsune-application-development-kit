using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.API2.Utils
{
    public class MediaUploadHelper
    {
        private const int BUFFER_SIZE = 4096;

        public static byte[] ReadInputStream(Stream inputStream)
        {
            using (var ms = new MemoryStream())
            using (inputStream)
            {
                var buffer = new byte[BUFFER_SIZE];
                var pos = 0;
                do
                {
                    pos = inputStream.Read(buffer, 0, BUFFER_SIZE);
                    ms.Write(buffer, 0, pos);
                } while (pos > 0);
                return ms.ToArray();
            }
        }
    }
}
