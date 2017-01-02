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

    public static Dictionary<string, Room> RoomList { get; private set; }
    public static Dictionary<string,Game> GameList { get; private set; }

    private static Dictionary<string, List<string>> SetLeaderCaller;

    static GameHub()
    {
        RoomList = new Dictionary<string, Room>();
        GameList = new Dictionary<string, Game>();
        SetLeaderCaller = new Dictionary<string, List<string>>();
    }

    public void SetLeader()
    {
        var roomName = Context.QueryString["room"];

        if (!SetLeaderCaller.ContainsKey(roomName))
        {
            SetLeaderCaller[roomName] = new List<string>();
        }

        if (!SetLeaderCaller[roomName].Contains(Context.User.Identity.Name))
        {
            SetLeaderCaller[roomName].Add(Context.User.Identity.Name);
        }

        if (SetLeaderCaller[roomName].Count < RoomList[roomName].MemberCount)
        {
            this.LeaderUpdate(roomName);
            return;
        }

        SetLeaderCaller[roomName].Clear();
        if (GameList[roomName].JudgeConclusion)
        {
            Clients.All.Gameover($"レジスタンス：{GameList[roomName].ResistanceWin} vs スパイ：{GameList[roomName].SpyWin}\n{(GameList[roomName].SpyWin < GameList[roomName].ResistanceWin ? "レジスタンス" : "スパイ")}側の勝利です！！");
            GameList[roomName] = new Game(RoomList[roomName].PlayerList);
            SetLeaderCaller.Remove(roomName);
            return;
        }

        GameList[roomName].GamePhase[GameList[roomName].CurrentPhaseIndex].NextLeader();
        this.LeaderUpdate(roomName);
    }

    private void LeaderUpdate(string roomName)
    {
        Clients.All.SetLeader(GameList[roomName].GamePhase[GameList[roomName].CurrentPhaseIndex].CurrentLeader.Name,
                        Rule.SelectMemberCount(RoomList[roomName].MemberCount, GameList[roomName].CurrentPhaseIndex + 1));
    }

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


        this.PlayerPositionReset(width, height);
        this.ShowPlayerRole();
    }

    public void PlayerPositionReset(int width, int height)
    {
        var roomName = Context.QueryString["room"];

        //var positionList = new List<ShapeModel>();
        var memberList = RoomList[roomName].PlayerList.ToList();
        var moveList = new List<Player>();
        for (int i = 0; i < memberList.Count; i++)
        {
            if (memberList[i].Name == Context.User.Identity.Name)
            {
                break;
            }
            else
            {
                moveList.Add(memberList[i]);
            }
        }

        for (int i = 0; i < moveList.Count; i++)
        {
            memberList.Remove(moveList[i]);
        }
        memberList.AddRange(moveList);

        width -= offset;
        height -= offset;

        switch (RoomList[roomName].MemberCount)
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
    }

    public void ShowPlayerRole()
    {
        var roomName = Context.QueryString["room"];
        Player myself = RoomList[roomName].PlayerList.Where(m => m.Name == Context.User.Identity.Name).SingleOrDefault();
        if (myself != null && myself.Role == PlayerRole.Spy)
        {
            Clients.Caller.ShowPlayerRole(RoomList[roomName].PlayerList.Where(p => p.Role == PlayerRole.Spy).ToArray());
        }
        else
        {
            Clients.Caller.ShowPlayerRole(new List<Player>().ToArray());
        }
    }

    private string GetIndex(Player player)
    {
        var roomName = Context.QueryString["room"];
        return $"player{RoomList[roomName].PlayerList.IndexOf(player) + 1}";
    }
    #endregion

    public override Task OnConnected()
    {
        var player = new Player(Context.ConnectionId, Context.User.Identity.Name);
        if (!ClientsInfo.List.Contains(player))
        {
            ClientsInfo.List.Add(player);
        }

        var roomName = Context.QueryString["room"];
        if (string.IsNullOrWhiteSpace(roomName))
        {
            // TODO:部屋名が指定されていない場合の処理
            return null;
        }

        Groups.Add(Context.ConnectionId, roomName);
        if (!RoomList.ContainsKey(roomName))
        {
            RoomList.Add(roomName, RoomInfo.Get(roomName));
        }

        if (!GameList.ContainsKey(roomName))
        {
            GameList.Add(roomName, RoomList[roomName].RoomGame);
        }
        return null;
    }
}
