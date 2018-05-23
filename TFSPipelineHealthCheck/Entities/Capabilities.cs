using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TFSPipelineHealthCheck.Entities
{
    [DataContract]
    public class Capabilities
    {
        [DataMember(Name = "Agent.ComputerName")]
        public string ComputerName;
    }
}
