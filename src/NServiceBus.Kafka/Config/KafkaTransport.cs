namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Features;
    using Transports;

    /// <summary>
    /// Transport definition for RabbirtMQ
    /// </summary>
    public class KafkaTransport : TransportDefinition
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public KafkaTransport()
        {
            HasNativePubSubSupport = true;
            HasSupportForCentralizedPubSub = true;
            HasSupportForDistributedTransactions = false;
        }

        /// <summary>
        /// Gives implementations access to the <see cref="T:NServiceBus.BusConfiguration"/> instance at configuration time.
        /// </summary>
        protected override void Configure(BusConfiguration config)
        {
            config.EnableFeature<KafkaTransportFeature>();
            config.EnableFeature<TimeoutManagerBasedDeferral>();

            config.GetSettings().EnableFeatureByDefault<TimeoutManager>();
        }
    }
}