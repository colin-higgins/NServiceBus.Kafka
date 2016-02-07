namespace NServiceBus.Transports.Kafka
{
    using System;
    using Config;
    using NServiceBus.Transports.Kafka.Connection;

    class KafkaConnectionManager : IDisposable, IManageKafkaConnections
    {
        public KafkaConnectionManager(KafkaConnectionFactory connectionFactory, ConnectionConfiguration connectionConfiguration)
        {
            this.connectionFactory = connectionFactory;
            this.connectionConfiguration = connectionConfiguration;
        }

        public object GetPublishConnection()
        {
            //note: The purpose is there so that we/users can add more advanced connection managers in the future
            lock (connectionFactory)
            {
                return connectionPublish ?? (connectionPublish = new PersistentConnection(connectionFactory, connectionConfiguration.RetryDelay,"Publish"));
            }
        }

        public object GetConsumeConnection()
        {
            //note: The purpose is there so that we/users can add more advanced connection managers in the future
            lock (connectionFactory)
            {
                return connectionConsume ?? (connectionConsume = new PersistentConnection(connectionFactory, connectionConfiguration.RetryDelay,"Consume"));
            }
        }

        public object GetAdministrationConnection()
        {
            //note: The purpose is there so that we/users can add more advanced connection managers in the future
            lock (connectionFactory)
            {
                return new PersistentConnection(connectionFactory, connectionConfiguration.RetryDelay,"Administration");
            }
        }

        public void Dispose()
        {
            //Injected at compile time
        }

        public void DisposeManaged()
        {

            if (connectionConsume != null)
            {
                connectionConsume.Dispose();
            }
            if (connectionPublish != null)
            {
                connectionPublish.Dispose();
            }
        }

        KafkaConnectionFactory connectionFactory;
        ConnectionConfiguration connectionConfiguration;
        PersistentConnection connectionConsume;
        PersistentConnection connectionPublish;
    }
}