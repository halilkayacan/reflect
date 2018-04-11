using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflect.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class TableAttribute : System.Attribute
    {
        public TableAttribute(string Table)
        {
            this.Table = Table;
        }

        public string Table { get; set; }
    }
}
