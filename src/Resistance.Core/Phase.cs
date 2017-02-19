using System.Collections.Generic;
using System.Linq;

namespace Resistance.Core
{
    public class Phase
    {
        public int GamePhaseIndex { get; private set; }
        public List<Player> PlayerList { get; private set; }
        public Player CurrentLeader { get; private set; }
        public List<Player> MissionMember { get; }
        public List<Vote> PhaseVote { get; }
        public int CurrentPhaseIndex { get; set; }
        public Mission PhaseMission { get; }

        public Phase(List<Player> playerList, Player player, int gamePhaseIndex)
        {
            this.GamePhaseIndex = gamePhaseIndex;
            this.PlayerList = playerList;
            this.CurrentLeader = player;
            this.MissionMember = new List<Player>();
            this.CurrentPhaseIndex = 0;
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

        public bool IsMissionMemberFull
        {
            get
            {
                return this.MissionMember.Count() == Rule.SelectMemberCount(this.PlayerList.Count(), this.GamePhaseIndex + 1);
            }
        }

        public void AddMissionMember(Player player)
        {
            if (this.MissionMember.Count() < Rule.SelectMemberCount(this.PlayerList.Count(), this.GamePhaseIndex + 1))
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

        public bool JodgeVote
        {
            get
            {
                return this.PhaseVote[this.CurrentPhaseIndex].IsApprove == null;
            }
        }
    }
}
