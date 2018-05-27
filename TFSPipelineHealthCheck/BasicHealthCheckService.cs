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
            if (!args.Contains("-noEventLog"))
                MonitorAgents.GetInstance().eventlog = eventLog1;
            MonitorAgents.GetInstance().GetTimer().Start();
            eventLog1.WriteEntry("TFSPipelineHealthCheck Service is started", EventLogEntryType.Information, 1);
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("TFSPipelineHealthCheck Service is stoped.",EventLogEntryType.Information,2);
        }

        internal void TestStartupAndStop(string[] args)
        {
            args = args.Concat(new string[] { "-noEventLog" }).ToArray();
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
