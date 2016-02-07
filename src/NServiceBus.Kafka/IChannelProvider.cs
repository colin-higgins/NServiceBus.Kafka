namespace NServiceBus.Transports.Kafka
{
    interface IChannelProvider
    {
        bool TryGetPublishChannel(out dynamic channel);

        ConfirmsAwareChannel GetNewPublishChannel();
    }
}