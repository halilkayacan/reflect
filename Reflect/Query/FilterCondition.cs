using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflect.Query
{
    public enum FilterCondition
    {
        Equals,
        NotEquals,
        Higher,
        Lower,
        Like,
        NotLike,
        HigherEquals,
        LowerEquals,
        Ascending,
        Descending,
        Inner,
        Outer,
        Left
    }
}
