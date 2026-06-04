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
        Task<bool> AddMetadata(string metadata);
        Task<bool> AddEnvironment(string environment);

        Task<bool> UpdateResult(string result);
        Task<bool> UpdateParameter(string parameter);
        Task<bool> UpdateMetadata(string metadata);
        Task<bool> UpdateEnvironment(string environment);

        Task<string> GetResult(string result);
        Task<string> GetParameter(string parameter);
        Task<string> GetMetadata(string metadata);
        Task<string> GetEnvironment(string environment);

        Task<bool> DeleteResult(string result);
        Task<bool> DeleteParameter(string parameter);
        Task<bool> DeleteMetadata(string metadata);
        Task<bool> DeleteEnvironment(string environment);

        Task<bool> AddResults(List<string> results);
        Task<bool> AddParameters(List<string> parameters);
        Task<bool> AddMetadata(List<string> metadata);
        Task<bool> AddEnvironments(List<string> environments);
        
        Task<bool> UpdateResults(List<string> results);
        Task<bool> UpdateParameters(List<string> parameters);
        Task<bool> UpdateMetadata(List<string> metadata);
        Task<bool> UpdateEnvironments(List<string> environments);

        Task<string> GetResults(List<string> results);
        Task<string> GetParameters(List<string> parameters);
        Task<string> GetMetadata(List<string> metadata);
        Task<string> GetEnvironments(List<string> environments);

        Task<bool> DeleteResults(List<string> results);
        Task<bool> DeleteParameters(List<string> parameters);
        Task<bool> DeleteMetadata(List<string> metadata);
        Task<bool> DeleteEnvironments(List<string> environments);
    }
}

