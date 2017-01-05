﻿using System;
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

    /// <summary>
    /// 部屋リスト
    /// </summary>
    public static Dictionary<string, Room> RoomList { get; private set; }

    /// <summary>
    /// ゲームリスト
    /// </summary>
    public static Dictionary<string,Game> GameList { get; private set; }

    /// <summary>
    /// リーダー更新確認カウンタ
    /// </summary>
    private static Dictionary<string, List<string>> SetLeaderCaller;

    /// <summary>
    /// 再投票確認カウンタ
    /// </summary>
    private static Dictionary<string, List<string>> ReVoteCaller;

    /// <summary>
    /// 選択更新履歴
    /// </summary>
    private static List<string> UpdateSelectHIstory;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    static GameHub()
    {       
        RoomList = new Dictionary<string, Room>();
        GameList = new Dictionary<string, Game>();
        SetLeaderCaller = new Dictionary<string, List<string>>();
        ReVoteCaller = new Dictionary<string, List<string>>();
        UpdateSelectHIstory = new List<string>();
    }

    /// <summary>
    /// 次フェーズのリーダー設定
    /// 兼 ゲーム終了判定
    /// </summary>
    public void SetLeader()
    {
        var roomName = Context.QueryString["room"];
        var game = GameList[roomName];

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
            return;
        }

        SetLeaderCaller[roomName].Clear();
        if (game.JudgeConclusion)
        {
            // ゲーム終了の判定はここではないはず
            Clients.Group(roomName).Gameover($"レジスタンス：{game.ResistanceWin} vs スパイ：{game.SpyWin}\n{(game.SpyWin < game.ResistanceWin ? "レジスタンス" : "スパイ")}側の勝利です！！");
            game = new Game(RoomList[roomName].PlayerList);
            SetLeaderCaller.Remove(roomName);
            return;
        }

        this.LeaderUpdate(roomName);
    }

    /// <summary>
    /// リーダーの変更ダイアログを表示
    /// </summary>
    /// <param name="roomName"></param>
    private void LeaderUpdate(string roomName)
    {
        var game = GameList[roomName];
        Clients.Caller.SetLeader(game.GamePhase[game.CurrentPhaseIndex].CurrentLeader.Name,
                        Rule.SelectMemberCount(RoomList[roomName].MemberCount, game.CurrentPhaseIndex + 1));
    }

    /// <summary>
    /// プレイヤーの選択状態を更新
    /// </summary>
    /// <param name="elementName"></param>
    /// <param name="color"></param>
    public void UpdateSelectStatus(string elementName, bool selected)
    {
        var roomName = Context.QueryString["room"];
        var gameIndex = GameList[roomName].CurrentPhaseIndex;
        var phase = GameList[roomName].GamePhase[gameIndex];
        var playerIndex = int.Parse(elementName.Last().ToString());

        Clients.Group(roomName).UpdateSelectStatus(elementName, selected);
        if (selected)
        {
            UpdateSelectHIstory.Add(elementName);
            phase.AddMissionMember(GameList[roomName].PlayerList[playerIndex]);
        }
        else
        {
            if (UpdateSelectHIstory.Contains(elementName))
            {
                UpdateSelectHIstory.Remove(elementName);
            }
            phase.RemoveMissionMember(GameList[roomName].PlayerList[playerIndex]);
        }
    }

    public void StartVote()
    {
        var roomName = Context.QueryString["room"];
        var gameIndex = GameList[roomName].CurrentPhaseIndex;
        var phase = GameList[roomName].GamePhase[gameIndex];

        if (phase.IsMissionMemberFull)
        {
            // 選択状態を再送信
            foreach (var elementName in UpdateSelectHIstory)
            {
                Clients.Group(roomName).UpdateSelectStatus(elementName, true);
            }

            // 投票用のインスタンスを生成
            if (phase.PhaseVote.Count() <= phase.CurrentVoteIndex)
            {
                phase.PhaseVote.Add(new Vote(phase.PlayerList, phase.MissionMember));
            }
                  
            var opinionLeaders = phase.PlayerList.Where(p => p.IsOpinionLeader).Select(p => p.ConnectionId).ToArray();           
            if (opinionLeaders.Length > 0)
            {
                // TODO: カード効果のある人は先に信任を表明しないといけない
                Clients.Users(opinionLeaders).PrecedenceVote(phase.PlayerList.Where(p => p.IsOpinionLeader).ToArray());
                Clients.Group(roomName, opinionLeaders).StartVote(phase.PlayerList.Where(p => !p.IsOpinionLeader).ToArray());
            }
            else
            {
                Clients.Group(roomName).StartVote(phase.MissionMember);
            }
            
        }
    }

    public void PlayerVote()
    {
        var roomName = Context.QueryString["room"];
        var gameIndex = GameList[roomName].CurrentPhaseIndex;
        var phase = GameList[roomName].GamePhase[gameIndex];

        if (phase.IsMissionMemberFull && phase.JodgeVote)
        {
            Clients.Caller.StartVote(phase.MissionMember);
        }
    }

    public void SendVote(bool ok)
    {
        var roomName = Context.QueryString["room"];
        var gameIndex = GameList[roomName].CurrentPhaseIndex;
        var phase = GameList[roomName].GamePhase[gameIndex];
        var vote = phase.PhaseVote[phase.CurrentVoteIndex];

        var player = vote.PlayerList.Where(p => p.Name == Context.User.Identity.Name).SingleOrDefault();
        var index = vote.PlayerList.IndexOf(player);
        vote.Set(player, ok);

        Clients.Group(roomName).VoteUpdate(index);
        if (vote.IsConclusion)
        {
            // 投票結果の集計
            vote.Count(vote.PlayerList.Count(), gameIndex);
            Clients.Group(roomName).VoteComplete(vote.IsApprove);
        }
    }

    private void ReVote()
    {
        var roomName = Context.QueryString["room"];
        var gameIndex = GameList[roomName].CurrentPhaseIndex;
        var phase = GameList[roomName].GamePhase[gameIndex];
        var vote = phase.PhaseVote[phase.CurrentVoteIndex];

        if (!ReVoteCaller.ContainsKey(roomName))
        {
            ReVoteCaller[roomName] = new List<string>();
        }

        if (!ReVoteCaller[roomName].Contains(Context.User.Identity.Name))
        {
            ReVoteCaller[roomName].Add(Context.User.Identity.Name);
        }

        if (ReVoteCaller[roomName].Count < RoomList[roomName].MemberCount)
        {
            return;
        }

        ReVoteCaller[roomName].Clear();        
        phase.CurrentVoteIndex++;
        if (Rule.MaxVoteCount <= phase.CurrentVoteIndex)
        {
            phase.PhaseMission.OverReject();
            Clients.Group(roomName).SpyWins();
        }

        phase.NextLeader();
    }

    public void MissionStart()
    {
        var roomName = Context.QueryString["room"];
        var gameIndex = GameList[roomName].CurrentPhaseIndex;
        var mission = GameList[roomName].GamePhase[gameIndex].PhaseMission;

        Clients.Group(roomName).MissionStart(mission.Member.ToArray());
    }

    public void MissionUpdate(bool success)
    {
        GameStatus.Info.CurrentSelectPlayers.Where(m => m.Name == Context.User.Identity.Name).Single().CurrentMission = success;
        var index = GameStatus.Info.Members.FindIndex(m => m.Name == Context.User.Identity.Name);
        Clients.All.MissionUpdate($"player{index + 1}");

        if (GameStatus.Info.CurrentSelectPlayers.Count == GameStatus.Info.CurrentSelectPlayers.Where(m => m.CurrentMission.HasValue).Count())
        {
            if (GameStatus.Info.TryMission(GameStatus.Info.CurrentSelectPlayers.Where(m => m.CurrentMission.Value == false).Count()))
            {
                Clients.All.ResistanceWins($"Resistance:{GameStatus.Info.ResistanceWin} vs Spy:{GameStatus.Info.SpyWin}");
            }
            else
            {
                Clients.All.SpyWins($"Resistance:{GameStatus.Info.ResistanceWin} vs Spy:{GameStatus.Info.SpyWin}");
            }
        }
    }

    #region Inititialize
    /// <summary>
    /// ゲームの初期化
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void Initialization()
    {
        // ゲームを初期化する処理はここにまとめる
        var roomName = Context.QueryString["room"];

        // プレイヤーの生成
        Clients.Caller.CreatePlayer(RoomList[roomName].PlayerList);

        // ゲームステータスの復元
        this.StartVote();

        // プレイヤーの役割ダイアログを表示する
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

    /// <summary>
    /// プレイヤーの通し番号を取得する
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private int GetIndex(Player player)
    {
        var roomName = Context.QueryString["room"];
        return RoomList[roomName].PlayerList.IndexOf(player);
    }
    #endregion

    #region SignalR Connection Method
    /// <summary>
    /// SignalR 接続確立時に行う処理
    /// </summary>
    /// <returns></returns>
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
    #endregion

}
