using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflect.Database
{
    public interface IDatabase
    {
        void DBLog(string Query);
        void ExecuteStoredProcedure(SqlCommand command);
        void ExecuteQuery(string Query);
        int PrintQuery(string Query);
        object Print(SqlCommand Query);
        object Print(SqlCommand Query, int Index);
        int PrintInt(SqlCommand Query);
        DataSet GetDataSetByQuery(string Query);
        DataSet GetDataSetByStoredProcedure(SqlCommand command);
        void CreateIfNotExists();
    }
}
