using MongoDB.Driver;

namespace MongoDbCapabilities
{
    public class MyMongoClient : MongoClient, IMongoClient
    {
        public MyMongoClient(MongoOptions optionsMonitor)
            : base(optionsMonitor.ConnectionString)
        {
        }
    }
}
