using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using TFSPipelineHealthCheck.Entities;

namespace TFSPipelineHealthCheck
{
    public static class Utility
    {
        private static string tfsUri;
        public static string TFSUri
        {
            get { return tfsUri; }
            set { tfsUri = value.TrimEnd('/'); }
        }
        public static string InvokeRestApi(String MyURI, params object[] args)
        {
            WebRequest WReq = WebRequest.Create(new Uri(TFSUri+string.Format(MyURI,args)));
            WReq.Method = "GET";
            WReq.ContentType = "application/json";
            WReq.UseDefaultCredentials = true;
            //WReq.Credentials = new NetworkCredential("[user name]", "[password]", "[domain]");
            WebResponse response = WReq.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            return responseFromServer;
        }
        public static T JsonDeserialize<T>(string json) where T : class, new()
        {
            T obj = new T();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            obj = ser.ReadObject(ms) as T;
            ms.Close();
            return obj;
        }
        public static string JsonSerialize<T>(T obj)
        {
            //Create a stream to serialize the object to.  
            MemoryStream ms = new MemoryStream();
            // Serializer the User object to the stream.  
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(ms, obj);
            byte[] json = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(json, 0, json.Length);
        }
        public static List<Pool> GetPools()
        {
            var s = InvokeRestApi("/_apis/distributedtask/pools");
            var pools = JsonDeserialize<TFSJSONCollection<Pool>>(s);
            return pools.value.ToList();
        }
        public static List<Agent> GetAgents()
        {
            var agents = new List<Agent>();
            var pools = GetPools();
            foreach (Pool pool in pools)
            {
                agents.AddRange(GetAgents(pool));
            }
            return agents;
        }
        public static List<Agent> GetAgents(Pool pool)
        {
            var s = InvokeRestApi("/_apis/distributedtask/pools/{0}/Agents?includeCapabilities=true", pool.id);
            var agents = JsonDeserialize<TFSJSONCollection<Agent>>(s);
            return agents.value.ToList();
        }
        public static List<Agent> GetAgents(string computerName)
        {
            return GetAgents().Where(a => a.systemCapabilities.ComputerName == computerName).ToList();
        }
        public static void SendMail(string subject, string body, string from, string to, string smtpserver, int port)
        {
            MailMessage mail = new MailMessage(from, to);
            SmtpClient client = new SmtpClient();
            client.Port = port;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = true;
            client.Host = smtpserver;
            mail.Subject = subject;
            mail.Body = body;
            mail.Priority = MailPriority.High;
            client.Send(mail);
        }
    }
}
