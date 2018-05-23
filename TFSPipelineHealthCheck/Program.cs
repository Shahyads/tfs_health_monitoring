using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TFSPipelineHealthCheck
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static bool GetParameters(string[] args)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                MonitorAgents.GetInstance().includedAgentNames = !string.IsNullOrEmpty(config.AppSettings.Settings["include"].Value)? config.AppSettings.Settings["include"].Value.Split(';').ToList() : new List<string>();
                MonitorAgents.GetInstance().excludedAgentNames = !string.IsNullOrEmpty(config.AppSettings.Settings["exclude"].Value)? config.AppSettings.Settings["exclude"].Value.Split(';').ToList() : new List<string>();
                MonitorAgents.GetInstance().includedHostNames = !string.IsNullOrEmpty(config.AppSettings.Settings["includeHost"].Value)? config.AppSettings.Settings["includeHost"].Value.Split(';').ToList() : new List<string>();
                MonitorAgents.GetInstance().excludedHostNames = !string.IsNullOrEmpty(config.AppSettings.Settings["excludeHost"].Value)? config.AppSettings.Settings["excludeHost"].Value.Split(';').ToList() : new List<string>();
                MonitorAgents.GetInstance().SmtpServer = config.AppSettings.Settings["smtp"].Value;
                MonitorAgents.GetInstance().Port = int.Parse(config.AppSettings.Settings["port"].Value);
                MonitorAgents.GetInstance().From = config.AppSettings.Settings["from"].Value;
                MonitorAgents.GetInstance().To = config.AppSettings.Settings["to"].Value;
                Utility.TFSUri = config.AppSettings.Settings["tfsurl"].Value;
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-?" || args[i] == "/?" || args[i] == "-Help" || args[i] == "/Help")
                        throw new Exception("help");
                    else if (i == args.Length - 1)
                        throw new Exception("invalid number of parameters");
                    else if (args[i].Equals("-tfsurl", StringComparison.OrdinalIgnoreCase) || args[i].Equals("/tfsurl", StringComparison.OrdinalIgnoreCase))
                    {
                        Utility.TFSUri = args[++i];
                    }
                    else if (args[i].Equals("-include", StringComparison.OrdinalIgnoreCase) || args[i].Equals("/include", StringComparison.OrdinalIgnoreCase))
                    {
                        MonitorAgents.GetInstance().includedAgentNames = args[++i].Split(';').ToList();
                    }
                    else if (args[i].Equals("-exclude", StringComparison.OrdinalIgnoreCase) || args[i].Equals("/exclude", StringComparison.OrdinalIgnoreCase))
                    {
                        MonitorAgents.GetInstance().excludedAgentNames = args[++i].Split(';').ToList();
                    }
                    else if (args[i].Equals("-includeHost", StringComparison.OrdinalIgnoreCase) || args[i].Equals("/includeHost", StringComparison.OrdinalIgnoreCase))
                    {
                        MonitorAgents.GetInstance().includedHostNames = args[++i].Split(';').ToList();
                    }
                    else if (args[i].Equals("-excludeHost", StringComparison.OrdinalIgnoreCase) || args[i].Equals("/excludeHost", StringComparison.OrdinalIgnoreCase))
                    {
                        MonitorAgents.GetInstance().excludedHostNames = args[++i].Split(';').ToList();
                    }
                    else if (args[i].Equals("-smtp", StringComparison.OrdinalIgnoreCase) || args[i].Equals("/smtp", StringComparison.OrdinalIgnoreCase))
                    {
                        MonitorAgents.GetInstance().SmtpServer = args[++i];
                    }
                    else if (args[i].Equals("-port", StringComparison.OrdinalIgnoreCase) || args[i].Equals("/port", StringComparison.OrdinalIgnoreCase))
                    {
                        MonitorAgents.GetInstance().Port = int.Parse(args[++i]);
                    }
                    else if (args[i].Equals("-from", StringComparison.OrdinalIgnoreCase) || args[i].Equals("/from", StringComparison.OrdinalIgnoreCase))
                    {
                        MonitorAgents.GetInstance().From = args[++i];
                    }
                    else if (args[i].Equals("-to", StringComparison.OrdinalIgnoreCase) || args[i].Equals("/to", StringComparison.OrdinalIgnoreCase))
                    {
                        MonitorAgents.GetInstance().To = args[++i];
                    }
                    else
                    {
                        throw new Exception("invalid parameter " + args[i]);
                    }
                }
                if (string.IsNullOrEmpty(Utility.TFSUri))
                    throw new Exception("missing value \"tfsurl\" in command line parameter list or config file");
                if (string.IsNullOrEmpty(MonitorAgents.GetInstance().SmtpServer))
                    throw new Exception("missing value \"Smtp\" in command line parameter list or config file");
                if (string.IsNullOrEmpty(MonitorAgents.GetInstance().To))
                    throw new Exception("missing value \"To\" in command line parameter list or config file");
                if (string.IsNullOrEmpty(MonitorAgents.GetInstance().From))
                    throw new Exception("missing value \"From\" in command line parameter list or config file");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("TFS Pipeline Helth Check");
                Console.WriteLine("Monitor health of agents which are included except for those which are excluded.");
                Console.WriteLine();
                Console.WriteLine("By default all agents are included unless explicitly determine which are included.");
                Console.WriteLine("By default no agents are excluded unless explicitly determine which are excluded.");
                Console.WriteLine("By default smtp port number is 25");
                Console.WriteLine("Default values can be set in config file settings.");
                Console.WriteLine("Use command line arguments to overwrite config file settings.");
                Console.WriteLine();
                Console.WriteLine("Syntax:");
                Console.WriteLine("TFSPipelineHelthCheck -smtp smtpserver [-port port] -from senderEmailAddress -To recipientsEmailAddresses [-include agentNames] [-includeHost ComputerNames] [-exclude agentNames] [-excludeHost ComputerNames]   ");
                Console.WriteLine("tfsurl:                    tfs url address (e.g. http://hostname:8080/tfs");
                Console.WriteLine("smtpserver:                smtp server name");
                Console.WriteLine("port:                      smtp server port");
                Console.WriteLine("senderEmailAddress:        sender email address");
                Console.WriteLine("recipientsEmailAddresses:  emicolon separated list of recipients' email addresses");
                Console.WriteLine("agentNames:                semicolon separated list of intended agents");
                Console.WriteLine("computerNames:             semicolon separated list of computers which are hosting intended agents");
                Console.WriteLine();
                Console.WriteLine("Example1: For monitoring all agents.");
                Console.WriteLine("TFSPipelineHelthCheck -smtp MySmtpServer -from TFSAdmin@mycompany.com -to John@smith.com;Dave@John.com");
                Console.WriteLine();
                Console.WriteLine("Example2: For monitoring all agents hosted on Computer1 or Computer2 except for agent1, agent2, and agent3.");
                Console.WriteLine("TFSPipelineHelthCheck  -smtp MySmtpServer -from TFSAdmin@mycompany.com -to John@smith.com -includeHost Computer1;Computer2 -exclude agent1;agent2;agent3");
                Console.WriteLine();
                Console.WriteLine("Example3: For monitoring all agents except for those which are hosted on Computer1.");
                Console.WriteLine("TFSPipelineHelthCheck  -smtp MySmtpServer -from TFSAdmin@mycompany.com -to John@smith.com -excludeHost Computer1");
                return false;
            }
            return true;
        }
        static int Main(string[] args)
        {
            if (!GetParameters(args))
                return -1;
            Console.WriteLine("effective parameters:");
            Console.WriteLine(" TFSUrl = " + Utility.TFSUri.ToString());
            Console.WriteLine(" include = " + MonitorAgents.GetInstance().includedAgentNames.Aggregate("", (s, t) => s + (s == "" ? "" : ";") + t));
            Console.WriteLine(" exclude = " + MonitorAgents.GetInstance().excludedAgentNames.Aggregate("", (s, t) => s + (s == "" ? "" : ";") + t));
            Console.WriteLine(" includeHost = " + MonitorAgents.GetInstance().includedHostNames.Aggregate("", (s, t) => s + (s == ""? "" : ";") + t));
            Console.WriteLine(" excludeHost = " + MonitorAgents.GetInstance().excludedHostNames.Aggregate("", (s, t) => s + (s == "" ? "" : ";") + t));
            Console.WriteLine(" smtp = " + MonitorAgents.GetInstance().SmtpServer);
            Console.WriteLine(" port = " + MonitorAgents.GetInstance().Port.ToString());
            Console.WriteLine(" from = " + MonitorAgents.GetInstance().From);
            Console.WriteLine(" to = " + MonitorAgents.GetInstance().To);
            if (Environment.UserInteractive)
            {
                BasicHealthCheckService service1 = new BasicHealthCheckService(args);
                service1.TestStartupAndStop(args);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                   new BasicHealthCheckService(args)
                };
                ServiceBase.Run(ServicesToRun);
            }
            return 0;
        }
    }
}
