using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TFSPipelineHealthCheck.Entities
{
    [DataContract]
    public class Agent
    {
        [DataMember]
        public int id;
        [DataMember]
        public string name;
        [DataMember]
        public Capabilities systemCapabilities;
        [DataMember]
        public string version;
        [DataMember]
        public bool enabled;
        [DataMember]
        public string status;
    }
}
