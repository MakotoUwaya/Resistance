using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Resistance.Core;
using Resistance.Data;
using Resistance.Helper;
using System.Linq;
using System.Web;

[HubName("robby")]
public class RobbyHub : Hub
{
    private string RoomName
    {
        get
        {
            return Context.QueryString["room"];
        }
    }

    public void WindowLoaded()
    {        
        foreach (var room in RoomInfo.List)
        {
            Clients.Caller.RoomUpdate(room.Name, RoomInfo.Get(room.Name).MemberCount, Context.User.Identity.Name, "");
        }        
    }

    public void Message(string name, string text, bool isAll)
    {
        if (isAll)
        {
            Clients.All.Buzz(name.ReEncode(), text.ReEncode());
        }
        else
        {
            Clients.Group(this.RoomName).Buzz(name.ReEncode(), text.ReEncode());
        }     
    }

    public void Create(string roomName)
    {
        if (RoomInfo.List.SelectMany(l => l.PlayerList).Where(p => p.Name == Context.User.Identity.Name).Count() > 0)
        {
            Clients.Caller.Buzz(Context.User.Identity.Name, "1つ以上のルームに参加することはできません。退出後に再実行してください。");
            return;
        }

        if (string.IsNullOrWhiteSpace(roomName))
        {
            Clients.Caller.Buzz(Context.User.Identity.Name, "このルーム名は指定できません。別の名前を入力してください。");
            return;
        }

        if (RoomInfo.List.Where(l => l.Name == roomName).Count() != 0)
        {
            Clients.Caller.Buzz(Context.User.Identity.Name, "このルーム名は既に登録されています。");
            return;
        }

        var player = ClientsInfo.List.Where(l => l.ConnectionId == Context.ConnectionId).SingleOrDefault();
        if (player == null)
        {
            player = new Player(Context.ConnectionId, Context.User.Identity.Name);
            ClientsInfo.List.Add(player);
        }

        RoomInfo.AddRoom(roomName, player);
        Groups.Add(player.ConnectionId, roomName);
        Clients.All.RoomUpdate(roomName, RoomInfo.Get(roomName).MemberCount, player.Name, $"「{roomName}」を作成しました。");
        Clients.Group(roomName).Buzz(player.Name, $"「{roomName}」に参加しました。");
    }


    public void Join(string roomName)
    {
        try
        {
            var room = RoomInfo.List.Where(l => l.Name == roomName).SingleOrDefault();

            if (room != null)
            {
                if (room.RoomGame.IsActive)
                {
                    Clients.Caller.Buzz(Context.User.Identity.Name, $"「{roomName}」はゲーム進行中です。");
                    return;
                }

                var player = ClientsInfo.List.Where(l => l.ConnectionId == Context.ConnectionId).SingleOrDefault();
                if (player == null)
                {
                    player = new Player(Context.ConnectionId, Context.User.Identity.Name);
                    ClientsInfo.List.Add(player);
                }

                room.AddPlayer(player);
                Groups.Add(player.ConnectionId, roomName);
                Clients.All.RoomUpdate(roomName, RoomInfo.Get(roomName).MemberCount, player.Name, $"「{roomName}」に参加しました。");
            }
            else
            {
                Clients.Caller.Buzz(Context.User.Identity.Name, $"「{roomName}」は存在しません。");
            }
        }
        catch (RoomExpction ex)
        {
            Clients.Caller.Buzz(Context.User.Identity.Name, ex.Message);
        }
    }

    public void Withdraw(string roomName)
    {
        try
        {
            var room = RoomInfo.List.Where(l => l.Name == roomName).SingleOrDefault();

            if (room != null)
            {
                var player = ClientsInfo.List.Where(l => l.ConnectionId == Context.ConnectionId).SingleOrDefault();
                if (player == null)
                {
                    player = new Player("", Context.User.Identity.Name);
                    ClientsInfo.List.Add(player);
                }

                room.RemovePlayer(player);
                
                if (RoomInfo.Get(roomName).MemberCount != 0)
                {
                    Clients.All.RoomUpdate(roomName, RoomInfo.Get(roomName).MemberCount, player.Name, $"「{roomName}」から退出しました。");
                }
                else
                {
                    RoomInfo.List.Remove(room);
                    Clients.All.RemoveRoom(roomName);
                    Clients.All.Buzz(player.Name, $"メンバーがいなくなったため、「{roomName}」を削除しました。");
                }

                Groups.Remove(player.ConnectionId, roomName);
            }
            else
            {
                Clients.Caller.Buzz(Context.User.Identity.Name, $"「{roomName}」は存在しません。");
            }
        }
        catch (RoomExpction ex)
        {
            Clients.Caller.Buzz(Context.User.Identity.Name, ex.Message);
        }
    }

    public void Dialogupdate(string roomName)
    {
        try
        {
            var room = RoomInfo.List.Where(l => l.Name == roomName).SingleOrDefault();

            if (room != null)
            {
                Clients.Caller.Dialogupdate(room.Name, room.PlayerList.ToArray(), 
                    room.CanStart && room.PlayerList.Where(p => p.Name == Context.User.Identity.Name).Count() > 0);
            }
            else
            {
                Clients.Caller.Buzz(Context.User.Identity.Name, $"「{roomName}」は存在しません。");
            }
        }
        catch (RoomExpction ex)
        {
            Clients.Caller.Buzz(Context.User.Identity.Name, ex.Message);
        }
    }

    public void GameStart(string roomname)
    {
        try
        {
            var room = RoomInfo.Get(roomname);
            if (room.CanStart)
            {
                if (room.PlayerList.Where(p => p.Name == Context.User.Identity.Name).Count() == 0)
                {
                    Clients.Group(roomname).Buzz(Context.User.Identity.Name, "所属していないルームのゲームは開始できません。");
                    return;
                }

                if (!room.RoomGame.IsActive)
                {
                    room.RoomGame.IsActive = true;
                    room.RoomGame.SetRole();
                    Clients.Group(roomname).GameStart(roomname);
                }
                else
                {
                    Clients.Caller.GameStart(roomname);
                }
            }
            else
            {
                Clients.Group(roomname).Buzz(Context.User.Identity.Name, "メンバーが不足、または超過しています。");
            }
        }
        catch (Exception ex)
        {
            Clients.Group(roomname).Buzz(Context.User.Identity.Name, ex.Message);
        }      
    }

    public override Task OnConnected()
    {
        var player = new Player(Context.ConnectionId, Context.User.Identity.Name);
        if (!ClientsInfo.List.Contains(player))
        {
            ClientsInfo.List.Add(player);
        }        
        return Groups.Add(Context.ConnectionId, Context.QueryString["robby"]);
    }
}

