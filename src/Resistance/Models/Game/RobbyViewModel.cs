using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using Resistance.Core;

namespace Resistance.Models.Game
{
    public class RobbyViewModel
    {
        [Required]
        [Display(Name = "ルーム名")]
        public string RoomName { get; set; }
    }
}