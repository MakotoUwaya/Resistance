using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

using Resistance.Core;
using Resistance.Data;
using Resistance.Data.Models;
using System.Threading.Tasks;

[HubName("gameHub")]
public class GameHub : Hub
{
    private const int offset = 16;
    private const int cardsize = 120;

    public Room CurrentRoom { get; private set; }
    public Game CurrentGame { get; private set; }

    //private bool IsSetLeader;
    //private bool IsMissionStart;

    //public GameHub()
    //{
    //}

    //public void SetLeader()
    //{
    //    if (!this.IsSetLeader)
    //    {
    //        this.IsSetLeader = true;
    //        if (GameStatus.Info.JudgeConclusion)
    //        {
    //            Clients.All.Gameover($"レジスタンス：{GameStatus.Info.ResistanceWin} vs スパイ：{GameStatus.Info.SpyWin}\n{(GameStatus.Info.SpyWin < GameStatus.Info.ResistanceWin ? "レジスタンス" : "スパイ")}側の勝利です！！");
    //            GameStatus.Info = null;
    //            RoomInfo.Primary.IsStarted = false;
    //            for (int i = RoomInfo.Primary.PlayerList.Count - 1; i >= 0; i--)
    //            {
    //                RoomInfo.Primary.RemovePlayer(RoomInfo.Primary.PlayerList[i]);
    //            }
    //            return;
    //        }

    //        Clients.All.SetLeader(GameStatus.Info.CurrentLeader.Name, GameStatus.Info.SelectPlayerCount);
    //        this.IsMissionStart = false;
    //    }      
    //}

    //public void UpdateSelectStatus(string elementName, string color)
    //{
    //    Clients.All.UpdateSelectStatus(elementName, color);
    //}

    //public void StartVote(string playerName)
    //{
    //    // リーダー設定送信の重複送信防止機構を解除する
    //    this.IsSetLeader = false;

    //    var player = RoomInfo.Primary.PlayerList.Where(m => m.Name == playerName).SingleOrDefault();
    //    if (player != null)
    //    {
    //        if (GameStatus.Info.AddSelectPlayer(player))
    //        {
    //            Clients.AllExcept(Context.ConnectionId).StartVote();
    //        }
    //    }
    //}

    //public void Vote(bool ok)
    //{
    //    var index = GameStatus.Info.Members.FindIndex(m => m.Name == Context.User.Identity.Name);
    //    GameStatus.Info.Members[index].CurrentVote = ok;

    //    Clients.All.VoteUpdate($"player{index + 1}", ok ? "true" : "false");
    //    if (GameStatus.Info.IsCompleteVote())
    //    {
    //        var voteResult = GameStatus.Info.Vote(GameStatus.Info.Members.Where(m => m.CurrentVote.Value).Count());
    //        Clients.All.VoteComplete(voteResult);
    //    }
    //}

    //public void Revote()
    //{
    //    GameStatus.Info.IncrementRevoteCount();
    //    if (GameStatus.Info.IsOverRevote)
    //    {
    //        if (!GameStatus.Info.TryMission(GameStatus.Info.RevoteCount))
    //        {
    //            Clients.All.SpyWins($"Resistance:{GameStatus.Info.ResistanceWin} vs Spy:{GameStatus.Info.SpyWin}");
    //        }
    //    }
    //    else
    //    {
    //        GameStatus.Info.ForwardLeader();
    //        this.SetLeader();
    //    }
    //}

    //public void MissionStart()
    //{
    //    if (!this.IsMissionStart)
    //    {
    //        // TODO: ミッションが終わったらフラグを解除する
    //        this.IsMissionStart = true;
    //        Clients.All.MissionStart(GameStatus.Info.CurrentSelectPlayers);
    //    }
    //}

    //public void MissionUpdate(bool success)
    //{
    //    GameStatus.Info.CurrentSelectPlayers.Where(m => m.Name == Context.User.Identity.Name).Single().CurrentMission = success;
    //    var index = GameStatus.Info.Members.FindIndex(m => m.Name == Context.User.Identity.Name);
    //    Clients.All.MissionUpdate($"player{index + 1}");

    //    if (GameStatus.Info.CurrentSelectPlayers.Count == GameStatus.Info.CurrentSelectPlayers.Where(m => m.CurrentMission.HasValue).Count())
    //    {
    //        if (GameStatus.Info.TryMission(GameStatus.Info.CurrentSelectPlayers.Where(m => m.CurrentMission.Value == false).Count()))
    //        {
    //            Clients.All.ResistanceWins($"Resistance:{GameStatus.Info.ResistanceWin} vs Spy:{GameStatus.Info.SpyWin}");
    //        }
    //        else
    //        {
    //            Clients.All.SpyWins($"Resistance:{GameStatus.Info.ResistanceWin} vs Spy:{GameStatus.Info.SpyWin}");
    //        }
    //    }
    //}

    #region Inititialize
    public void PlayerInitialization(int width, int height)
    {
        Groups.Add(Context.ConnectionId, Context.QueryString["room"]);
        this.CurrentRoom = RoomInfo.Get(Context.QueryString["room"]);
        this.CurrentGame = this.CurrentRoom.RoomGame;        

        var name = Context.User.Identity.Name;
        var positionList = new List<ShapeModel>();

        var memberList = this.CurrentRoom.PlayerList.ToList();
        var moveList = new List<Player>();
        for (int i = 0; i < memberList.Count; i++)
        {
            if (memberList[i].Name == name)
            {
                break;
            }
            else
            {
                moveList.Add(memberList[i]);
            }

            //if (memberList[i].CurrentVote.HasValue)
            //{
            //    Clients.All.VoteUpdate($"player{i + 1}", memberList[i].CurrentVote.Value ? "true" : "false");
            //}
        }

        for (int i = 0; i < moveList.Count; i++)
        {
            memberList.Remove(moveList[i]);
        }
        memberList.AddRange(moveList);

        width -= offset;
        height -= offset;

        switch (this.CurrentRoom.MemberCount)
        {
            case 5:
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.5) - (cardsize * 0.5), Top = height - cardsize }, GetIndex(memberList[0]), memberList[0].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = width - cardsize, Top = (height * 0.5) - (cardsize * 0.5) }, GetIndex(memberList[1]), memberList[1].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.7) - (cardsize * 0.5), Top = offset }, GetIndex(memberList[2]), memberList[2].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.3) - (cardsize * 0.5), Top = offset }, GetIndex(memberList[3]), memberList[3].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = offset, Top = (height * 0.5) - (cardsize * 0.5) }, GetIndex(memberList[4]), memberList[4].Name);
                break;

            case 6:
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.5) - (cardsize * 0.5), Top = height - cardsize }, GetIndex(memberList[0]), memberList[0].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = width - cardsize, Top = (height * 0.7) - (cardsize * 0.5) }, GetIndex(memberList[1]), memberList[1].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = width - cardsize, Top = (height * 0.3) - (cardsize * 0.5) }, GetIndex(memberList[2]), memberList[2].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.5) - (cardsize * 0.5), Top = offset }, GetIndex(memberList[3]), memberList[3].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = offset, Top = (height * 0.3) - (cardsize * 0.5) }, GetIndex(memberList[4]), memberList[4].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = offset, Top = (height * 0.7) - (cardsize * 0.5) }, GetIndex(memberList[5]), memberList[5].Name);
                break;

            case 7:
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.5) - (cardsize * 0.5), Top = height - cardsize }, GetIndex(memberList[0]), memberList[0].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.7) - (cardsize * 0.5), Top = (height * 0.7) - (cardsize * 0.5) }, GetIndex(memberList[1]), memberList[1].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = width - cardsize, Top = (height * 0.4) - (cardsize * 0.5) }, GetIndex(memberList[2]), memberList[2].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.7) - (cardsize * 0.5), Top = offset }, GetIndex(memberList[3]), memberList[3].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.3) - (cardsize * 0.5), Top = offset }, GetIndex(memberList[4]), memberList[4].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = offset, Top = (height * 0.4) - (cardsize * 0.5) }, GetIndex(memberList[5]), memberList[5].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.3) - (cardsize * 0.5), Top = (height * 0.7) - (cardsize * 0.5) }, GetIndex(memberList[6]), memberList[6].Name);
                break;

            case 8:
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.5) - (cardsize * 0.5), Top = height - cardsize }, GetIndex(memberList[0]), memberList[0].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.7) - (cardsize * 0.5), Top = (height * 0.7) - (cardsize * 0.5) }, GetIndex(memberList[1]), memberList[1].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = width - cardsize, Top = (height * 0.5) - (cardsize * 0.5) }, GetIndex(memberList[2]), memberList[2].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.7) - (cardsize * 0.5), Top = (height * 0.3) - (cardsize * 0.5) }, GetIndex(memberList[3]), memberList[3].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.5) - (cardsize * 0.5), Top = offset }, GetIndex(memberList[4]), memberList[4].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.3) - (cardsize * 0.5), Top = (height * 0.3) - (cardsize * 0.5) }, GetIndex(memberList[5]), memberList[5].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = offset, Top = (height * 0.5) - (cardsize * 0.5) }, GetIndex(memberList[6]), memberList[6].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.3) - (cardsize * 0.5), Top = (height * 0.7) - (cardsize * 0.5) }, GetIndex(memberList[7]), memberList[7].Name);
                break;

            case 9:
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.5) - (cardsize * 0.5), Top = height - cardsize }, GetIndex(memberList[0]), memberList[0].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.75) - (cardsize * 0.5), Top = (height * 0.9) - (cardsize * 0.5) }, GetIndex(memberList[1]), memberList[1].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = width - cardsize, Top = (height * 0.7) - (cardsize * 0.5) }, GetIndex(memberList[2]), memberList[2].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = width - cardsize, Top = (height * 0.3) - (cardsize * 0.5) }, GetIndex(memberList[3]), memberList[3].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.75) - (cardsize * 0.5), Top = offset }, GetIndex(memberList[4]), memberList[4].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.25) - (cardsize * 0.5), Top = offset }, GetIndex(memberList[5]), memberList[5].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = offset, Top = (height * 0.3) - (cardsize * 0.5) }, GetIndex(memberList[6]), memberList[6].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = offset, Top = (height * 0.7) - (cardsize * 0.5) }, GetIndex(memberList[7]), memberList[7].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.25) - (cardsize * 0.5), Top = (height * 0.9) - (cardsize * 0.5) }, GetIndex(memberList[8]), memberList[8].Name);
                break;

            case 10:
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.5) - (cardsize * 0.5), Top = height - cardsize }, GetIndex(memberList[0]), memberList[0].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.75) - (cardsize * 0.5), Top = (height * 0.9) - (cardsize * 0.5) }, GetIndex(memberList[1]), memberList[1].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = width - cardsize, Top = (height * 0.7) - (cardsize * 0.5) }, GetIndex(memberList[2]), memberList[2].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = width - cardsize, Top = (height * 0.3) - (cardsize * 0.5) }, GetIndex(memberList[3]), memberList[3].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.75) - (cardsize * 0.5), Top = (height * 0.1) - (cardsize * 0.5) }, GetIndex(memberList[4]), memberList[4].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.5) - (cardsize * 0.5), Top = offset }, GetIndex(memberList[5]), memberList[5].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.25) - (cardsize * 0.5), Top = (height * 0.1) - (cardsize * 0.5) }, GetIndex(memberList[6]), memberList[6].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = offset, Top = (height * 0.3) - (cardsize * 0.5) }, GetIndex(memberList[7]), memberList[7].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = offset, Top = (height * 0.7) - (cardsize * 0.5) }, GetIndex(memberList[8]), memberList[8].Name);
                Clients.Caller.PositionInitialization(new ShapeModel()
                { Left = (width * 0.25) - (cardsize * 0.5), Top = (height * 0.9) - (cardsize * 0.5) }, GetIndex(memberList[9]), memberList[9].Name);
                break;

            default:
                break;
        }

        // 役割確認
        // スパイの場合はスパイのプレイヤー名を赤色で3秒間表示
        this.ShowPlayerRole();
    }

    public void ShowPlayerRole()
    {
        Player myself = this.CurrentRoom.PlayerList.Where(m => m.Name == Context.User.Identity.Name).SingleOrDefault();
        if (myself != null && myself.Role == PlayerRole.Spy)
        {
            Clients.Caller.ShowPlayerRole(this.CurrentRoom.PlayerList.Where(p => p.Role == PlayerRole.Spy).ToArray());
        }
        else
        {
            Clients.Caller.ShowPlayerRole(new List<Player>().ToArray());
        }
    }

    private string GetIndex(Player player)
    {
        return $"player{this.CurrentRoom.PlayerList.IndexOf(player) + 1}";
    }
    #endregion

    public override Task OnConnected()
    {
        var player = new Player(Context.ConnectionId, Context.User.Identity.Name);
        if (!ClientsInfo.List.Contains(player))
        {
            ClientsInfo.List.Add(player);
        }
        return null;
    }
}
