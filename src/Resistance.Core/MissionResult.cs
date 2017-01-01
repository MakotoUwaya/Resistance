using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resistance.Core
{
    public class MissionResult : Result
    {
        public MissionResult(Player player, bool isTrue) : base(player, isTrue)
        {
        }
    }
}
