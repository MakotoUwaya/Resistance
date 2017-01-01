using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resistance.Core
{
    public class RoomExpction : Exception
    {
        public RoomExpction()
        {
        }

        public RoomExpction(string message) : base(message)
        {
        }

        public RoomExpction(string message, Exception inner) : base(message, inner)
        {
        }

    }
}
