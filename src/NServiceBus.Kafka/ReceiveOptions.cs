namespace NServiceBus.Transports.Kafka
{
    using System;

    class ReceiveOptions
    {
        public object Converter { get; private set; }
        public ushort DefaultPrefetchCount { get; private set; }
        public int DequeueTimeout { get; private set; }
        public bool PurgeOnStartup { get; private set; }
        public string ConsumerTag { get; private set; }

        public ReceiveOptions(Func<string, SecondaryReceiveSettings> getSecondaryReceiveSettings,
            object converter,
            ushort defaultPrefetchCount, 
            int dequeueTimeout,
            bool purgeOnStartup,
            string consumerTag)
        {
            Converter = converter;
            DefaultPrefetchCount = defaultPrefetchCount;
            DequeueTimeout = dequeueTimeout;
            PurgeOnStartup = purgeOnStartup;
            ConsumerTag = consumerTag;
            secondaryReceiveSettings = getSecondaryReceiveSettings;
        }

        public SecondaryReceiveSettings GetSettings(string queue)
        {
            return secondaryReceiveSettings(queue);
        }

        Func<string, SecondaryReceiveSettings> secondaryReceiveSettings;

    }
}