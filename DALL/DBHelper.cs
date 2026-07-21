using System.Data;
using Microsoft.Data.SqlClient;

namespace DL
{
    public class DBHelper
    {
       
       

        private readonly string _connectionString;

        public string ConnectionString =>
     _connectionString ?? throw new Exception("ConnectionString no inicializado");

        public DBHelper()
        {
            _connectionString =
                @"Server=(localdb)\MSSQLLocalDB;
                  Database=MF CYBER DB;
                  Trusted_Connection=True;
                  MultipleActiveResultSets=true";
        }

        private SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public DataTable ExecuteQuery(string query, SqlParameter[]? parameters = null)
        {
            using SqlConnection conn = CreateConnection();
            using SqlCommand cmd = new SqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            conn.Open();
            da.Fill(dt);

            return dt;
        }

        public int ExecuteNonQuery(string query, SqlParameter[]? parameters = null)
        {
            using SqlConnection conn = CreateConnection();
            using SqlCommand cmd = new SqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public object? ExecuteScalar(string query, SqlParameter[]? parameters = null)
        {
            using SqlConnection conn = CreateConnection();
            using SqlCommand cmd = new SqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            conn.Open();
            return cmd.ExecuteScalar();
        }

        internal SqlConnection GetConnection()
        {
            return CreateConnection();
        }
    }
}