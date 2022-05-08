using StackExchange.Redis;

namespace FreeCourse.Services.Basket.Services
{
    public class RedisService
    {
        private readonly string host;
        private readonly int port;

        private ConnectionMultiplexer connectionMultiplexer;

        public RedisService(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public void Connect() => connectionMultiplexer = ConnectionMultiplexer.Connect($"{host}:{port}");
        public IDatabase GetDb(int db = 1) => connectionMultiplexer.GetDatabase(db);
    }
}
