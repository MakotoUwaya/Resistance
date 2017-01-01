using System;
using System.Collections.Generic;

namespace Resistance.Core
{
    public class Player : IEquatable<Player>
    {
        public string ConnectionId { get; }
        public string Name { get; }
        public string ImagePath { get; set; }
        public PlayerRole Role { get; private set; }
        public List<PlotCard> PossesionCards { get; set; }

        public Player( string connectionId, string name ) : this(connectionId, name, "")
        {
        }

        public Player(string connectionId, string name, string imagePath)
        {
            this.ConnectionId = connectionId;
            this.Name = name;
            this.ImagePath = ImagePath;
            this.Role = PlayerRole.None;
            this.PossesionCards = new List<PlotCard>();
        }

        public void SetRole(PlayerRole role)
        {
            this.Role = role;
        }

        public bool Equals(Player other)
        {
            return this.ConnectionId == other.ConnectionId && this.Name == other.Name;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
