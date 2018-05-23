using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TFSPipelineHealthCheck.Entities
{
    [DataContract]
    public class Pool
    {
        [DataMember]
        public int size;
        [DataMember]
        public int id;
        [DataMember]
        public string name;
        [DataMember]
        public User createdBy;
    }
}
