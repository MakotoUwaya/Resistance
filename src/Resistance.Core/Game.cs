using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resistance.Core
{
    public class Game
    {
        public List<Player> PlayerList { get; private set; }
        public bool IsActive { get; set; } = false;
        public int CurrentPhaseIndex { get; private set; }
        public List<Phase> GamePhase { get; private set; }
        public int ResistanceWin
        {
            get
            {
                return this.GamePhase.Where(p => p.PhaseMission.IsSuccess.HasValue && p.PhaseMission.IsSuccess.Value).Count();
            }
        }

        public int SpyWin
        {
            get
            {
                return this.GamePhase.Where(p => p.PhaseMission.IsSuccess.HasValue && !p.PhaseMission.IsSuccess.Value).Count();
            }
        }

        public Game(List<Player> playerList)
        {
            var random = new Random();

            this.PlayerList = playerList;
            this.GamePhase = new List<Phase>(5);
            this.GamePhase.Add(new Phase(this.PlayerList, this.PlayerList[random.Next(this.PlayerList.Count())]));
            this.CurrentPhaseIndex = 0;
        }

        public void SetRole()
        {
            var leftMemberCount = this.PlayerList.Where(m => m.Role == PlayerRole.None).Select(m => m).Count();
            if (leftMemberCount == 0)
            {
                return;
            }

            var memberCount = Rule.GetRoleCount(this.PlayerList.Count());
            var randam = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < memberCount.Spy; i++)
            {
                var leftMember = this.PlayerList.Where(m => m.Role == PlayerRole.None).Select(m => m).ToArray();
                leftMember[randam.Next(leftMember.Length)].SetRole(PlayerRole.Spy);
            }

            foreach (var player in this.PlayerList.Where(m => m.Role == PlayerRole.None))
            {
                player.SetRole(PlayerRole.Resistance);
            }
        }

        public bool JudgeConclusion
        {
            get
            {
                return 5 <= this.CurrentPhaseIndex || 3 <= this.ResistanceWin || 3 <= this.SpyWin;
            }
        }

        public PlayerRole GetWinRole
        {
            get
            {
                if (!JudgeConclusion) return PlayerRole.None;
                if (this.SpyWin <= this.ResistanceWin)
                {
                    return PlayerRole.Resistance;
                }
                else
                {
                    return PlayerRole.Spy;
                }
            }
        }

        public string GetElementName( Player player )
        {
            return $"player{this.PlayerList.IndexOf(player)}";
        }
    }
}
