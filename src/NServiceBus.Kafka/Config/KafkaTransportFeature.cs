namespace NServiceBus.Features
{
    using System;
    using System.Configuration;
    using NServiceBus.CircuitBreakers;
    using NServiceBus.Pipeline;
    using NServiceBus.Transports.Kafka.Connection;
    using Settings;
    using Support;
    using Transports;
    using Transports.Kafka;
    using Transports.Kafka.Config;
    using Transports.Kafka.Routing;

    class KafkaTransportFeature : ConfigureTransport
    {
        public const string UseCallbackReceiverSettingKey = "Kafka.UseCallbackReceiver";
        public const string MaxConcurrencyForCallbackReceiver = "Kafka.MaxConcurrencyForCallbackReceiver";
        public const string CustomMessageIdStrategy = "Kafka.CustomMessageIdStrategy";

        public KafkaTransportFeature()
        {
            Defaults(s =>
            {
                s.SetDefault(UseCallbackReceiverSettingKey, true);

                s.SetDefault(MaxConcurrencyForCallbackReceiver, 1);
            });
        }

        protected override string GetLocalAddress(ReadOnlySettings settings)
        {
            return Address.Parse(settings.Get<string>("NServiceBus.LocalAddress")).Queue;
        }

        protected override void Configure(FeatureConfigurationContext context, string connectionString)
        {
            var useCallbackReceiver = context.Settings.Get<bool>(UseCallbackReceiverSettingKey);
            var maxConcurrencyForCallbackReceiver = context.Settings.Get<int>(MaxConcurrencyForCallbackReceiver);
            var queueName = GetLocalAddress(context.Settings);
            var callbackQueue = string.Format("{0}.{1}", queueName, RuntimeEnvironment.MachineName);
            var connectionConfiguration = new ConnectionStringParser(context.Settings).Parse(connectionString);

            //MessageConverter messageConverter;

            //if (context.Settings.HasSetting(CustomMessageIdStrategy))
            //{
            //    messageConverter = new MessageConverter(context.Settings.Get<Func<BasicDeliverEventArgs, string>>(CustomMessageIdStrategy));
            //}
            //else
            //{
            //    messageConverter = new MessageConverter();
            //}

            string hostDisplayName;
            if (!context.Settings.TryGet("NServiceBus.HostInformation.DisplayName", out hostDisplayName))//this was added in 5.1.2 of the core
            {
                hostDisplayName = RuntimeEnvironment.MachineName;
            }

            var consumerTag = string.Format("{0} - {1}", hostDisplayName, context.Settings.EndpointName());

            var receiveOptions = new ReceiveOptions(workQueue =>
            {
                //if this isn't the main queue we shouldn't use callback receiver
                if (!useCallbackReceiver || workQueue != queueName)
                {
                    return SecondaryReceiveSettings.Disabled();
                }
                return SecondaryReceiveSettings.Enabled(callbackQueue, maxConcurrencyForCallbackReceiver);
            },
            null, //messageConverter,
            connectionConfiguration.PrefetchCount,
            connectionConfiguration.DequeueTimeout * 1000,
            context.Settings.GetOrDefault<bool>("Transport.PurgeOnStartup"),
            consumerTag);



            context.Container.RegisterSingleton(connectionConfiguration);

            context.Container.ConfigureComponent(builder => new KafkaDequeueStrategy(
                builder.Build<IManageKafkaConnections>(),
                SetupCircuitBreaker(builder.Build<CriticalError>()),
                receiveOptions), DependencyLifecycle.InstancePerCall);


            context.Container.ConfigureComponent<OpenPublishChannelBehavior>(DependencyLifecycle.InstancePerCall);

            context.Pipeline.Register<OpenPublishChannelBehavior.Registration>();
            context.Pipeline.Register<ReadIncomingCallbackAddressBehavior.Registration>();

            context.Container.ConfigureComponent(b => new KafkaMessageSender(b.Build<IRoutingTopology>(), b.Build<IChannelProvider>(), b.Build<PipelineExecutor>().CurrentContext),  DependencyLifecycle.InstancePerCall);

            if (useCallbackReceiver)
            {
                context.Container.ConfigureComponent<CallbackQueueCreator>(DependencyLifecycle.InstancePerCall)
                    .ConfigureProperty(p => p.Enabled, true)
                    .ConfigureProperty(p => p.CallbackQueueAddress, Address.Parse(callbackQueue));

                //context.Pipeline.Register<ForwardCallbackQueueHeaderBehavior.Registration>();
                context.Pipeline.Register<SetOutgoingCallbackAddressBehavior.Registration>();
                context.Container.ConfigureComponent<SetOutgoingCallbackAddressBehavior>(DependencyLifecycle.SingleInstance)
                    .ConfigureProperty(p => p.CallbackQueue, callbackQueue);
            }

            context.Container.ConfigureComponent<ChannelProvider>(DependencyLifecycle.InstancePerCall)
                  .ConfigureProperty(p => p.UsePublisherConfirms, connectionConfiguration.UsePublisherConfirms)
                  .ConfigureProperty(p => p.MaxWaitTimeForConfirms, connectionConfiguration.MaxWaitTimeForConfirms);

            context.Container.ConfigureComponent<KafkaDequeueStrategy>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureComponent<KafkaMessagePublisher>(DependencyLifecycle.InstancePerCall);

            context.Container.ConfigureComponent<KafkaSubscriptionManager>(DependencyLifecycle.SingleInstance)
             .ConfigureProperty(p => p.EndpointQueueName, queueName);

            context.Container.ConfigureComponent<KafkaQueueCreator>(DependencyLifecycle.InstancePerCall);

            if (context.Settings.HasSetting<IRoutingTopology>())
            {
                context.Container.RegisterSingleton(context.Settings.Get<IRoutingTopology>());
            }
            else
            {
                //var durable = GetDurableMessagesEnabled(context.Settings);

                //IRoutingTopology topology;

                //DirectRoutingTopology.Conventions conventions;


                //if (context.Settings.TryGet(out conventions))
                //{
                //    topology = new DirectRoutingTopology(conventions, durable);
                //}
                //else
                //{
                //    topology = new ConventionalRoutingTopology(durable);
                //}


                //context.Container.RegisterSingleton(topology);
            }

            if (context.Settings.HasSetting("IManageKafkaConnections"))
            {
                context.Container.ConfigureComponent(context.Settings.Get<Type>("IManageKafkaConnections"), DependencyLifecycle.SingleInstance);
            }
            else
            {
                context.Container.ConfigureComponent<KafkaConnectionManager>(DependencyLifecycle.SingleInstance);

                context.Container.ConfigureComponent(builder => new KafkaConnectionFactory(builder.Build<ConnectionConfiguration>()), DependencyLifecycle.InstancePerCall);
            }
        }

        static RepeatedFailuresOverTimeCircuitBreaker SetupCircuitBreaker(CriticalError criticalError)
        {

            var timeToWaitBeforeTriggering = TimeSpan.FromMinutes(2);
            var timeToWaitBeforeTriggeringOverride = ConfigurationManager.AppSettings["NServiceBus/KafkaDequeueStrategy/TimeToWaitBeforeTriggering"];

            if (!string.IsNullOrEmpty(timeToWaitBeforeTriggeringOverride))
            {
                timeToWaitBeforeTriggering = TimeSpan.Parse(timeToWaitBeforeTriggeringOverride);
            }

            var delayAfterFailure = TimeSpan.FromSeconds(5);
            var delayAfterFailureOverride = ConfigurationManager.AppSettings["NServiceBus/KafkaDequeueStrategy/DelayAfterFailure"];

            if (!string.IsNullOrEmpty(delayAfterFailureOverride))
            {
                delayAfterFailure = TimeSpan.Parse(delayAfterFailureOverride);
            }

            return new RepeatedFailuresOverTimeCircuitBreaker("KafkaConnectivity",
                timeToWaitBeforeTriggering, 
                ex => criticalError.Raise("Repeated failures when communicating with the broker",
                    ex), delayAfterFailure);
        }

        static bool GetDurableMessagesEnabled(ReadOnlySettings settings)
        {
            bool durableMessagesEnabled;
            if (settings.TryGet("Endpoint.DurableMessages", out durableMessagesEnabled))
            {
                return durableMessagesEnabled;
            }
            return true;
        }


        protected override string ExampleConnectionStringForErrorMessage
        {
            get { return "host=localhost"; }
        }

    }
}