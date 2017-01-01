using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resistance.Core
{
    public class Phase
    {
        public List<Player> PlayerList { get; private set; }
        public Player CurrentLeader { get; private set; }
        public List<Player> MissionMember { get; }
        public List<Vote> PhaseVote { get; }
        public int CurrentVoteIndex { get; private set; }
        public Mission PhaseMission { get; }

        public Phase(List<Player> playerList, Player player)
        {
            this.PlayerList = playerList;
            this.CurrentLeader = player;
            this.MissionMember = new List<Player>();
            this.CurrentVoteIndex = 0;
            this.PhaseVote = new List<Vote>(5);
            this.PhaseVote.Add(new Vote(this.PlayerList, this.MissionMember));
            this.PhaseMission = new Mission(this.MissionMember);
        }

        public void NextLeader()
        {
            int currentLeaderIndex = this.PlayerList.IndexOf(this.CurrentLeader);
            if (currentLeaderIndex + 1 < this.PlayerList.Count())
            {
                this.CurrentLeader = this.PlayerList[currentLeaderIndex + 1];
            }
            else
            {
                this.CurrentLeader = this.PlayerList[0];
            }
        }

        public void SelectLeader(Player player)
        {
            this.CurrentLeader = player;
        }

        public bool IsMissionMemberFull(int playerCount)
        {
            return this.MissionMember.Count() == Rule.SelectMemberCount(playerCount, this.CurrentVoteIndex + 1);
        }

        public void AddMissionMember(Player player)
        {
            if (this.MissionMember.Count() < Rule.SelectMemberCount(this.PlayerList.Count(), this.CurrentVoteIndex + 1))
            {
                this.MissionMember.Add(player);
            }
        }

        public void RemoveMissionMember(Player player)
        {
            if (this.MissionMember.Contains(player))
            {
                this.MissionMember.Remove(player);
            }
        }

        public bool JodgeVote()
        {
            return this.PhaseVote[this.CurrentVoteIndex] == null;
        }

    }
}
