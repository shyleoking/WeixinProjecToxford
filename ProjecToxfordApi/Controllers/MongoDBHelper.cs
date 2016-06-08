using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;

namespace ProjecToxfordApi.Controllers
{
    public class MongoDBHelper<T>
    {
        private static string mongoDbServer = ConfigurationManager.ConnectionStrings["mongoConnection"].ConnectionString;
        private static string mongoDbName = ConfigurationManager.AppSettings["mongoDb"];

        private IMongoClient client;
        private IMongoDatabase database;
        private IMongoCollection<T> collection;

        public string Collection { get {
            return collection.CollectionNamespace.CollectionName; ;
        } }

        public string Database { get {
            return collection.Database.DatabaseNamespace.DatabaseName;
        } }

        public MongoDBHelper(string collectionName)
        {
            client = new MongoClient(mongoDbServer);
            database = client.GetDatabase(mongoDbName);
            collection = database.GetCollection<T>(collectionName);
        }

        public async Task InsertAsync(T document)
        {
            await collection.InsertOneAsync(document);
        }


        public async Task InsertAsync(IEnumerable<T> documents)
        {
            await collection.InsertManyAsync(documents);
        }

        public async Task<T> SelectOneAsync(Expression<Func<T, bool>> func)
        {
            return await collection.Find<T>(func).FirstOrDefaultAsync();
        }

        public async Task<List<T>> SelectMoreAsync(Expression<Func<T, bool>> func)
        {
            return await collection.Find(func).ToListAsync();
        }

    }
}