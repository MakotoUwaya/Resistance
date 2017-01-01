using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Resistance.Core;

namespace Resistance.Data
{
    public static class RoomInfo
    {
        public static List<Room> List { get; }

        static RoomInfo()
        {
            List = new List<Room>();
        }

        public static Room Get(int index)
        {
            return List[index];
        }

        public static Room Get(string name)
        {
            var room = List.Where(r => r.Name == name).SingleOrDefault();
            if (room != null)
            {
                return room;
            }
            else
            {
                throw new RoomExpction("対象のルームは存在しません。");
            }
        }

        public static void AddRoom(string name, Player player)
        {
            var room = new Room(name, player);
            if (!List.Contains(room))
            {
                List.Add(room);
            }
            else
            {
                throw new RoomExpction("別のルーム名を指定してください。");
            }
        }

        public static void RemoveRoom(string name)
        {
            var room = List.Where(r => r.Name == name).SingleOrDefault();
            if (room != null)
            {
                List.Remove(room);
            }
            else
            {
                throw new RoomExpction("対象のルームは存在しません。");
            }
        }
    }
}