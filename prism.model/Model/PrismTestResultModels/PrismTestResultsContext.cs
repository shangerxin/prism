using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using prism.infra.Model;
using System.Text.Json;

using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;
using System.Text.Json.Nodes;

namespace prism.web.service.Model
{
    public class PrismTestResultsContext : IPrismTestResultsContext
    {
        private bool disposedValue;
        protected IMongoClient _client;
        protected IMongoDatabase _database;
        protected bool _isConnected = false;

        public PrismTestResultsContext()
        {
            if (Connect())
            {
                Trace.WriteLine("Connected to mongodb successfully.");
            }
            else
            {
                Trace.WriteLine("Failed to connect to mongodb.");
            }
        }

        public bool Connect(string connectString = "mongodb://localhost:27017/", string database = "prism")
        {
            if (_isConnected)
            {
                return true;
            }
            _client = new MongoClient(connectString);
            _database = _client.GetDatabase(database);
            _isConnected = true;
            return true;
        }

        protected async Task<bool> Add(string collectionName, string data)
        {
            var json = JsonNode.Parse(data);
            var collection = _database.GetCollection<BsonDocument>(collectionName);
            if (collection == null)
            {
                await _database.CreateCollectionAsync(collectionName);
                collection = _database.GetCollection<BsonDocument>(collectionName);
            }
            var doc = BsonDocument.Parse(data);
            await collection.InsertOneAsync(doc);
            return true;
        }

        protected async Task<bool> Addex(string collectionTrailName, string data)
        {
            var json = JsonNode.Parse(data);
            var projectName = json["projectName"]?.GetValue<string>();
            var testJobName = json["testJobName"]?.GetValue<string>();
            var collectionName = $@"{projectName}.{testJobName}.{collectionTrailName}";
            return await Add(collectionName, data);
        }

        protected async Task<List<BsonDocument>> Get(string collectionName, string filter)
        {
            var collection = _database.GetCollection<BsonDocument>(collectionName);
            if (collection == null)
            {
                return null;
            }
            var bsonFilter = BsonDocument.Parse(filter);
            return await collection.Find(bsonFilter).ToListAsync();
        }

        protected async Task<List<BsonDocument>> Getex(string collectionTrailName, string filter)
        {
            var json = JsonNode.Parse(filter);
            var projectName = json["projectName"]?.GetValue<string>();
            var testJobName = json["testJobName"]?.GetValue<string>();
            var collectionName = $@"{projectName}.{testJobName}.{collectionTrailName}";
            return await Get(collectionName, filter);
        }

        protected async Task<bool> Delete(string collectionName, string filter)
        {
            var collection = _database.GetCollection<BsonDocument>(collectionName);
            if (collection == null)
            {
                return false;
            }
            var bsonFilter = BsonDocument.Parse(filter);
            var result = await collection.DeleteManyAsync(bsonFilter);
            return result.IsAcknowledged;
        }

        protected async Task<bool> Deleteex(string collectionTrailName, string filter)
        {
            var json = JsonNode.Parse(filter);
            var projectName = json["projectName"]?.GetValue<string>();
            var testJobName = json["testJobName"]?.GetValue<string>();
            var collectionName = $@"{projectName}.{testJobName}.{collectionTrailName}";
            return await Delete(collectionName, filter);
        }

        protected async Task<bool> Update(string collectionName, string filter, string update)
        {
            var collection = _database.GetCollection<BsonDocument>(collectionName);
            if (collection == null)
            {
                return false;
            }
            var bsonFilter = BsonDocument.Parse(filter);
            var bsonUpdate = BsonDocument.Parse(update);
            var result = await collection.UpdateManyAsync(bsonFilter, bsonUpdate);
            return result.IsAcknowledged;
        }

        protected async Task<bool> Updateex(string collectionTrailName, string filter, string update)
        {
            var json = JsonNode.Parse(filter);
            var projectName = json["projectName"]?.GetValue<string>();
            var testJobName = json["testJobName"]?.GetValue<string>();
            var guid = json["guid"]?.GetValue<string>();
            filter = $"{{ \"guid\": \"{guid}\" }}";
            var collectionName = $@"{projectName}.{testJobName}.{collectionTrailName}";
            return await Update(collectionName, filter, update);
        }

        #region Add
        public async Task<bool> AddEnvironment(string environment)
        {
            return await Addex("Environments", environment);
        }

        public async Task<bool> AddEnvironments(List<string> environments)
        {
            List<Task<bool>> result = new List<Task<bool>>();
            foreach (var environment in environments)
            {
                result.Add(AddEnvironment(environment));
            }
            return (await Task.WhenAll(result)).All(r => r);
        }

        public async Task<bool> AddMetaData(string metadata)
        {
            return await Addex("MetaDatas", metadata);
        }

        public async Task<bool> AddMetaDatas(List<string> metadatas)
        {
            List<Task<bool>> result = new List<Task<bool>>();
            foreach (var metadata in metadatas)
            {
                result.Add(AddMetaData(metadata));
            }
            return (await Task.WhenAll(result)).All(r => r);
        }

        public async Task<bool> AddParameter(string parameter)
        {
            return await Addex("Parameters", parameter);
        }

        public async Task<bool> AddParameters(List<string> parameters)
        {
            List<Task<bool>> result = new List<Task<bool>>();
            foreach (var parameter in parameters)
            {
                result.Add(AddParameter(parameter));
            }
            return (await Task.WhenAll(result)).All(r => r);
        }

        public async Task<bool> AddResult(string result)
        {
            return await Addex("Results", result);
        }

        public async Task<bool> AddResults(List<string> results)
        {
            List<Task<bool>> result = new List<Task<bool>>();
            foreach (var res in results)
            {
                result.Add(AddResult(res));
            }
            return (await Task.WhenAll(result)).All(r => r);
        }
        #endregion Add

        #region Delete
        public async Task<bool> DeleteEnvironment(string environment)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteEnvironments(List<string> environments)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteMetaData(string metadata)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteMetaDatas(List<string> metadatas)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteParameter(string parameter)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteParameters(List<string> parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteResult(string result)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteResults(List<string> results)
        {
            throw new NotImplementedException();
        }

        #endregion Delete 

        public void DisconnectDB()
        {
            if (_isConnected)
            {
                _client.Dispose();
                _isConnected = false;
            }
        }

        #region Get
        public async Task<string> GetEnvironment(string environment)
        {
            return await Getex("Environments", environment).ContinueWith(t => t.Result.FirstOrDefault()?.ToJson());
        }

        public async Task<bool> GetEnvironments(List<string> environments)
        {
            return await Task.WhenAll(environments.Select(e => GetEnvironment(e).ContinueWith(t => t.Result != null))).ContinueWith(t => t.Result.All(r => r));
        }

        public async Task<string> GetMetaData(string metadata)
        {
            return await Getex("MetaDatas", metadata).ContinueWith(t => t.Result.FirstOrDefault()?.ToJson());
        }

        public async Task<bool> GetMetaDatas(List<string> metadatas)
        {
            return await Task.WhenAll(metadatas.Select(m => GetMetaData(m).ContinueWith(t => t.Result != null))).ContinueWith(t => t.Result.All(r => r));
        }

        public async Task<string> GetParameter(string parameter)
        {
            return await Getex("Parameters", parameter).ContinueWith(t => t.Result.FirstOrDefault()?.ToJson());
        }

        public async Task<bool> GetParameters(List<string> parameters)
        {
            return await Task.WhenAll(parameters.Select(p => GetParameter(p).ContinueWith(t => t.Result != null))).ContinueWith(t => t.Result.All(r => r));
        }

        public async Task<string> GetResult(string result)
        {
            return await Getex("Results", result).ContinueWith(t => t.Result.FirstOrDefault()?.ToJson());
        }

        public async Task<bool> GetResults(List<string> results)
        {
            return await Task.WhenAll(results.Select(r => GetResult(r).ContinueWith(t => t.Result != null))).ContinueWith(t => t.Result.All(r => r));
        }

        #endregion Get

        public async Task SaveChanges()
        {
            if (!_isConnected)
            {
                return;
            }

            // MongoDB C# driver does not require explicit save changes, as operations are executed immediately.
            // This method is implemented for interface compatibility and future extensibility.
            await Task.CompletedTask;
        }

        #region Update
        public async Task<bool> UpdateEnvironment(string environment)
        {
            return await Updateex("Environments", environment, environment);
        }

        public async Task<bool> UpdateEnvironments(List<string> environments)
        {
            return await Task.WhenAll(environments.Select(e => UpdateEnvironment(e))).ContinueWith(t => t.Result.All(r => r));
        }

        public async Task<bool> UpdateMetaData(string metadata)
        {
            return await Updateex("MetaDatas", metadata, metadata);
        }

        public async Task<bool> UpdateMetadatas(List<string> metadatas)
        {
            return await Task.WhenAll(metadatas.Select(m => UpdateMetaData(m))).ContinueWith(t => t.Result.All(r => r));
        }

        public async Task<bool> UpdateParameter(string parameter)
        {
            return await Updateex("Parameters", parameter, parameter);
        }

        public async Task<bool> UpdateParameters(List<string> parameters)
        {
            return await Task.WhenAll(parameters.Select(p => UpdateParameter(p))).ContinueWith(t => t.Result.All(r => r));
        }

        public async Task<bool> UpdateResult(string result)
        {
            return await Updateex("Results", result, result);
        }

        public async Task<bool> UpdateResults(List<string> results)
        {
            return await Task.WhenAll(results.Select(r => UpdateResult(r))).ContinueWith(t => t.Result.All(r => r));
        }
        #endregion Update

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisconnectDB();
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