﻿namespace NServiceBus.Transports.Kafka.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using NServiceBus.Transports.Kafka.Connection;
    using Support;

    class ConnectionConfiguration
    {
        public const ushort DefaultHeartBeatInSeconds = 5;
        public const int DefaultDequeueTimeout = 1;
        public const ushort DefaultPort = 5672;

        public static TimeSpan DefaultWaitTimeForConfirms = TimeSpan.FromSeconds(30);

        IDictionary<string, object> clientProperties = new Dictionary<string, object>();

        public ushort Port { get; set; }

        public string VirtualHost { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public ushort RequestedHeartbeat { get; set; }

        public int DequeueTimeout { get; set; }

        public ushort PrefetchCount { get; set; }

        public bool UsePublisherConfirms { get; set; }

        public TimeSpan MaxWaitTimeForConfirms { get; set; }

        public TimeSpan RetryDelay { get; set; }

        public IDictionary<string, object> ClientProperties
        {
            get { return clientProperties; }
            private set { clientProperties = value; }
        }

        public HostConfiguration HostConfiguration { get; private set; }

        public ConnectionConfiguration()
        {
            // set default values
            Port = DefaultPort;
            VirtualHost = "/";
            UserName = "guest";
            Password = "guest";
            RequestedHeartbeat = DefaultHeartBeatInSeconds;
            DequeueTimeout = DefaultDequeueTimeout;
            MaxWaitTimeForConfirms = DefaultWaitTimeForConfirms;
            RetryDelay = TimeSpan.FromSeconds(10);
            SetDefaultClientProperties();
            UsePublisherConfirms = true;
        }

        private void SetDefaultClientProperties()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var applicationNameAndPath = Environment.GetCommandLineArgs()[0];
            var applicationName = Path.GetFileName(applicationNameAndPath);
            var applicationPath = Path.GetDirectoryName(applicationNameAndPath);
            var hostname = RuntimeEnvironment.MachineName;

            clientProperties.Add("client_api", "NServiceBus");
            clientProperties.Add("nservicebus_version", version);
            clientProperties.Add("application", applicationName);
            clientProperties.Add("application_location", applicationPath);
            clientProperties.Add("machine_name", hostname);
            clientProperties.Add("user", UserName);
            clientProperties.Add("connected", DateTime.Now.ToString("G"));
        }

        public void Validate()
        {
            if (HostConfiguration == null)
            {
                throw new Exception("Invalid connection string. 'host' value must be supplied. e.g: \"host=myServer\"");
            }

            if (HostConfiguration.Port == 0)
            {
                HostConfiguration.Port = Port;
            }
        }

        public void ParseHosts(string hostsConnectionString)
        {
            var hostsAndPorts = hostsConnectionString.Split(',');

            if (hostsAndPorts.Length > 1)
            {
                var message =
                    "Multiple hosts are no longer supported. " +
                    "If you are using Kafka in a cluster, " +
                        "consider using a load balancer to represent the nodes as a single host.";

                throw new ArgumentException(message, "hostsConnectionString");
            }

            HostConfiguration =
                (from hostAndPort in hostsAndPorts
                 select hostAndPort.Split(':') into hostParts
                 let host = hostParts.ElementAt(0)
                 let portString = hostParts.ElementAtOrDefault(1)
                 let port = (portString == null) ? Port : ushort.Parse(portString)
                 select new HostConfiguration { Host = host, Port = port })
                .FirstOrDefault();
        }
    }
}
