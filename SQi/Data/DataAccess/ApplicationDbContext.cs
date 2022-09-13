using Data.MongoCollections;
using Data.ViewModels;
using MongoDB.Driver;
using System.Linq;

namespace Data.DataAccess
{
    public class ApplicationDbContext
    {
        private readonly IMongoDatabase _db;
        private IMongoClient _mongoClient;

        public ApplicationDbContext(IMongoClient client, string databaseName)
        {
            _db = client.GetDatabase(databaseName);
            _mongoClient = client;
        }
        public IMongoCollection<SQi> SQiCollection => _db.GetCollection<SQi>("sqi_collection");
        public IMongoCollection<AggregateData> ReportCollection => _db.GetCollection<AggregateData>("report_collection");

        public IClientSessionHandle StartSession()
        {
            var session = _mongoClient.StartSession();
            return session;
        }

        public void CreateCollectionsIfNotExists()
        {
            var collectionNames = _db.ListCollectionNames().ToList();
            if (!collectionNames.Any(name => name == "sqi_collection"))
            {
                _db.CreateCollection("sqi_collection");
            }
            if (!collectionNames.Any(name => name == "report_collection"))
            {
                _db.CreateCollection("report_collection");
            }
        }
    }
}
