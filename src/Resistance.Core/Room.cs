using System;
using System.Collections.Generic;
using System.Linq;


namespace Resistance.Core
{
    public class Room : IEquatable<Room>
    {
        private int MinimumMemberCount;
        private int MaximumMemberCount;

        public string Name { get; set; }
        public Game RoomGame { get; }
        public List<Player> PlayerList { get; }   
        public Player[] OutputList
        {
            get
            {
                return this.PlayerList.ToArray();
            }
        }
            
        public int MemberCount
        {
            get
            {
                return this.PlayerList.Count();
            }
        }
        public bool CanStart
        {
            get
            {
                return this.MinimumMemberCount <= this.MemberCount && this.MemberCount <= this.MaximumMemberCount;
            }

        }
        public bool IsMemberFull
        {
            get
            {
                return this.MaximumMemberCount <= this.MemberCount;
            }
            
        }

        public Room(string name, Player player) : this(5, 10, name, player)
        {
        }

        public Room( int min, int max, string name, Player player )
        {
            this.MinimumMemberCount = min;
            this.MaximumMemberCount = max;
            this.Name = name;
            this.PlayerList = new List<Player>();
            this.PlayerList.Add(player);
            this.RoomGame = new Game(this.PlayerList);
        }

        public string GetElementName(Player player)
        {
            return $"player{this.PlayerList.IndexOf(player)}";
        }

        public void AddPlayer(Player player)
        {
            if (this.IsMemberFull)
            {
                throw new RoomExpction($"ルームに参加できる人数({MaximumMemberCount}人)を超えています。");
            }
            else if (this.PlayerList.Where(m => m.Name == player.Name).Count() > 0)
            {
                throw new RoomExpction($"同じ名前のプレイヤーが存在します。別の名前を入力して下さい。");
            }
            else if (this.PlayerList.Contains(player))
            {
                throw new RoomExpction($"既にルーム参加しています。");
            }
            else
            {
                this.PlayerList.Add(player);                
            }
        }

        public void RemovePlayer(Player player)
        {
            if (this.PlayerList.Contains(player))
            {
                this.PlayerList.Remove(player);
            }
            else
            {
                throw new RoomExpction($"ルーム内にプレイヤーが存在しません。");
            }
        }

        public bool Equals(Room other)
        {
            return this.Name == other.Name;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
