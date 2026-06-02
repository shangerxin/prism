using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace prism.infra.Model
{
    public interface IPrismTestResultsContext: IDisposable
    {
        bool Connect(string connectString = "mongodb://localhost:27017/", string database = "prism");
        void DisconnectDB();
        Task SaveChanges();

        Task<bool> AddResult(string result);
        Task<bool> AddParameter(string parameter);
        Task<bool> AddMetaData(string metadata);
        Task<bool> AddEnvironment(string environment);

        Task<bool> UpdateResult(string result);
        Task<bool> UpdateParameter(string parameter);
        Task<bool> UpdateMetaData(string metadata);
        Task<bool> UpdateEnvironment(string environment);

        Task<string> GetResult(string result);
        Task<string> GetParameter(string parameter);
        Task<string> GetMetaData(string metadata);
        Task<string> GetEnvironment(string environment);

        Task<bool> DeleteResult(string result);
        Task<bool> DeleteParameter(string parameter);
        Task<bool> DeleteMetaData(string metadata);
        Task<bool> DeleteEnvironment(string environment);

        Task<bool> AddResults(List<string> results);
        Task<bool> AddParameters(List<string> parameters);
        Task<bool> AddMetaDatas(List<string> metadatas);
        Task<bool> AddEnvironments(List<string> environments);
        
        Task<bool> UpdateResults(List<string> results);
        Task<bool> UpdateParameters(List<string> parameters);
        Task<bool> UpdateMetadatas(List<string> metadatas);
        Task<bool> UpdateEnvironments(List<string> environments);

        Task<bool> GetResults(List<string> results);
        Task<bool> GetParameters(List<string> parameters);
        Task<bool> GetMetaDatas(List<string> metadatas);
        Task<bool> GetEnvironments(List<string> environments);

        Task<bool> DeleteResults(List<string> results);
        Task<bool> DeleteParameters(List<string> parameters);
        Task<bool> DeleteMetaDatas(List<string> metadatas);
        Task<bool> DeleteEnvironments(List<string> environments);
    }
}

