using System.Data;
using System.Data.Common;

namespace DadJokes.Api.Data
{
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Create a new DbConnection. Caller is responsible for opening / disposing.
        /// </summary>
        DbConnection CreateConnection();
    }
}
