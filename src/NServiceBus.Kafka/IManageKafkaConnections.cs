namespace NServiceBus.Transports.Kafka
{
    /// <summary>
    /// Allows users to provide their own connection strategies
    /// </summary>
    public interface IManageKafkaConnections
    {
        /// <summary>
        /// Gets a connection for outgoing operations
        /// </summary>
        /// <returns></returns>
        dynamic GetPublishConnection();
        /// <summary>
        /// Gets a connection for consuming messages from the broker
        /// </summary>
        /// <returns></returns>
        dynamic GetConsumeConnection();
        /// <summary>
        /// Get a admin connection to create queues, exchanges etc
        /// </summary>
        /// <returns></returns>
        dynamic GetAdministrationConnection();
    }
}