using System;
using System.Collections.Generic;

namespace Resistance.Core
{
    public class PlotCard : IEquatable<PlotCard>
    {
        public string Name { get; }
        public string Description { get; }
        public string ImagePath { get; }
        public CardExecuteTiming ExecuteTiming { get; }
        public bool IsOnce { get; set; }
        public bool IsUsed { get; set; }
        public Action<Player, Player> Effect { get; set; }

        public PlotCard(string name, string description, string imagePath, CardExecuteTiming timing, bool isOnce, Action<Player,Player> effect)
        {
            this.Name = name;
            this.Description = description;
            this.ImagePath = imagePath;
            this.ExecuteTiming = timing;
            this.IsOnce = isOnce;
            this.Effect = effect;
        }

        public bool Equals(PlotCard other)
        {
            return this.Name == other.Name;
        }

        public override string ToString()
        {
            return this.Name;
        }


    }
}
