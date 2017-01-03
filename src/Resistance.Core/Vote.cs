using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resistance.Core
{
    public class Vote
    {
        private int MajorityCount
        {
            get
            {
                return (this.PlayerList.Count()) / 2 + (0 < (this.PlayerList.Count()) % 2 ? 1 : 0);
            }
        }

        public List<Player> PlayerList { get; private set; }
        public List<Player> MissionMember { get; }
        public List<VoteResult> Result { get; }
        public bool? IsApprove { get; private set; }
        public bool IsConclusion
        {
            get
            {
                return this.MissionMember.Count == this.Result.Count;
            }
        }

        public Vote(List<Player> playerList, List<Player> missionMember)
        {
            this.PlayerList = playerList;
            this.MissionMember = missionMember;
            this.Result = new List<VoteResult>();
        }

        public void Set( Player player, bool approve)
        {
            var result = this.Result.Where(r => r.TargetPlayer == player).SingleOrDefault();
            if (result == null)
            {
                this.Result.Add(new VoteResult(player, approve));
            }
            else
            {
                result.IsTrue = approve;
            }
        }

        public void Count( int memberCount, int phaseIndex )
        {
            if (this.IsConclusion)
            {
                this.IsApprove = this.MajorityCount <= this.Result.Where(r => r.IsTrue.HasValue && r.IsTrue.Value).Count();
            }
            else
            {
                this.IsApprove = null;
            }
        }
    }
}
