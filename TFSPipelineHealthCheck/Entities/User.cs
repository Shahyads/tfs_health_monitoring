using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TFSPipelineHealthCheck.Entities
{
    [DataContract]
    public class User
    {
        [DataMember]
        public string id;
        [DataMember]
        public string displayName;
        [DataMember]
        public string uniqueName;
    }
}
