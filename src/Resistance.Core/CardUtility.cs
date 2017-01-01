using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resistance.Core
{
    public static class CardUtility
    {
        public static List<PlotCard> Piles { get; private set; }

        static CardUtility()
        {
            Piles = new List<PlotCard>();
        }

        public static PlotCard[] DrawCards(int playerCount)
        {
            var drawCardList = new List<PlotCard>();
            var randam = new Random();

            for (int i = 0, let = Rule.DrowCardCount(playerCount); i < let; i++)
            {
                var card = Piles[randam.Next(playerCount)];
                drawCardList.Add(card);
                Piles.Remove(card);
            }
            return drawCardList.ToArray();
        }

        public static void PassCard(Player target, PlotCard card )
        {
            target.PossesionCards.Add(card);
        }

        public static void PassCard(Player user, Player target, PlotCard card)
        {
            if (user.PossesionCards.Contains(card))
            {
                user.PossesionCards.Remove(card);
            }
            target.PossesionCards.Add(card);
        }

        public static void UsedCard(PlotCard card, Player user, Player target )
        {
            var plot = user.PossesionCards.Where(c => c == card && c.IsUsed == false).FirstOrDefault();
            if (plot != null)
            {
                plot.Effect(user, target);
                plot.IsUsed = true;
            }
            else
            {
                throw new ArgumentOutOfRangeException("対象のカードが存在しません。");
            }
        }

        public static void Initialize(int memberCount)
        {
            Piles = new List<PlotCard>();
            Piles.Add(new PlotCard("責任者",
                "あなたは他のプレイヤーの陰謀カードを1枚引き取らなければならない。", "",
                CardExecuteTiming.AfterCardDrow, true,
                ( a, b ) => System.Diagnostics.Debug.Assert(a != b)));
            Piles.Add(new PlotCard("強力なリーダー",
                "あなたはリーダーになれる。陰謀カードもしくはチームカードが分配される前に使用する。", "",
                CardExecuteTiming.BeforeCardDrow, true,
                ( a, b ) => System.Diagnostics.Debug.Assert(a != b)));
            Piles.Add(new PlotCard("強力なリーダー",
                "あなたはリーダーになれる。陰謀カードもしくはチームカードが分配される前に使用する。", "",
                CardExecuteTiming.BeforeCardDrow, true,
                ( a, b ) => System.Diagnostics.Debug.Assert(a != b)));
            Piles.Add(new PlotCard("不信",
                "あなたは承認された投票を無効とし、リーダーを交代させることができる。", "",
                CardExecuteTiming.AfterVote, true,
                ( a, b ) => System.Diagnostics.Debug.Assert(a != b)));
            Piles.Add(new PlotCard("総意の形成者",
                "あなたはこのカードを獲得した後、他のプレイヤーより先に投票カードを公開して投票しなくてはならない。", "",
                CardExecuteTiming.BeforeVote, false,
                ( a, b ) => System.Diagnostics.Debug.Assert(a != b)));
            Piles.Add(new PlotCard("監視者",
                "あなたは、プレイヤーの前に出されたミッションカードを１枚見ることができる。", "",
                CardExecuteTiming.AfterMission, true,
                ( a, b ) => System.Diagnostics.Debug.Assert(a != b)));
            Piles.Add(new PlotCard("監視者",
                "あなたは、プレイヤーの前に出されたミッションカードを１枚見ることができる。", "",
                CardExecuteTiming.AfterMission, true,
                ( a, b ) => System.Diagnostics.Debug.Assert(a != b)));

            if (7 <= memberCount)
            {
                Piles.Add(new PlotCard("総意の形成者",
                    "あなたはこのカードを獲得した後、他のプレイヤーより先に投票カードを公開して投票しなくてはならない。", "",
                    CardExecuteTiming.BeforeVote, false,
                    ( a, b ) => System.Diagnostics.Debug.Assert(a != b)));
                Piles.Add(new PlotCard("立ち聞きされた会話",
                    "あなたはすぐ右にいるか左にいるプレイヤー1人の役割カードの中身を見なければならない。", "",
                    CardExecuteTiming.AfterCardDrow, true,
                    ( a, b ) => System.Diagnostics.Debug.Assert(a != b)));
                Piles.Add(new PlotCard("立ち聞きされた会話",
                    "あなたはすぐ右にいるか左にいるプレイヤー1人の役割カードの中身を見なければならない。", "",
                    CardExecuteTiming.AfterCardDrow, true,
                    ( a, b ) => System.Diagnostics.Debug.Assert(a != b)));
                Piles.Add(new PlotCard("情報開示",
                    "あなたは自分の役割カードをあなたの選んだ他のプレイヤー(リーダーを含む)１人に見せなくてはならない。", "",
                    CardExecuteTiming.AfterCardDrow, true,
                    ( a, b ) => System.Diagnostics.Debug.Assert(a != b)));
                Piles.Add(new PlotCard("注目の的",
                    "あなたは他のプレイヤー１人を選び、ミッションカードを公開して提出させることができる。", "",
                    CardExecuteTiming.BeforeMission, true,
                    ( a, b ) => System.Diagnostics.Debug.Assert(a != b)));
                Piles.Add(new PlotCard("信用の確立",
                    "リーダーは自分の役割カードを他のプレイヤー１人を選んで見せなくてはならない。", "",
                    CardExecuteTiming.AfterCardDrow, true,
                    ( a, b ) => System.Diagnostics.Debug.Assert(a != b)));
                Piles.Add(new PlotCard("不信",
                    "あなたは承認された投票を無効とし、リーダーを交代させることができる。", "",
                    CardExecuteTiming.AfterVote, true,
                    ( a, b ) => System.Diagnostics.Debug.Assert(a != b)));
                Piles.Add(new PlotCard("不信",
                    "あなたは承認された投票を無効とし、リーダーを交代させることができる。", "",
                    CardExecuteTiming.AfterVote, true,
                    ( a, b ) => System.Diagnostics.Debug.Assert(a != b)));
            }
        }
    }
}
