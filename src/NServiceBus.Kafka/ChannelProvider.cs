namespace NServiceBus.Transports.Kafka
{
    using System;
    using Pipeline;

    class ChannelProvider// : IChannelProvider
    {
        public PipelineExecutor PipelineExecutor { get; set; }

        public IManageKafkaConnections ConnectionManager { get; set; }

        public bool UsePublisherConfirms { get; set; }

        public TimeSpan MaxWaitTimeForConfirms { get; set; }


        //bool IChannelProvider.TryGetPublishChannel(out IModel channel)
        //{
        //    Lazy<ConfirmsAwareChannel> lazyChannel;

        //    if (!PipelineExecutor.CurrentContext.TryGet("Kafka.PublishChannel", out lazyChannel))
        //    {
        //        channel = null;
        //        return false;
        //    }

        //    channel = lazyChannel.Value.Channel;

        //    return true;
        //}

        public ConfirmsAwareChannel GetNewPublishChannel()
        {
            return null;
            //return new ConfirmsAwareChannel(ConnectionManager.GetPublishConnection(), UsePublisherConfirms, MaxWaitTimeForConfirms);
        }
    }
}