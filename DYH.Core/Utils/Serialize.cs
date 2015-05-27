using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace DYH.Core.Utils
{
    public class Serialize
    {
        public static T Dereference<T>(T source)
        {
            var formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Clone));
            object obj = null;
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                stream.Position = 0;
                obj = formatter.Deserialize(stream);
                stream.Flush();
                stream.Close();
            }

            var result = (T)Convert.ChangeType(obj, source.GetType());

            return result;
        }
    }
}
