using Reflect.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Reflect
{
    public class ReflectEntity
    {
        public int ID { get; set; }
        [NoRelation]
        public bool IsDeleted { get; set; }
        [NoRelation]
        public DateTime CreatedOn { get; set; }
        [NoRelation]
        public DateTime UpdatedOn { get; set; }

        public ReflectEntity()
        {

        }

        public ReflectEntity(int ObjectID)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>();
            Parameters.Add("ID", ObjectID.ToString());
            object Instance = Engine.GetSingleByParameters(this.GetType(), Parameters);
            foreach (System.Reflection.PropertyInfo SystemAttribute in this.GetType().GetProperties())
            {
                try
                {
                    SystemAttribute.SetValue(this, Instance.GetType().GetProperty(SystemAttribute.Name).GetValue(Instance));
                }
                catch (Exception)
                {

                }
            }
        }

        public void Synchronise()
        {
            ID = Engine.GetUniqueID(this);
            ID = Engine.Save(this);
        }

        public void Save()
        {
            ID = Engine.Save(this);
        }

        public void Delete()
        {
            Engine.Delete(this);
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static object ParseJSON(string JSON)
        {
            return JsonConvert.DeserializeObject(JSON);
        }
    }
}
