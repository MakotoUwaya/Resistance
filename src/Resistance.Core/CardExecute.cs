using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resistance.Core
{
    public enum CardExecuteTiming
    {
        BeforeCardDrow,
        AfterCardDrow,
        BeforeSelected,
        AfterSelected,
        BeforeVote,
        AfterVote,
        BeforeMission,
        AfterMission
    }
}
