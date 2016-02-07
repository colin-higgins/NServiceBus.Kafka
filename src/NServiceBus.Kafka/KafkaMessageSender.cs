namespace NServiceBus.Transports.Kafka
{
    using NServiceBus.Pipeline;
    using Routing;
    using Unicast;

    class KafkaMessageSender : ISendMessages
    {
        IRoutingTopology routingTopology;
        IChannelProvider channelProvider;
        BehaviorContext context;

        public KafkaMessageSender(IRoutingTopology routingTopology, IChannelProvider channelProvider, BehaviorContext context)
        {
            this.routingTopology = routingTopology;
            this.channelProvider = channelProvider;
            this.context = context;
        }

        public void Send(TransportMessage message, SendOptions sendOptions)
        {
            dynamic channel;

            if (channelProvider.TryGetPublishChannel(out channel))
            {
                SendMessage(message, sendOptions, channel);
            }
            else
            {
                using (var confirmsAwareChannel = channelProvider.GetNewPublishChannel())
                {
                    SendMessage(message, sendOptions, confirmsAwareChannel.Channel);
                }
            }
        }

        void SendMessage(TransportMessage message, SendOptions sendOptions, dynamic channel)
        {
            var destination = DetermineDestination(sendOptions);
            var properties = channel.CreateBasicProperties();

            KafkaTransportMessageExtensions.FillKafkaProperties(message, sendOptions, properties);

            routingTopology.Send(channel, destination, message, properties);
        }

        Address DetermineDestination(SendOptions sendOptions)
        {
            return RequestorProvidedCallbackAddress(sendOptions) ?? SenderProvidedDestination(sendOptions);
        }

        static Address SenderProvidedDestination(SendOptions sendOptions)
        {
            return sendOptions.Destination;
        }

        Address RequestorProvidedCallbackAddress(SendOptions sendOptions)
        {
            string callbackAddress;
            if (IsReply(sendOptions) && context.TryGet(CallbackHeaderKey, out callbackAddress))
            {
                return Address.Parse(callbackAddress);
            }
            return null;
        }

        static bool IsReply(SendOptions sendOptions)
        {
            return sendOptions.GetType().FullName.EndsWith("ReplyOptions");
        }

        public const string CallbackHeaderKey = "NServiceBus.Kafka.CallbackQueue";
    }
}