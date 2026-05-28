using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using prism.infra.Model;

using MongoDB.Bson;
using MongoDB.Driver;

namespace prism.web.service.Model
{
    public class PrismTestResultsContext : IPrismTestResultsContext
    {
        private bool disposedValue;

        //// 1. Your arbitrary JSON string
        //string json = "{ 'name': 'John Doe', 'age': 30, 'tags': ['csharp', 'mongodb'] }";

        //// 2. Parse the JSON into a BsonDocument
        //var document = BsonDocument.Parse(json);

        //// 3. Get your collection (typed as BsonDocument for arbitrary data)
        //var collection = database.GetCollection<BsonDocument>("MyCollection");

        //// 4. Insert the document
        //await collection.InsertOneAsync(document);

        public Task<bool> AddEnvironment(string environment)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddEnvironments(List<string> environments)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddMetaData(string metadata)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddMetaDatas(List<string> metadatas)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddParameter(string parameter)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddParameters(List<string> parameters)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddResult(string result)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddResults(List<string> results)
        {
            throw new NotImplementedException();
        }

        public Task ConnectDB(string connectionString)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteEnvironment(string environment)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteEnvironments(List<string> environments)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteMetaData(string metadata)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteMetaDatas(List<string> metadatas)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteParameter(string parameter)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteParameters(List<string> parameters)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteResult(string result)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteResults(List<string> results)
        {
            throw new NotImplementedException();
        }

        public Task DisconnectDB()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetEnvironment(string environment)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetEnvironments(List<string> environments)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetMetaData(string metadata)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetMetaDatas(List<string> metadatas)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetParameter(string parameter)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetParameters(List<string> parameters)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetResult(string result)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetResults(List<string> results)
        {
            throw new NotImplementedException();
        }

        public Task SaveChanges()
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateEnvironment(string environment)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateEnvironments(List<string> environments)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateMetaData(string metadata)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateMetadatas(List<string> metadatas)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateParameter(string parameter)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateParameters(List<string> parameters)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateResult(string result)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateResults(List<string> results)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~PrismTestResultsContext()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}