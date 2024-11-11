using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Task1.Model
{
    /// <summary>
    /// Provides methods for executing stored procedures and retrieving data from a SQL Server database.
    /// </summary>
    public class DatabaseManager(string connectionString)
    {
        private readonly string connectionString = connectionString;

        /// <summary>
        /// Calls a stored procedure that does not return any results.
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure to call.</param>
        /// <param name="parameters">The parameters to pass to the stored procedure.</param>
        public void CallProcedure(string procedureName, params SqlParameter[] parameters)
        {
            using SqlConnection connection = new(connectionString);
            connection.Open();

            using SqlCommand command = new(procedureName, connection);
            command.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Calls a stored procedure and returns the results in a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure to call.</param>
        /// <returns>A <see cref="DataTable"/> containing the results of the stored procedure.</returns>
        public DataTable CallProcedure(string procedureName)
        {
            using SqlConnection connection = new(connectionString);
            connection.Open();

            using SqlCommand command = new(procedureName, connection);
            command.CommandType = CommandType.StoredProcedure;

            using SqlDataAdapter adapter = new(command);
            DataTable resultTable = new();

            adapter.Fill(resultTable);
            return resultTable;
        }
    }
}
