using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflect.Database
{
    public class Executor
    {
        private static IDatabase _Database;
        private static IDatabase Database
        {
            get
            {
                if (_Database == null)
                {
                    _Database = new MsSqlDatabase();
                }
                return _Database;
            }
        }

        private static void DBLog(string Query)
        {
            Database.DBLog(Query);
        }

        public static void ExecuteStoredProcedure(SqlCommand command)
        {
            Database.ExecuteStoredProcedure(command);
        }

        public static void ExecuteQuery(string Query)
        {
            Database.ExecuteQuery(Query);
        }

        public static int PrintQuery(string Query)
        {
            return Database.PrintQuery(Query);
        }

        public static object Print(SqlCommand Query)
        {
            return Database.Print(Query);
        }

        public static object Print(SqlCommand Query, int Index)
        {
            return Database.Print(Query, Index);
        }

        public static int PrintInt(SqlCommand Query)
        {
            return Database.PrintInt(Query);
        }

        public static DataSet GetDataSetByQuery(string Query)
        {
            return Database.GetDataSetByQuery(Query);
        }

        public static DataSet GetDataSetByStoredProcedure(SqlCommand command)
        {
            return Database.GetDataSetByStoredProcedure(command);
        }

        public static void CreateIfNotExists()
        {
            Database.CreateIfNotExists();
        }
    }
}
