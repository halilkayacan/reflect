using Reflect.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflect
{
    public class Engine
    {
        public static void CreateDomain()
        {
            Database.Executor.CreateIfNotExists();
            Database.Executor.ExecuteQuery(DatabaseQuery());
        }

        public static List<ReflectEntity> SaveList(List<ReflectEntity> Objects)
        {
            List<ReflectEntity> SavedList = new List<ReflectEntity>();
            foreach (ReflectEntity Instance in Objects)
            {
                Instance.ID = Save(Instance);
                SavedList.Add(Instance);
            }
            return SavedList;
        }

        public static int GetUniqueID(object Instance)
        {
            int ID = ((ReflectEntity)Instance).ID;
            if (ID > 0)
            {
                return ID;
            }

            string Query = "SELECT [ID] FROM [dbo].[" + EncryptTableName(Instance.GetType().Name) + "]";
            string Filter = "[IsDeleted] = 0";

            foreach (System.Reflection.PropertyInfo SystemAttribute in Instance.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(UniqueAtrribute))))
            {
                if (Attribute.IsDefined(SystemAttribute, typeof(RelationAttribute)))
                {
                    foreach (System.Reflection.PropertyInfo RelatedAttribute in SystemAttribute.PropertyType.GetProperties())
                    {
                        if (Attribute.IsDefined(RelatedAttribute, typeof(NoDataAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(NoRelationAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(RelationAttribute)))
                        {
                            continue;
                        }

                        else if (RelatedAttribute.PropertyType.Namespace.StartsWith("System") || RelatedAttribute.PropertyType.IsEnum)
                        {
                            if (RelatedAttribute.PropertyType.IsEnum || RelatedAttribute.GetValue(Instance, null) != null)
                            {
                                Filter += " AND [" + SystemAttribute.Name + "." + RelatedAttribute + "] = '" + (IsCryptable(RelatedAttribute) ? EncryptColumnValue(RelatedAttribute.GetValue(Instance, null).ToString()) : RelatedAttribute.GetValue(Instance, null)) + "'";
                            }
                        }
                    }
                }
                else if (SystemAttribute.PropertyType.Namespace.StartsWith("System") || SystemAttribute.PropertyType.IsEnum)
                {
                    if (SystemAttribute.PropertyType.IsEnum || SystemAttribute.GetValue(Instance, null) != null)
                    {
                        Filter += " AND [" + SystemAttribute.Name + "] = '" + (IsCryptable(SystemAttribute) ? EncryptColumnValue(SystemAttribute.GetValue(Instance, null).ToString()) : SystemAttribute.GetValue(Instance, null)) + "'";
                    }
                }
            }

            if (Filter != "")
                Query += " WHERE " + Filter;

            try
            {
                ID = Database.Executor.PrintQuery(Query);
            }
            catch (Exception)
            {

            }

            return ID;
        }

        public static int Save(object Instance)
        {
            int ID = ((ReflectEntity)Instance).ID;
            ((ReflectEntity)Instance).UpdatedOn = DateTime.Now;

            if (Instance.GetType().IsSubclassOf(typeof(ReflectEntity)))
            {
                if (((ReflectEntity)Instance).ID == 0)
                {
                    ((ReflectEntity)Instance).CreatedOn = DateTime.Now;
                    string Columns = "";
                    string Values = "";

                    foreach (System.Reflection.PropertyInfo SystemAttribute in Instance.GetType().GetProperties())
                    {
                        if (Attribute.IsDefined(SystemAttribute, typeof(NoDataAttribute)))
                        {
                            continue;
                        }

                        if (SystemAttribute.Name != "ID")
                        {
                            if (Attribute.IsDefined(SystemAttribute, typeof(RelationAttribute)))
                            {
                                foreach (System.Reflection.PropertyInfo RelatedAttribute in SystemAttribute.PropertyType.GetProperties())
                                {
                                    if (Attribute.IsDefined(RelatedAttribute, typeof(NoDataAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(NoRelationAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(RelationAttribute)))
                                    {
                                        continue;
                                    }

                                    if (RelatedAttribute.PropertyType.Namespace.StartsWith("System") || RelatedAttribute.PropertyType.IsEnum)
                                    {
                                        if (Columns != "")
                                            Columns += ", ";

                                        Columns += "[" + EncryptColumnName(SystemAttribute.Name + "." + RelatedAttribute.Name) + "]";
                                    }
                                }
                            }
                            else if (SystemAttribute.PropertyType.Namespace.StartsWith("System") || SystemAttribute.PropertyType.IsEnum)
                            {
                                if (Columns != "")
                                    Columns += ", ";

                                Columns += "[" + EncryptColumnName(SystemAttribute.Name) + "]";
                            }

                            if (!SystemAttribute.PropertyType.IsEnum && SystemAttribute.GetValue(Instance, null) == null)
                            {
                                if (Values != "")
                                    Values += ", ";
                                Values += "NULL";
                            }
                            else
                            {
                                if (SystemAttribute.PropertyType == typeof(DateTime))
                                {
                                    if (Values != "")
                                        Values += ", ";

                                    if (Convert.ToDateTime(SystemAttribute.GetValue(Instance, null)) == DateTime.MinValue)
                                    {
                                        Values += "NULL";
                                    }
                                    else
                                    {
                                        Values += "'" + (IsCryptable(SystemAttribute) ? EncryptColumnValue(SystemAttribute.GetValue(Instance, null).ToString()) : new System.Data.SqlClient.SqlParameter("@var", SystemAttribute.GetValue(Instance, null)) { SqlDbType = SqlDbType.SmallDateTime }.SqlValue.ToString()) + "'";
                                    }
                                }
                                else if (Attribute.IsDefined(SystemAttribute, typeof(RelationAttribute)))
                                {
                                    foreach (System.Reflection.PropertyInfo RelatedAttribute in SystemAttribute.PropertyType.GetProperties())
                                    {
                                        if (Attribute.IsDefined(RelatedAttribute, typeof(NoDataAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(NoRelationAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(RelationAttribute)))
                                        {
                                            continue;
                                        }

                                        else if (RelatedAttribute.PropertyType.Namespace.StartsWith("System") || RelatedAttribute.PropertyType.IsEnum)
                                        {
                                            if (Values != "")
                                                Values += ", ";

                                            if (RelatedAttribute.GetValue(Instance, null) == null)
                                                Values += "[" + EncryptColumnName(SystemAttribute.Name + "." + RelatedAttribute.Name) + "] = NULL";
                                            else
                                                Values += "[" + EncryptColumnName(SystemAttribute.Name + "." + RelatedAttribute.Name) + "] = '" + (IsCryptable(RelatedAttribute) ? EncryptColumnValue(RelatedAttribute.GetValue(Instance, null).ToString()) : RelatedAttribute.GetValue(Instance, null)) + "'";
                                        }
                                    }
                                }
                                else if (SystemAttribute.PropertyType.Namespace.StartsWith("System") || SystemAttribute.PropertyType.IsEnum)
                                {
                                    if (Values != "")
                                        Values += ", ";

                                    Values += "'" + (IsCryptable(SystemAttribute) ? EncryptColumnValue(SystemAttribute.GetValue(Instance, null).ToString()) : SystemAttribute.GetValue(Instance, null)) + "'";
                                }
                            }
                        }
                    }

                    string Query = "INSERT INTO [dbo].[" + EncryptTableName(Instance.GetType().Name) + "] (" + Columns + ") VALUES (" + Values + "); SELECT SCOPE_IDENTITY();";

                    ID = Database.Executor.PrintQuery(Query);
                }
                else
                {
                    string Values = "";

                    foreach (System.Reflection.PropertyInfo SystemAttribute in Instance.GetType().GetProperties())
                    {
                        if (Attribute.IsDefined(SystemAttribute, typeof(NoDataAttribute)))
                        {
                            continue;
                        }

                        if (SystemAttribute.Name != "ID")
                        {
                            if (Values != "")
                                Values += ", ";

                            if (SystemAttribute.PropertyType == typeof(DateTime))
                            {
                                if (Convert.ToDateTime(SystemAttribute.GetValue(Instance, null)) == DateTime.MinValue)
                                {
                                    Values += "[" + SystemAttribute.Name + "] = NULL";
                                }
                                else
                                {
                                    Values += "[" + SystemAttribute.Name + "] = '" + (IsCryptable(SystemAttribute) ? EncryptColumnValue(SystemAttribute.GetValue(Instance, null).ToString()) : new System.Data.SqlClient.SqlParameter("@var", SystemAttribute.GetValue(Instance, null)) { SqlDbType = SqlDbType.SmallDateTime }.SqlValue.ToString()) + "'";
                                }
                            }
                            else if (Attribute.IsDefined(SystemAttribute, typeof(RelationAttribute)))
                            {
                                foreach (System.Reflection.PropertyInfo RelatedAttribute in SystemAttribute.PropertyType.GetProperties())
                                {
                                    if (Attribute.IsDefined(RelatedAttribute, typeof(NoDataAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(NoRelationAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(RelationAttribute)))
                                    {
                                        continue;
                                    }

                                    if (RelatedAttribute.PropertyType.Namespace.StartsWith("System") || RelatedAttribute.PropertyType.IsEnum)
                                    {
                                        if (!RelatedAttribute.PropertyType.IsEnum && RelatedAttribute.GetValue(Instance, null) == null)
                                            Values += "[" + EncryptColumnName(SystemAttribute.Name + "." + RelatedAttribute.Name) + "]= NULL";
                                        else
                                            Values += "[" + EncryptColumnName(SystemAttribute.Name + "." + RelatedAttribute.Name) + "]= '" + (IsCryptable(RelatedAttribute) ? EncryptColumnValue(RelatedAttribute.GetValue(Instance, null).ToString()) : RelatedAttribute.GetValue(Instance, null)) + "'";

                                    }

                                }
                            }
                            else if (SystemAttribute.PropertyType.Namespace.StartsWith("System") || SystemAttribute.PropertyType.IsEnum)
                            {
                                if (!SystemAttribute.PropertyType.IsEnum && SystemAttribute.GetValue(Instance, null) == null)
                                    Values += "[" + SystemAttribute.Name + "] = NULL";
                                else
                                    Values += "[" + SystemAttribute.Name + "] = '" + (IsCryptable(SystemAttribute) ? EncryptColumnValue(SystemAttribute.GetValue(Instance, null).ToString()) : SystemAttribute.GetValue(Instance, null)) + "'";
                            }


                        }
                    }

                    string Query = "UPDATE [dbo].[" + EncryptTableName(Instance.GetType().Name) + "] SET " + Values + " WHERE [ID] = '" + ((ReflectEntity)Instance).ID + "';";
                    Database.Executor.ExecuteQuery(Query);
                }
            }

            return ID;
        }

        public static void Delete(object Instance)
        {
            if (Instance.GetType().IsSubclassOf(typeof(ReflectEntity)))
            {
                string Query = "UPDATE [dbo].[" + EncryptTableName(Instance.GetType().Name) + "] SET [IsDeleted] = 1 WHERE ID = '" + ((ReflectEntity)Instance).ID + "';";
                Database.Executor.ExecuteQuery(Query);
            }
        }

        public static object GetSingleByParameters(System.Type SystemType)
        {
            return GetSingleByParameters(SystemType, null);
        }

        public static object GetSingleByParameters(System.Type SystemType, Dictionary<string, string> Parameters)
        {
            List<object> ObjectList = GetByParameters(SystemType, Parameters, 1, 1);
            if (ObjectList.Count == 0)
                return null;
            return ObjectList[0];
        }

        public static List<object> GetByParameters(System.Type SystemType)
        {
            return GetByParameters(SystemType, null);
        }

        public static List<object> GetByParameters(System.Type SystemType, Dictionary<string, string> Parameters)
        {
            return GetByParameters(SystemType, Parameters, int.MaxValue, 1);
        }

        public static List<object> GetByParameters(System.Type SystemType, Dictionary<string, string> Parameters, int Count, int Page)
        {
            string Query = "";
            foreach (System.Reflection.PropertyInfo SystemAttribute in SystemType.GetProperties())
            {
                if (Attribute.IsDefined(SystemAttribute, typeof(NoDataAttribute)))
                {
                    continue;
                }

                if (Attribute.IsDefined(SystemAttribute, typeof(RelationAttribute)))
                {
                    foreach (System.Reflection.PropertyInfo RelatedAttribute in SystemAttribute.PropertyType.GetProperties())
                    {
                        if (Attribute.IsDefined(RelatedAttribute, typeof(NoDataAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(NoRelationAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(RelationAttribute)))
                        {
                            continue;
                        }

                        if (Query != "")
                            Query += ", ";

                        Query += "[" + EncryptColumnName(SystemAttribute.Name + "." + RelatedAttribute.Name) + "]";
                    }
                }
                else
                {
                    if (Query != "")
                        Query += ", ";

                    Query += "[" + EncryptColumnName(SystemAttribute.Name) + "]";
                }
            }
            Query = "SELECT " + Query + " FROM [dbo].[" + EncryptTableName(SystemType.Name) + "]";
            string Filter = "[IsDeleted] = 0";
            if (Parameters != null)
            {
                if (Parameters.Count > 0)
                {
                    foreach (var Parameter in Parameters)
                    {
                        Filter += " AND [" + EncryptColumnName(Parameter.Key) + "] = '" + (IsCryptable(GetPropertyInfo(SystemType, Parameter.Key)) ? EncryptColumnValue(Parameter.Value) : Parameter.Value) + "'";
                    }
                }
            }
            if (Filter != "")
                Query += " WHERE " + Filter;

            return ObjectsByDataSet(SystemType, Database.Executor.GetDataSetByQuery(Query));
        }

        public static int GetCountByParameters(System.Type SystemType, Dictionary<string, string> Parameters)
        {
            string Query = "SELECT COUNT([ID]) FROM [dbo].[" + EncryptTableName(SystemType.Name) + "]";
            string Filter = "[IsDeleted] = 0";
            if (Parameters != null)
            {
                if (Parameters.Count > 0)
                {
                    foreach (var Parameter in Parameters)
                    {
                        Filter += " AND [" + EncryptColumnName(Parameter.Key) + "] = '" + (IsCryptable(GetPropertyInfo(SystemType, Parameter.Key)) ? EncryptColumnValue(Parameter.Value) : Parameter.Value) + "'";
                    }
                }
            }
            if (Filter != "")
                Query += " WHERE " + Filter;

            return Database.Executor.PrintQuery(Query);
        }

        public static object GetSingleByFilters(System.Type SystemType)
        {
            return GetSingleByFilters(SystemType, null);
        }

        public static object GetSingleByFilters(System.Type SystemType, List<Query.Filter> Filters)
        {
            List<object> ObjectList = GetByFilters(SystemType, Filters, 1, 1);
            if (ObjectList.Count == 0)
                return null;
            return ObjectList[0];
        }

        public static List<object> GetByFilters(System.Type SystemType)
        {
            return GetByFilters(SystemType, null);
        }

        public static List<object> GetByFilters(System.Type SystemType, List<Query.Filter> Filters)
        {
            return GetByFilters(SystemType, Filters, int.MaxValue, 1);
        }

        public static List<object> GetByFilters(System.Type SystemType, List<Query.Filter> Filters, int Count, int Page)
        {
            string Query = "";
            foreach (System.Reflection.PropertyInfo SystemAttribute in SystemType.GetProperties())
            {
                if (Attribute.IsDefined(SystemAttribute, typeof(NoDataAttribute)))
                {
                    continue;
                }

                if (Attribute.IsDefined(SystemAttribute, typeof(RelationAttribute)))
                {
                    foreach (System.Reflection.PropertyInfo RelatedAttribute in SystemAttribute.PropertyType.GetProperties())
                    {
                        if (Attribute.IsDefined(RelatedAttribute, typeof(NoDataAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(NoRelationAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(RelationAttribute)))
                        {
                            continue;
                        }

                        if (Query != "")
                            Query += ", ";
                        Query += "[dbo].[" + EncryptTableName(SystemType.Name) + "].[" + EncryptColumnName(SystemAttribute.Name + "." + RelatedAttribute.Name) + "]";
                    }
                }
                else
                {
                    if (Query != "")
                        Query += ", ";
                    Query += "[dbo].[" + EncryptTableName(SystemType.Name) + "].[" + EncryptColumnName(SystemAttribute.Name) + "]";
                }
            }

            Query = "SELECT DISTINCT " + Query + " FROM [dbo].[" + EncryptTableName(SystemType.Name) + "]";
            string Filter = "[IsDeleted] = 0";
            string Group = "";
            string Order = "";
            string Join = "";
            if (Filters != null)
            {
                if (Filters.Count > 0)
                {
                    foreach (var FilterObject in Filters.Where(x => x.Combine != Reflect.Query.FilterCombine.ORDERBY && x.Combine != Reflect.Query.FilterCombine.GROUPBY && x.Combine != Reflect.Query.FilterCombine.JOIN))
                    {
                        if (Filter != "")
                            Filter += " " + FilterObject.Combine.ToString() + " ";

                        string FilterCondition = "=";
                        string FilterValue = "'" + (IsCryptable(GetPropertyInfo(SystemType, FilterObject.Column)) ? EncryptColumnValue(FilterObject.Value) : FilterObject.Value) + "'";

                        switch (FilterObject.Condition)
                        {
                            case Reflect.Query.FilterCondition.Equals:
                                FilterCondition = "=";
                                break;
                            case Reflect.Query.FilterCondition.NotEquals:
                                FilterCondition = "!=";
                                break;
                            case Reflect.Query.FilterCondition.Higher:
                                FilterCondition = ">";
                                break;
                            case Reflect.Query.FilterCondition.Lower:
                                FilterCondition = "<";
                                break;
                            case Reflect.Query.FilterCondition.Like:
                                FilterCondition = "LIKE";
                                FilterValue = "'%" + (IsCryptable(GetPropertyInfo(SystemType, FilterObject.Column)) ? EncryptColumnValue(FilterObject.Value) : FilterObject.Value) + "%'";
                                break;
                            case Reflect.Query.FilterCondition.NotLike:
                                FilterCondition = "NOT LIKE";
                                FilterValue = "'%" + (IsCryptable(GetPropertyInfo(SystemType, FilterObject.Column)) ? EncryptColumnValue(FilterObject.Value) : FilterObject.Value) + "%'";
                                break;
                            case Reflect.Query.FilterCondition.HigherEquals:
                                FilterCondition = ">=";
                                break;
                            case Reflect.Query.FilterCondition.LowerEquals:
                                FilterCondition = "<=";
                                break;
                            default:
                                FilterCondition = "=";
                                break;
                        }

                        if (!string.IsNullOrEmpty(FilterObject.Query))
                        {
                            Filter += FilterObject.Query;
                        }
                        else
                        {
                            Filter += "[" + EncryptColumnName(FilterObject.Column) + "] " + FilterCondition + " " + FilterValue;
                        }
                    }

                    if (Filters.Where(x => x.Combine == Reflect.Query.FilterCombine.GROUPBY).Count() > 0)
                    {
                        string GroupBy = "";
                        foreach (var FilterObject in Filters.Where(x => x.Combine == Reflect.Query.FilterCombine.GROUPBY))
                        {
                            if (GroupBy != "")
                            {
                                GroupBy += ",";
                            }
                            GroupBy += FilterObject.Column;
                        }
                        Group = " GROUP BY " + GroupBy;
                    }

                    if (Filters.Where(x => x.Combine == Reflect.Query.FilterCombine.JOIN).Count() > 0)
                    {
                        foreach (var FilterObject in Filters.Where(x => x.Combine == Reflect.Query.FilterCombine.JOIN))
                        {
                            switch (FilterObject.Condition)
                            {
                                case Reflect.Query.FilterCondition.Inner:
                                    Join += " INNER JOIN ";
                                    break;
                                case Reflect.Query.FilterCondition.Outer:
                                    Join += " RIGHT OUTER JOIN ";
                                    break;
                                case Reflect.Query.FilterCondition.Left:
                                    Join += " LEFT OUTER JOIN ";
                                    break;
                                default:
                                    Join += " INNER JOIN ";
                                    break;
                            }
                            Join += FilterObject.Value + " ON " + FilterObject.Query;
                        }
                    }

                    if (Filters.Where(x => x.Combine == Reflect.Query.FilterCombine.ORDERBY).Count() > 0)
                    {
                        string GroupBy = "";
                        foreach (var FilterObject in Filters.Where(x => x.Combine == Reflect.Query.FilterCombine.ORDERBY))
                        {
                            if (GroupBy != "")
                            {
                                GroupBy += ",";
                            }

                            string FilterCondition = "ASC";
                            switch (FilterObject.Condition)
                            {
                                case Reflect.Query.FilterCondition.Ascending:
                                    FilterCondition = "ASC";
                                    break;
                                case Reflect.Query.FilterCondition.Descending:
                                    FilterCondition = "DESC";
                                    break;
                                default:
                                    FilterCondition = "ASC";
                                    break;
                            }

                            GroupBy += FilterObject.Column + " " + FilterCondition;
                        }
                        Order = " ORDER BY " + GroupBy;
                    }
                }
            }
            if (Join != "")
                Query += Join;
            if (Filter != "")
                Query += " WHERE " + Filter;

            if (Count != int.MaxValue && Count > 1 && Order == "")
            {
                Order = " ORDER BY [dbo].[" + EncryptTableName(SystemType.Name) + "].[ID]";
            }

            Query += Group + Order;

            if (Count != int.MaxValue && Count > 1)
            {
                Query += " OFFSET " + ((Page - 1) * Count) + " ROWS FETCH NEXT " + Count + " ROWS ONLY";
            }

            return ObjectsByDataSet(SystemType, Database.Executor.GetDataSetByQuery(Query));
        }

        public static int GetCountByFilters(System.Type SystemType, List<Query.Filter> Filters)
        {
            string Query = "SELECT COUNT([dbo].[" + EncryptTableName(SystemType.Name) + "].[ID]) FROM [dbo].[" + EncryptTableName(SystemType.Name) + "]";
            string Filter = "[IsDeleted] = 0";
            string Group = "";
            string Order = "";
            string Join = "";
            if (Filters != null)
            {
                if (Filters.Count > 0)
                {
                    foreach (var FilterObject in Filters.Where(x => x.Combine != Reflect.Query.FilterCombine.ORDERBY && x.Combine != Reflect.Query.FilterCombine.GROUPBY && x.Combine != Reflect.Query.FilterCombine.JOIN))
                    {
                        if (Filter != "")
                            Filter += " " + FilterObject.Combine.ToString() + " ";

                        string FilterCondition = "=";
                        string FilterValue = "'" + (IsCryptable(GetPropertyInfo(SystemType, FilterObject.Column)) ? EncryptColumnValue(FilterObject.Value) : FilterObject.Value) + "'";

                        switch (FilterObject.Condition)
                        {
                            case Reflect.Query.FilterCondition.Equals:
                                FilterCondition = "=";
                                break;
                            case Reflect.Query.FilterCondition.NotEquals:
                                FilterCondition = "!=";
                                break;
                            case Reflect.Query.FilterCondition.Higher:
                                FilterCondition = ">";
                                break;
                            case Reflect.Query.FilterCondition.Lower:
                                FilterCondition = "<";
                                break;
                            case Reflect.Query.FilterCondition.Like:
                                FilterCondition = "LIKE";
                                FilterValue = "'%" + (IsCryptable(GetPropertyInfo(SystemType, FilterObject.Column)) ? EncryptColumnValue(FilterObject.Value) : FilterObject.Value) + "%'";
                                break;
                            case Reflect.Query.FilterCondition.NotLike:
                                FilterCondition = "NOT LIKE";
                                FilterValue = "'%" + (IsCryptable(GetPropertyInfo(SystemType, FilterObject.Column)) ? EncryptColumnValue(FilterObject.Value) : FilterObject.Value) + "%'";
                                break;
                            case Reflect.Query.FilterCondition.HigherEquals:
                                FilterCondition = ">=";
                                break;
                            case Reflect.Query.FilterCondition.LowerEquals:
                                FilterCondition = "<=";
                                break;
                            default:
                                FilterCondition = "=";
                                break;
                        }

                        if (!string.IsNullOrEmpty(FilterObject.Query))
                        {
                            Filter += FilterObject.Query;
                        }
                        else
                        {
                            Filter += "[" + EncryptColumnName(FilterObject.Column) + "] " + FilterCondition + " " + FilterValue;
                        }
                    }

                    if (Filters.Where(x => x.Combine == Reflect.Query.FilterCombine.GROUPBY).Count() > 0)
                    {
                        string GroupBy = "";
                        foreach (var FilterObject in Filters.Where(x => x.Combine == Reflect.Query.FilterCombine.GROUPBY))
                        {
                            if (GroupBy != "")
                            {
                                GroupBy += ",";
                            }
                            GroupBy += FilterObject.Column;
                        }
                        Group = " GROUP BY " + GroupBy;
                    }

                    if (Filters.Where(x => x.Combine == Reflect.Query.FilterCombine.JOIN).Count() > 0)
                    {
                        foreach (var FilterObject in Filters.Where(x => x.Combine == Reflect.Query.FilterCombine.JOIN))
                        {
                            switch (FilterObject.Condition)
                            {
                                case Reflect.Query.FilterCondition.Inner:
                                    Join += " INNER JOIN ";
                                    break;
                                case Reflect.Query.FilterCondition.Outer:
                                    Join += " RIGHT OUTER JOIN ";
                                    break;
                                case Reflect.Query.FilterCondition.Left:
                                    Join += " LEFT OUTER JOIN ";
                                    break;
                                default:
                                    Join += " INNER JOIN ";
                                    break;
                            }
                            Join += FilterObject.Value + " ON " + FilterObject.Query;
                        }
                    }
                }
            }
            if (Join != "")
                Query += Join;
            if (Filter != "")
                Query += " WHERE " + Filter;
            Query += Order + Group;

            return Database.Executor.PrintQuery(Query);
        }

        public static List<object> ObjectsByDataSet(System.Type SystemType, DataSet Result)
        {
            if (Result.Tables.Count == 0)
            {
                return new List<object>();
            }

            if (Result.Tables[0].Rows.Count == 0)
            {
                return new List<object>();
            }

            List<object> ObjectList = new List<object>();

            foreach (System.Data.DataRow DataRow in Result.Tables[0].Rows)
            {
                object Instance = Activator.CreateInstance(SystemType);
                foreach (System.Reflection.PropertyInfo SystemAttribute in SystemType.GetProperties())
                {
                    if (Attribute.IsDefined(SystemAttribute, typeof(NoDataAttribute)))
                    {
                        continue;
                    }

                    try
                    {
                        if (SystemAttribute.PropertyType.IsEnum)
                        {
                            SystemAttribute.SetValue(Instance, Enum.Parse(SystemAttribute.PropertyType, (IsCryptable(SystemAttribute) ? DecryptColumnValue(DataRow[EncryptColumnName(SystemAttribute.Name)].ToString()) : DataRow[EncryptColumnName(SystemAttribute.Name)]).ToString(), true), null);
                        }
                        else if (SystemAttribute.PropertyType == typeof(Guid))
                        {
                            SystemAttribute.SetValue(Instance, Guid.Parse((IsCryptable(SystemAttribute) ? DecryptColumnValue(DataRow[EncryptColumnName(SystemAttribute.Name)].ToString()) : DataRow[EncryptColumnName(SystemAttribute.Name)]).ToString()), null);
                        }
                        else if (Attribute.IsDefined(SystemAttribute, typeof(RelationAttribute)))
                        {
                            object RelatedInstance = Activator.CreateInstance(SystemAttribute.PropertyType);

                            foreach (System.Reflection.PropertyInfo RelatedAttribute in SystemAttribute.PropertyType.GetProperties())
                            {
                                if (Attribute.IsDefined(RelatedAttribute, typeof(NoDataAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(RelationAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(NoRelationAttribute)))
                                {
                                    continue;
                                }

                                RelatedAttribute.SetValue(RelatedInstance, (IsCryptable(RelatedAttribute) ? DecryptColumnValue(DataRow[EncryptColumnName(SystemAttribute.Name + "." + RelatedAttribute.Name)].ToString()) : DataRow[EncryptColumnName(SystemAttribute.Name + "." + RelatedAttribute.Name)]), null);
                            }

                            SystemAttribute.SetValue(Instance, RelatedInstance);
                        }
                        else
                        {
                            SystemAttribute.SetValue(Instance, (IsCryptable(SystemAttribute) ? DecryptColumnValue(DataRow[EncryptColumnName(SystemAttribute.Name)].ToString()) : DataRow[EncryptColumnName(SystemAttribute.Name)]), null);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }

                ObjectList.Add(Instance);
            }

            return ObjectList;
        }

        public static string DatabaseQuery()
        {
            string Query = "";

            try
            {
                List<System.Type> SystemTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsSubclassOf(typeof(ReflectEntity)) && !type.Name.EndsWith("Object")).ToList();
                foreach (System.Type SystemType in SystemTypes)
                {
                    Query += ReflectionQuery(SystemType);

                }
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                throw new Exception("Reflection Exception: " + errorMessage);
            }

            return Query;
        }

        public static string ReflectionQuery(System.Type SystemType)
        {
            string Query = "";
            string AlterQuery = AlterTable(SystemType);

            if (AlterQuery != "")
            {
                Query += "IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'" + EncryptTableName(SystemType.Name) + "') \nBEGIN " + AlterTable(SystemType) + "\nEND ";
            }

            Query += "\nIF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'" + EncryptTableName(SystemType.Name) + "') \nBEGIN " + CreateTable(SystemType) + "\nEND ";
            return Query;
        }

        public static string CreateTable(System.Type SystemType)
        {
            int Seed = 1;

            foreach (var Attribute in SystemType.GetCustomAttributes(true))
            {
                if (Attribute is AutoAttribute)
                {
                    AutoAttribute AutoAttribute = (AutoAttribute)Attribute;
                    Seed = AutoAttribute.Seed;
                    break;
                }
            }

            string Query = "\nCREATE TABLE [dbo].[" + EncryptTableName(SystemType.Name) + "] ([ID] INT IDENTITY(" + Seed + ",1) NOT NULL PRIMARY KEY CLUSTERED); ";
            foreach (System.Reflection.PropertyInfo SystemAttribute in SystemType.GetProperties())
            {
                if (Attribute.IsDefined(SystemAttribute, typeof(NoDataAttribute)))
                {
                    continue;
                }

                if (SystemAttribute.Name != "ID")
                    Query += AddColumn(SystemAttribute, SystemType.Name, "");
            }
            return Query;
        }

        public static string AlterTable(System.Type SystemType)
        {
            string Query = "";
            foreach (System.Reflection.PropertyInfo SystemAttribute in SystemType.GetProperties())
            {
                if (Attribute.IsDefined(SystemAttribute, typeof(NoDataAttribute)))
                {
                    continue;
                }

                if (Attribute.IsDefined(SystemAttribute, typeof(RelationAttribute)))
                {
                    foreach (System.Reflection.PropertyInfo RelatedAttribute in SystemAttribute.PropertyType.GetProperties())
                    {
                        if (Attribute.IsDefined(RelatedAttribute, typeof(NoDataAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(NoRelationAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(RelationAttribute)))
                        {
                            continue;
                        }

                        Query += "\nIF COL_LENGTH('" + EncryptTableName(SystemType.Name) + "','" + EncryptColumnName(SystemAttribute.Name + "." + RelatedAttribute.Name) + "') IS NULL \nBEGIN " + AddColumn(SystemAttribute, SystemType.Name, "") + "\nEND ";
                    }
                }
                else
                {
                    if (SystemAttribute.Name != "ID")
                        Query += "\nIF COL_LENGTH('" + EncryptTableName(SystemType.Name) + "','" + EncryptColumnName(SystemAttribute.Name) + "') IS NULL \nBEGIN " + AddColumn(SystemAttribute, SystemType.Name, "") + "\nEND ";
                }
            }
            return Query;
        }

        public static string AddColumn(System.Reflection.PropertyInfo SystemAttribute, string Table, string NameSpace)
        {
            string Query = "";
            string ColumnType = "VARCHAR(MAX)";

            if (SystemAttribute.PropertyType == typeof(int))
            {
                ColumnType = "INT";
            }
            if (SystemAttribute.PropertyType == typeof(double))
            {
                ColumnType = "FLOAT";
            }
            if (SystemAttribute.PropertyType == typeof(decimal))
            {
                ColumnType = "DECIMAL";
            }
            if (SystemAttribute.PropertyType == typeof(DateTime))
            {
                ColumnType = "SMALLDATETIME";
            }
            if (SystemAttribute.PropertyType == typeof(bool))
            {
                ColumnType = "BIT";
            }
            if (SystemAttribute.PropertyType == typeof(Guid))
            {
                ColumnType = "UNIQUEIDENTIFIER";
            }

            if (Attribute.IsDefined(SystemAttribute, typeof(RelationAttribute)))
            {
                foreach (System.Reflection.PropertyInfo RelatedAttribute in SystemAttribute.PropertyType.GetProperties())
                {
                    if (Attribute.IsDefined(RelatedAttribute, typeof(NoDataAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(NoRelationAttribute)) || Attribute.IsDefined(RelatedAttribute, typeof(RelationAttribute)))
                    {
                        continue;
                    }

                    if (SystemAttribute.Name != "ID")
                        Query += AddColumn(RelatedAttribute, SystemAttribute.ReflectedType.Name, SystemAttribute.Name);
                }
            }
            else
            {
                if (NameSpace != "")
                {
                    Query = "\nALTER TABLE [dbo].[" + EncryptTableName(Table) + "] ADD [" + EncryptColumnName(NameSpace + "." + SystemAttribute.Name) + "] " + ColumnType + " NULL; ";
                }
                else
                {
                    Query = "\nALTER TABLE [dbo].[" + EncryptTableName(Table) + "] ADD [" + EncryptColumnName(SystemAttribute.Name) + "] " + ColumnType + " NULL; ";
                }
            }

            return Query;
        }

        public static string EncryptColumnName(string Name)
        {
            if (Convert.ToInt32(Configuration.GetConfiguration("CryptoLevel")) > 1)
            {
                return Security.Crypto.Encrypt(Name);
            }

            return Name;
        }

        public static string EncryptColumnValue(string Value)
        {
            if (Value == null)
                return "";

            if (Value == "")
                return "";

            Value = Value.Replace("'", "''");

            if (Convert.ToInt32(Configuration.GetConfiguration("CryptoLevel")) > 0)
            {
                return Security.Crypto.Encrypt(Value);
            }

            return Value;
        }

        public static string EncryptTableName(string Name)
        {
            if (Convert.ToInt32(Configuration.GetConfiguration("CryptoLevel")) > 2)
            {
                return Security.Crypto.Encrypt(Name);
            }

            return Name;
        }

        public static string DecryptColumnName(string Name)
        {
            if (Convert.ToInt32(Configuration.GetConfiguration("CryptoLevel")) > 1)
            {
                return Security.Crypto.Decrypt(Name);
            }

            return Name;
        }

        public static string DecryptColumnValue(string Value)
        {
            if (Value == "")
                return "";

            if (Convert.ToInt32(Configuration.GetConfiguration("CryptoLevel")) > 0)
            {
                return Security.Crypto.Decrypt(Value);
            }

            return Value;
        }

        public static string DecryptTableName(string Name)
        {
            if (Convert.ToInt32(Configuration.GetConfiguration("CryptoLevel")) > 2)
            {
                return Security.Crypto.Decrypt(Name);
            }

            return Name;
        }

        public static System.Reflection.PropertyInfo GetPropertyInfo(System.Type SystemType, string Name)
        {
            if (SystemType == null)
                return null;

            foreach (System.Reflection.PropertyInfo SystemAttribute in SystemType.GetProperties())
            {
                if (SystemAttribute.Name == Name)
                {
                    return SystemAttribute;
                }
            }

            return null;
        }

        public static bool IsCryptable(System.Reflection.PropertyInfo SystemAttribute)
        {
            if (SystemAttribute == null)
                return false;

            if (SystemAttribute.PropertyType == typeof(int))
            {
                return false;
            }
            if (SystemAttribute.PropertyType == typeof(double))
            {
                return false;
            }
            if (SystemAttribute.PropertyType == typeof(decimal))
            {
                return false;
            }
            if (SystemAttribute.PropertyType == typeof(DateTime))
            {
                return false;
            }
            if (SystemAttribute.PropertyType == typeof(bool))
            {
                return false;
            }
            if (SystemAttribute.PropertyType == typeof(Guid))
            {
                return false;
            }

            if (Attribute.IsDefined(SystemAttribute, typeof(UncryptedAttribute)))
            {
                return false;
            }

            return true;
        }
    }
}
