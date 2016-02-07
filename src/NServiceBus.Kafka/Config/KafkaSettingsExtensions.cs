namespace NServiceBus
{
    using System;
    using Configuration.AdvanceExtensibility;
    using Transports.Kafka;
    using Transports.Kafka.Routing;

    /// <summary>
    /// Adds access to the Kafka transport config to the global Transports object
    /// </summary>
    public static class KafkaSettingsExtensions
    {
        /// <summary>
        /// Use the direct routing topology with the given conventions
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="routingKeyConvention">The routing key conventions.</param>
        /// <param name="exchangeNameConvention">The exchange name convention.</param>
        public static TransportExtensions<KafkaTransport> UseDirectRoutingTopology(this TransportExtensions<KafkaTransport> transportExtensions, Func<Type, string> routingKeyConvention = null, Func<Address, Type, string> exchangeNameConvention = null)
        {
            if (routingKeyConvention == null)
            {
                routingKeyConvention = DefaultRoutingKeyConvention.GenerateRoutingKey;
            }

            if (exchangeNameConvention == null)
            {
                exchangeNameConvention = (address, eventType) => "amq.topic";
            }

            //transportExtensions.GetSettings().Set<DirectRoutingTopology.Conventions>(new DirectRoutingTopology.Conventions(exchangeNameConvention, routingKeyConvention));

            return transportExtensions;
        }

        /// <summary>
        /// Register a custom routing topology
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static TransportExtensions<KafkaTransport> UseRoutingTopology<T>(this TransportExtensions<KafkaTransport> transportExtensions) where T : IRoutingTopology
        {
            transportExtensions.GetSettings().Set<IRoutingTopology>(Activator.CreateInstance<T>());
            return transportExtensions;
        }

        /// <summary>
        /// Registers a custom connection manager te be used
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static TransportExtensions<KafkaTransport> UseConnectionManager<T>(this TransportExtensions<KafkaTransport> transportExtensions) where T : IManageKafkaConnections
        {
            transportExtensions.GetSettings().Set("IManageKafkaConnections", typeof(T));
            return transportExtensions;
        }

        /// <summary>
        /// Disables the separate receiver that pulls messages from the machine specific callback queue
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <returns></returns>
        public static TransportExtensions<KafkaTransport> DisableCallbackReceiver(this TransportExtensions<KafkaTransport> transportExtensions) 
        {
            transportExtensions.GetSettings().Set(Features.KafkaTransportFeature.UseCallbackReceiverSettingKey, false);
            return transportExtensions;
        }

        /// <summary>
        /// Changes the number of threads that should be used for the callback receiver. The default is 1
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="maxConcurrency">The new value for concurrency</param>
        /// <returns></returns>
        public static TransportExtensions<KafkaTransport> CallbackReceiverMaxConcurrency(this TransportExtensions<KafkaTransport> transportExtensions, int maxConcurrency)
        {
            if (maxConcurrency <= 0)
            {
                throw new ArgumentException("Maximum concurrency value must be greater than zero.", "maxConcurrency");
            }
            transportExtensions.GetSettings().Set(Features.KafkaTransportFeature.MaxConcurrencyForCallbackReceiver, maxConcurrency);
            return transportExtensions;
        }

        /// <summary>
        /// Allows the user to control how the message id is determined. Mostly useful when doing native integration with non NSB endpoints
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="customIdStrategy">The user defined strategy for giving the message a unique id</param>
        /// <returns></returns>
        public static TransportExtensions<KafkaTransport> CustomMessageIdStrategy(this TransportExtensions<KafkaTransport> transportExtensions, Func<dynamic,string> customIdStrategy)
        {

            transportExtensions.GetSettings().Set(Features.KafkaTransportFeature.CustomMessageIdStrategy, customIdStrategy);
            return transportExtensions;
        }
    }
}