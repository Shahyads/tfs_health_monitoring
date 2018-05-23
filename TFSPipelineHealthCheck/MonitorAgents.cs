using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TFSPipelineHealthCheck.Entities;

namespace TFSPipelineHealthCheck
{
    public class MonitorAgents
    {
        public List<string> includedAgentNames = new List<string>();
        public List<string> excludedAgentNames = new List<string>();
        public List<string> includedHostNames = new List<string>();
        public List<string> excludedHostNames = new List<string>();
        public string From { get; set; }
        public string To { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; } = 25;
        private static MonitorAgents instance = null;
        private MonitorAgents()
        {
        }
        public static MonitorAgents GetInstance()
        {
            if (instance == null)
                instance = new MonitorAgents();
            return instance;
        }
        public bool IsRunning
        {
            get;
            [MethodImpl(MethodImplOptions.Synchronized)]
            private set;
        } = false;
        private List<Agent> badAgentList = new List<Agent>();
        public void Run(System.Diagnostics.EventLog eventlog, ref int eventId)
        {
            try
            {
                IsRunning = true;
                List<Agent> newBadAgentList = Utility.GetAgents()
                    .Where(a=> (includedHostNames.Count == 0 || includedHostNames.Contains(a.systemCapabilities.ComputerName)) 
                    && !excludedHostNames.Contains(a.systemCapabilities.ComputerName)
                    && (includedAgentNames.Count==0 || includedAgentNames.Contains(a.name))
                    && !excludedAgentNames.Contains(a.name)
                    && (!a.enabled || a.status != "online")).ToList();
                foreach (Agent agent in newBadAgentList)
                {
                    if (!badAgentList.Where(a=>a.id == agent.id).Any())
                    {
                        Utility.SendMail("Agent \""+ agent.name + "\" is not online or enable", Utility.JsonSerialize<Agent>(agent), From, To, SmtpServer, Port);
                    }
                }
                badAgentList = newBadAgentList;
            }
            catch (Exception ex)
            {
                eventlog.WriteEntry(ex.Message +"\n"+ ex.StackTrace, EventLogEntryType.Error, eventId++);
            }
            finally
            {
                IsRunning = false;
            }
        }
    }
}
