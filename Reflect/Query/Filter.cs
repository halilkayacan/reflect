using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflect.Query
{
    public class Filter
    {
        public string Column { get; set; }
        public string Value { get; set; }
        public string Query { get; set; }
        public FilterCombine Combine { get; set; }
        public FilterCondition Condition { get; set; }
    }
}
