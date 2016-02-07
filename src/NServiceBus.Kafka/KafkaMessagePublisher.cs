namespace NServiceBus.Transports.Kafka
{
    using Routing;
    using Unicast;

    class KafkaMessagePublisher : IPublishMessages
    {
        public IRoutingTopology RoutingTopology { get; set; }

        public IChannelProvider ChannelProvider { get; set; }

        public void Publish(TransportMessage message, PublishOptions publishOptions)
        {
        
            dynamic channel;

            if (ChannelProvider.TryGetPublishChannel(out channel))
            {
                PublishMessage(message,publishOptions,channel);
            }
            else
            {
                using (var confirmsAwareChannel = ChannelProvider.GetNewPublishChannel())
                {
                    PublishMessage(message, publishOptions, confirmsAwareChannel.Channel);    
                }
                
            }
            
        }

        void PublishMessage(TransportMessage message, PublishOptions publishOptions, dynamic channel)
        {
            var eventType = publishOptions.EventType;

            var properties = channel.CreateBasicProperties();

            KafkaTransportMessageExtensions.FillKafkaProperties(message, publishOptions, properties);

            RoutingTopology.Publish(channel, eventType, message, properties);
        }
    }
}