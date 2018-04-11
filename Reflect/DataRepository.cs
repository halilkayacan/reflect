using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflect
{
    public class DataRepository<T>
    {
        public static T GetById(int Id)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>();
            Parameters.Add("ID", Id.ToString());
            object Instance = Engine.GetSingleByParameters(typeof(T), Parameters);
            if (Instance != null)
                return (T)Instance;
            return default(T);
        }

        public static T GetByParameter(Dictionary<string, string> Parameters)
        {
            List<T> ObjectList = GetAll(Parameters, 1, 1);
            if (ObjectList == null)
                return default(T);
            if (ObjectList.Count == 0)
                return default(T);
            return ObjectList[0];
        }

        public static int GetCountByFilters()
        {
            return GetCountByFilters(new List<Query.Filter>());
        }

        public static int GetCountByFilters(List<Query.Filter> Filters)
        {
            return Engine.GetCountByFilters(typeof(T), Filters);
        }

        public static int GetCount()
        {
            return GetCount(null);
        }

        public static int GetCount(Dictionary<string, string> Parameters)
        {
            return Engine.GetCountByParameters(typeof(T), Parameters);
        }

        public static List<T> GetAll()
        {
            return GetAll(null);
        }

        public static List<T> GetAll(Dictionary<string, string> Parameters)
        {
            return GetAll(Parameters, int.MaxValue, 1);
        }

        public static List<T> GetAll(Dictionary<string, string> Parameters, int Count, int Page)
        {
            List<T> ObjectList = new List<T>();
            foreach (object Instance in Engine.GetByParameters(typeof(T), Parameters, Count, Page))
            {
                ObjectList.Add((T)Instance);
            }
            return ObjectList;
        }

        public static T GetByFilters(List<Query.Filter> Filters)
        {
            List<T> ObjectList = GetAllByFilters(Filters, 1, 1);
            if (ObjectList == null)
                return default(T);
            if (ObjectList.Count == 0)
                return default(T);
            return ObjectList[0];
        }

        public static List<T> GetAllByFilters()
        {
            return GetAllByFilters(null);
        }

        public static List<T> GetAllByFilters(List<Query.Filter> Filters)
        {
            return GetAllByFilters(Filters, int.MaxValue, 1);
        }

        public static List<T> GetAllByFilters(List<Query.Filter> Filters, int Count, int Page)
        {
            List<T> ObjectList = new List<T>();
            foreach (object Instance in Engine.GetByFilters(typeof(T), Filters, Count, Page))
            {
                ObjectList.Add((T)Instance);
            }
            return ObjectList;
        }
    }
}
