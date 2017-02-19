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
    /// ミッション開始確認カウンタ
    /// </summary>
    private static Dictionary<string, List<string>> MissionStartCaller;

    /// <summary>
    /// ミッション結果確認カウンタ
    /// </summary>
    private static Dictionary<string, List<string>> MissionReslutCaller;

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
        MissionStartCaller = new Dictionary<string, List<string>>();
        MissionReslutCaller = new Dictionary<string, List<string>>();
        UpdateSelectHIstory = new List<string>();
    }

    /// <summary>
    /// 次フェーズのリーダー設定
    /// 兼 ゲーム終了判定
    /// </summary>
    public void SetLeader()
    {
        var roomName = Context.QueryString["room"];
        if (!CallerResponceCheck(SetLeaderCaller, roomName, RoomList[roomName].MemberCount))
        {
            return;
        }

        var game = GameList[roomName];
        if (game.JudgeConclusion)
        {
            // ゲーム終了の判定はここではないはず
            Clients.Group(roomName).Gameover($"レジスタンス：{game.ResistanceWin} vs スパイ：{game.SpyWin}\n{(game.SpyWin < game.ResistanceWin ? "レジスタンス" : "スパイ")}側の勝利です！！");
            game = new Game(RoomList[roomName].PlayerList);
            SetLeaderCaller.Remove(roomName);
            return;
        }

        try
        {
            var status = new List<Mission>();
            for (int i = 0; i <= game.CurrentPhaseIndex; i++)
            {
                status.Add(game.GamePhase[i].PhaseMission);
            }

            UpdateSelectHIstory.Clear();

            Clients.Group(roomName).Statusupdate(status);
            Clients.Group(roomName).SetLeader(game.GamePhase[game.CurrentPhaseIndex].CurrentLeader.Name,
                                                Rule.SelectMemberCount(RoomList[roomName].MemberCount, game.CurrentPhaseIndex + 1));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }

    }

    /// <summary>
    /// リーダーの変更ダイアログを表示
    /// </summary>
    /// <param name="roomName"></param>
    private void LeaderCheck()
    {
        var roomName = Context.QueryString["room"];
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

        if (!phase.IsMissionMemberFull)
        {
            return;
        }

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
        if (!CallerResponceCheck(ReVoteCaller, roomName, RoomList[roomName].MemberCount))
        {
            return;
        }

        var gameIndex = GameList[roomName].CurrentPhaseIndex;
        var phase = GameList[roomName].GamePhase[gameIndex];
        var vote = phase.PhaseVote[phase.CurrentVoteIndex];

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
        var game = GameList[roomName];
        var gameIndex = game.CurrentPhaseIndex;
        var mission = game.GamePhase[gameIndex].PhaseMission;
        //if (!CallerResponceCheck(MissionStartCaller, roomName, game.PlayerList.Count))
        //{
        //    return;
        //}

        var indexList = game.PlayerList.Where(p => mission.Member.Contains(p)).Select(p => GetIndex(p));
        Clients.Caller.MissionStart(mission.Member.ToArray(), indexList.ToArray());
    }

    public void MissionUpdate(bool success)
    {
        var roomName = Context.QueryString["room"];
        var game = GameList[roomName];
        var gameIndex = game.CurrentPhaseIndex;
        var mission = game.GamePhase[gameIndex].PhaseMission;

        var player = game.PlayerList.Where(p => p.Name == Context.User.Identity.Name).SingleOrDefault();
        var index = game.PlayerList.IndexOf(player);
        mission.SetBehavior(player, success);
        Clients.Group(roomName).MissionUpdate(index);

        if (!CallerResponceCheck(MissionReslutCaller, roomName, mission.Member.Count))
        {
            return;
        }
        
        if (mission.IsConclusion)
        {
            var sortedResult = mission.Result.OrderByDescending(r => r.IsTrue).ThenBy(r => r.TargetPlayer.ConnectionId);
            mission.CarryOut(game.PlayerList.Count, game.CurrentPhaseIndex + 1);
            Clients.Group(roomName).MissionComplete(mission.IsSuccess, sortedResult.ToArray());

            var leader = game.GamePhase[game.CurrentPhaseIndex].CurrentLeader;
            game.GamePhase.Add(new Phase(game.PlayerList, leader));
            game.GamePhase[++game.CurrentPhaseIndex].NextLeader();
        }
    }

    #region Inititialize
    /// <summary>
    /// ゲームの初期化
    /// </summary>
    public void Initialization()
    {
        // ゲームを初期化する処理はここにまとめる
        var roomName = Context.QueryString["room"];
        var game = GameList[roomName];
        var gameIndex = game.CurrentPhaseIndex;
        var phase = game.GamePhase[gameIndex];
        var vote = phase.PhaseVote[phase.CurrentVoteIndex];
        var mission = game.GamePhase[gameIndex].PhaseMission;

        // プレイヤーの生成
        Clients.Caller.CreatePlayer(RoomList[roomName].PlayerList);

        // ステータス表示の更新
        var status = new List<Mission>();
        for (int i = 0; i <= game.CurrentPhaseIndex; i++)
        {            
            status.Add(game.GamePhase[i].PhaseMission);
        }

        Clients.Caller.Statusupdate(status);

        // ゲームステータスの復元
        if (!phase.IsMissionMemberFull)
        {
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
        else if (!vote.IsConclusion)
        {
            // 投票を開始する
            this.StartVote();
        }
        else if (!mission.IsConclusion)
        {
            // ミッションを開始する
            this.MissionStart();
        }
        else if (mission.IsSuccess.HasValue)
        {
            // 結果を表示する
            var sortedResult = mission.Result.OrderByDescending(r => r.IsTrue).ThenBy(r => r.TargetPlayer.ConnectionId);
            Clients.Caller.MissionComplete(mission.IsSuccess, sortedResult.ToArray());
        }
    }

    /// <summary>
    /// プレイヤーの通し番号を取得する
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <returns>通し番号(Index)</returns>
    private int GetIndex(Player player)
    {
        var roomName = Context.QueryString["room"];
        return RoomList[roomName].PlayerList.IndexOf(player);
    }

    /// <summary>
    /// 投票完了確認
    /// </summary>
    /// <param name="dict">投票状況を管理する配列</param>
    /// <param name="roomName">部屋名</param>
    /// <param name="maxCount">最大人数</param>
    /// <returns>投票完了</returns>
    private bool CallerResponceCheck(Dictionary<string, List<string>> dict, string roomName, int maxCount)
    {
        if (!dict.ContainsKey(roomName))
        {
            dict[roomName] = new List<string>();
        }

        if (!dict[roomName].Contains(Context.User.Identity.Name))
        {
            dict[roomName].Add(Context.User.Identity.Name);
        }

        if (dict[roomName].Count < maxCount)
        {
            return false;
        }

        dict[roomName].Clear();
        return true;
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