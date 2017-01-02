using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using Resistance.Core;

namespace Resistance.Models.Game
{
    public class GameViewModel
    {
        [Required]
        [Display(Name = "ルーム名")]
        public string RoomName { get; set; }

        [Required]
        [Display(Name = "プレイヤー名")]
        public string PlayerName { get; set; }
    }
}