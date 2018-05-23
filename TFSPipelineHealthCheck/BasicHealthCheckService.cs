using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TFSPipelineHealthCheck
{
    public partial class BasicHealthCheckService : ServiceBase
    {
        public int AgentHealthCheckInterval { get; set; } = 1000; // miliseconds
        private int eventId = 1;
        private static BasicHealthCheckService instance = null;
        public static BasicHealthCheckService GetInstance()
        {
            if (instance == null)
                instance = new BasicHealthCheckService();
            return instance;
        }
        private BasicHealthCheckService()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists(eventLog1.Source))
            {
                System.Diagnostics.EventLog.CreateEventSource(eventLog1.Source, eventLog1.Log);
            }
        }

        protected override void OnStart(string[] args)
        {
            // Set up a timer to trigger every minute.  
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = AgentHealthCheckInterval;   
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();
            eventLog1.WriteEntry("TFSPipelineHealthCheck Service is started");
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("TFSPipelineHealthCheck Service is stoped.");
        }
        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            if (! MonitorAgents.GetInstance().IsRunning)
            {
                MonitorAgents.GetInstance().Run(eventLog1, ref eventId);
            }
            //eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
        }

        private static void MonitorTFSAgents()
        {
            var agents = Utility.GetAgents();
        }

        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.WriteLine("Monitoring is started ....");
            do
            {
                Console.WriteLine();
                Console.WriteLine("Enter capital Y to exit.");

            } while (Console.ReadKey().KeyChar != 'Y'); 
            this.OnStop();
        }
    }
}
