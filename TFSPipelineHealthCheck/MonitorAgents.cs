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
        public int AgentHealthCheckInterval { get; set; } = 1000; // miliseconds
        private int eventId = 3;  //1 is reserved for start //2 is reserved for stop //so that be able to trigger some tasks on start and stop service events
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
        public System.Diagnostics.EventLog eventlog=null;
        public void Run()
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
                WriteLog(ex.Message +"\n"+ ex.StackTrace, EventLogEntryType.Error);
            }
            finally
            {
                IsRunning = false;
            }
        }
        public System.Timers.Timer GetTimer()
        {
            // Set up a timer to trigger every minute.  
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = AgentHealthCheckInterval;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            return timer;
        }
        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            if (!MonitorAgents.GetInstance().IsRunning)
            {
                MonitorAgents.GetInstance().Run();
            }
            //WriteLog("Monitoring the System", EventLogEntryType.Information);
        }

        private void WriteLog(string log, EventLogEntryType entryType)
        {
            if (eventlog != null)
                eventlog.WriteEntry(log, entryType, eventId++);
            else
                Console.WriteLine(log);
        }
    }
}
