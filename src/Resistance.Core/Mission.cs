using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resistance.Core
{
    public class Mission
    {
        public List<Player> Member { get; }
        public List<MissionResult> Result { get; }
        public bool? IsSuccess { get; private set; }
        public bool IsConclusion
        {
            get
            {
                return this.Member.Count == this.Result.Count;
            }
        }

        public Mission(List<Player> missionMember)
        {
            this.Member = missionMember;
            this.Result = new List<MissionResult>();
        }

        public void SetBehavior( Player player, bool behavior)
        {
            if (this.Member.Contains(player))
            {
                var result = this.Result.Where(r => r.TargetPlayer == player).SingleOrDefault();
                if (result == null)
                {
                    this.Result.Add(new MissionResult(player, behavior));
                }
                else
                {
                    result.IsTrue = behavior;
                }
            }
        }

        public void CarryOut( int memberCount, int phaseIndex)
        {
            if (this.IsConclusion)
            {
                this.IsSuccess = Rule.ResultDetection(memberCount, phaseIndex,
                            this.Result.Where(r => r.IsTrue.HasValue && !r.IsTrue.Value).Count());
            }
            else
            {
                this.IsSuccess = null;
            }       
        }
    }
}
