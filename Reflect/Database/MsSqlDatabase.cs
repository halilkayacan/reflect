using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflect.Database
{
    public class MsSqlDatabase : IDatabase
    {
        public void DBLog(string Query)
        {
            if (Configuration.GetConfiguration("DBLog") == "1")
            {
                if (!System.IO.Directory.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/DBLog/")))
                {
                    System.IO.Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/DBLog/"));
                }
                System.IO.File.WriteAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/DBLog/" + DateTime.Now.ToFileTime().ToString() + ".txt"), Query);
            }
        }

        public void ExecuteStoredProcedure(SqlCommand command)
        {
            DBLog(command.CommandText);
            SqlConnection sqlConnection = new SqlConnection(Configuration.GetConfiguration("ConnectionString"));
            sqlConnection.Open();
            command.Connection = sqlConnection;
            command.ExecuteNonQuery();
            sqlConnection.Close();
        }

        public void ExecuteQuery(string Query)
        {
            DBLog(Query);
            SqlCommand command = new SqlCommand(Query);
            command.CommandType = CommandType.Text;
            SqlConnection sqlConnection = new SqlConnection(Configuration.GetConfiguration("ConnectionString"));
            sqlConnection.Open();
            command.Connection = sqlConnection;
            command.ExecuteNonQuery();
            sqlConnection.Close();
        }

        public int PrintQuery(string Query)
        {
            DBLog(Query);
            SqlCommand command = new SqlCommand(Query);
            command.CommandType = CommandType.Text;
            return PrintInt(command);
        }

        public object Print(SqlCommand Query)
        {
            return Print(Query, 0);
        }

        public object Print(SqlCommand Query, int Index)
        {
            DBLog(Query.CommandText);
            SqlConnection SQLConnection = new SqlConnection(Configuration.GetConfiguration("ConnectionString"));
            Query.Connection = SQLConnection;
            SQLConnection.Open();
            object _Return = "";
            SqlDataReader _SqlDataReader = Query.ExecuteReader();
            if ((_SqlDataReader.Read()))
            {
                if ((!_SqlDataReader.IsDBNull(Index)))
                {
                    _Return = _SqlDataReader.GetValue(Index);
                }
            }
            SQLConnection.Close();
            return _Return;
        }

        public int PrintInt(SqlCommand Query)
        {
            return Convert.ToInt32(Print(Query));
        }

        public DataSet GetDataSetByQuery(string Query)
        {
            DBLog(Query);
            SqlCommand command = new SqlCommand(Query);
            command.CommandType = CommandType.Text;
            SqlConnection sqlConnection = new SqlConnection(Configuration.GetConfiguration("ConnectionString"));
            sqlConnection.Open();
            command.Connection = sqlConnection;
            DataSet Return = new DataSet();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
            sqlDataAdapter.Fill(Return);
            sqlConnection.Close();
            return Return;
        }

        public DataSet GetDataSetByStoredProcedure(SqlCommand command)
        {
            DBLog(command.CommandText);
            SqlConnection sqlConnection = new SqlConnection(Configuration.GetConfiguration("ConnectionString"));
            sqlConnection.Open();
            command.Connection = sqlConnection;
            DataSet Return = new DataSet();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
            sqlDataAdapter.Fill(Return);
            sqlConnection.Close();
            return Return;
        }

        public void CreateIfNotExists()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(Configuration.GetConfiguration("ConnectionString"));
            var databaseName = connectionStringBuilder.InitialCatalog;

            connectionStringBuilder.InitialCatalog = "master";

            using (var connection = new SqlConnection(connectionStringBuilder.ToString()))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = string.Format("select * from master.dbo.sysdatabases where name='{0}'", databaseName);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // exists
                            return;
                    }

                    command.CommandText = string.Format("CREATE DATABASE {0}", databaseName);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
