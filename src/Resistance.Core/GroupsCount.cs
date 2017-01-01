using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resistance.Core
{
    public class RoleCount
    {
        public int Resistance { get; }
        public int Spy { get; }

        public RoleCount(int res, int spy)
        {
            this.Resistance = res;
            this.Spy = spy;
        }
    }
}
