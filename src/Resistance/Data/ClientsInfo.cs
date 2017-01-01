using System;
using System.Collections.Generic;
using System.Security.Principal;
using Resistance.Core;

namespace Resistance.Data
{
    public static class ClientsInfo
    {
        public static List<Player> List { get; }

        static ClientsInfo()
        {
            List = new List<Player>();
        }
    }
}