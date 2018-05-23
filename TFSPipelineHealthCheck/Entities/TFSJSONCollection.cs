using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TFSPipelineHealthCheck.Entities
{
    [DataContract]
    public class TFSJSONCollection<T> 
    {
        [DataMember]
        public int count;
        [DataMember]
        public T[] value;
    }
}
