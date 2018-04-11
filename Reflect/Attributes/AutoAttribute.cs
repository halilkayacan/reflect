using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflect.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class AutoAttribute : System.Attribute
    {
        public AutoAttribute(int Seed)
        {
            this.Seed = Seed;
        }

        public int Seed { get; set; }
    }
}
