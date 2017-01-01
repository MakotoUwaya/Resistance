using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resistance.Core
{
    public abstract class Result
    {
        public Player TargetPlayer { get; }
        public bool? IsTrue { get; set; }

        public Result(Player player, bool isTrue)
        {
            this.TargetPlayer = player;
            this.IsTrue = isTrue;
        }
    }
}
