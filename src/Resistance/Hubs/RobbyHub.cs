using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Resistance.Core;
using Resistance.Data;
using Resistance.Helper;

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
        Clients.Caller.Result(false);
        foreach (var room in RoomInfo.List)
        {
            Clients.Caller.StatusUpdate(room.MemberCount, room.CanStart);
        }        
    }

    public void UpdateMessage(string name, string message)
    {
        try
        {
            var room = RoomInfo.Get(this.RoomName);
            Clients.Group(this.RoomName).Received(name.ReEncode(), message.ReEncode(), room.MemberCount, room.CanStart);
        }
        catch (Exception ex)
        {
            Clients.Group(this.RoomName).Received(name.ReEncode(), ex.Message.ReEncode(), 0, false);
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

    public void GameStart()
    {
        try
        {
            var room = RoomInfo.Get(this.RoomName);
            if (room.CanStart)
            {
                if (!room.RoomGame.IsActive)
                {
                    Clients.Group(this.RoomName).GameStart();
                }
                else
                {
                    Clients.Caller.GameStart();
                }
            }            
        }
        catch (Exception ex)
        {
            Clients.Group(this.RoomName).Buzz("システム".ReEncode(), ex.Message.ReEncode());
        }      
    }

    public override Task OnConnected()
    {
        Groups.Add(Context.ConnectionId, Context.User.Identity.GetHashCode().ToString());
        return Groups.Add(Context.ConnectionId, Context.QueryString["room"]);
    }
}

