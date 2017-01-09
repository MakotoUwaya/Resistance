
function userMassage(message, type) {
    $("#usermessage").removeClass("alert-success");
    $("#usermessage").removeClass("alert-warning");
    $("#usermessage").removeClass("alert-info");
    $("#usermessage").removeClass("alert-success");

    $("#usermessage").addClass(type);
    $("#usermessage").text(message);
}

$(function () {
    // 同期を取るルーム名を指定 ここではURL
    $.connection.hub.qs = { room: $("#hiddenroomname").val() };

    // デバッグ用にログを出力
    $.connection.hub.logging = true;

    // 変数宣言
    var gameHub = $.connection.gameHub,
		elementname = "",
        leaderindex = -1,
        maxselectcount = 0,
		currentselectcount = 0;

    // プレイヤーの生成
    gameHub.client.createPlayer = function (players) {
        for (var i = 0; i < 10; i++) {
            if (!$("#playerpanel" + i).hasClass("hidden")) {
                $("#playerpanel" + i).addClass("hidden");
            }            
        }

        for (var j = 0, len = players.length; j < len; j++) {
            $("#playerpanel" + j).removeClass("hidden");
            $("#playername" + j).text(players[j].Name);
            //$("#leaderbutton").removeClass("hidden");
            //$("#selectbutton" + i).removeClass("hidden");
            $("#playerimage" + j).attr("src", "/Image/Component/Common/Player_img.png");
            $("#plotcardlist" + j).empty();
        }
    };

    // 役割ダイアログの表示
    gameHub.client.showPlayerRole = function (players) {
        userMassage("役割の確認中です。しばらくお待ちください。", "alert-success");

        $("#spylist").empty();
        if (players.length === 0) {
            $("#rolename").text("レジスタンス");
            $("#roleimage").attr("src", "/Image/Component/Common/resistance_img.png");
            $("#roleimage").attr("alt", "resistance");
        } else {
            $("#rolename").text("スパイ");
            $("#roleimage").attr("src", "/Image/Component/Common/spy_img.png");
            $("#roleimage").attr("alt", "spy");
        }     
        
        $.each(players, function (i, p) {
            $("#spylist").append('<a class="btn btn-default" role="button">' + p.Name + '</a>');
        });

        $('#rolemodal').modal('show');
    };

    // リーダーの設定
    gameHub.client.setLeader = function (leadername, selectCount) {

        if (leadername === $("#hiddenplayername").val()) {
            userMassage("ミッションメンバーを" + selectCount + "名選択してください。", "alert-success");
            $("#leadername").text("あなた");

            var num = 0;
            for (num = 0; num < 10; num++) {
                if ($("#playerpanel" + num).hasClass("hidden")) {
                    continue;
                }

                // 決定ボタンを非活性にする
                $("#leaderbutton").prop("disabled", true);

                // 選択ボタンにメソッドをセット
                $("#selectbutton" + num).click(function () {
                    var panelid = $(this).attr('id').replace(/selectbutton/g, '');


                    if ($("#playerbackcolor" + panelid).hasClass("bg-info")) {
                        currentselectcount--;
                        $("#playerbackcolor" + panelid).removeClass("bg-info");
                    } else {
                        if (currentselectcount < maxselectcount) {
                            currentselectcount++;
                            $("#playerbackcolor" + panelid).addClass("bg-info")
                        }
                    }
                    gameHub.server.updateSelectStatus("playerbackcolor" + panelid, $("#playerbackcolor" + panelid).hasClass("bg-info"));

                    // 決定ボタンの活性状態を変更
                    $("#leaderbutton").prop("disabled", !(currentselectcount === maxselectcount));
                });

                $("#selectbutton" + num).removeClass("hidden");

                if (leadername === $("#playername" + num).text()) {
                    leaderindex = num;

                    $("#leaderbutton").removeClass("hidden");
                    $("#leader" + num).removeClass("hidden");
                } else {
                    $("#leader" + num).addClass("hidden");
                }
            }
        } else {
            userMassage("ミッションメンバー：" + selectCount + "名 リーダーの選択を待機しています．．．", "alert-success");
            $("#leadername").text(leadername);

            for (var k = 0; k < 10; k++) {
                if (leadername === $("#playername" + k).text()) {
                    $("#leader" + k).removeClass("hidden");
                }
            }
        }

        maxselectcount = selectCount;
        currentselectcount = 0;
        $(".missionmembercount").text(selectCount);
        $("#leaderimage").attr("src", "/Image/Component/Common/Player_img.png");
        $('#leadermodal').modal('show');
    };


    gameHub.client.statusupdate = function (status) {
        var resistancewincount = 0,
            spywincount = 0;

        $.each(status, function (i, state) {
            var index = i + 1;
            if (status.length === index) {
                $("#phase" + index).removeClass("text-muted");
                $("#phase" + index).addClass("text-success");
            } else {
                $("#phase" + index).removeClass("text-muted");
                $("#phase" + index).removeClass("text-success");
                if (state.IsSuccess) {
                    resistancewincount++;
                    $("#phase" + index).addClass("text-primary");
                } else {
                    spywincount++;
                    $("#phase" + index).addClass("text-danger");
                }
            }
        });

        $("#resistancewincount").text(resistancewincount);
        $("#spywincount").text(spywincount);
    };

    gameHub.client.updateSelectStatus = function (element, selected) {
        if (selected) {
            $("#" + element).addClass("bg-info");
        } else {
            $("#" + element).removeClass("bg-info");
        }
    };

    gameHub.client.startVote = function (players) {
        $("#votedialogmembercount").text(players.length);
        for (var i = 0, len = players.length; i < len; i++) {
            $("#missionmember" + i + "image").attr("src", "/Image/Component/Common/Player_img.png");
            $("#missionmember" + i + "image").attr("alt", players[i].Name);
            $("#missionmember" + i + "name").text(players[i].Name);
            $("#missionmember" + i).removeClass("hidden");
        }

        var cardstatuslist = $(".cardstatus");
        $.each(cardstatuslist, function (i, element) {
            $(element).attr("src", "/Image/Component/Common/Question.png");
            $(element).removeClass("hidden");
        });

        // リーダーは自動的に信任する
        if ($("#playername" + leaderindex).text() === $("#hiddenplayername").val()) {
            userMassage("ミッションメンバー：" + players.length + "名 投票の完了を待機しています．．．", "alert-success");
            gameHub.server.sendVote(true);
        } else {
            userMassage("ミッションメンバー：" + players.length + "名 信任投票をしてください。", "alert-success");
            $("#votebutton").removeClass("hidden");
            $("#votemodal").modal("show");
        }
    };

    gameHub.client.precedenceVote = function (players) {
        $("#votedialogmembercount").text(players.length);
        for (var i = 0, len = players.length; i < len; i++) {
            $("#missionmember" + i + "image").attr("src", "/Image/Component/Common/Player_img.png");
            $("#missionmember" + i + "image").attr("alt", players[i].Name);
            $("#missionmember" + i + "name").text(players[i].Name);
            $("#missionmember" + i).removeClass("hidden");
        }

        var cardstatuslist = $(".cardstatus");
        $.each(cardstatuslist, function (i, element) {
            $(element).attr("src", "/Image/Component/Common/Question.png");
            $(element).removeClass("hidden");
        });

        // リーダーは自動的に信任する
        if ($("#playername" + leaderindex).text() === $("#hiddenplayername").val()) {
            userMassage("ミッションメンバー：" + players.length + "名 投票の完了を待機しています．．．", "alert-success");
            gameHub.server.sendVote(true);
        } else {
            userMassage("ミッションメンバー：" + players.length + "名 信任投票をしてください。", "alert-success");
            $("#votebutton").removeClass("hidden");
            $("#votemodal").modal("show");
        }
    };

    // 投票状態が確認できたら「？」マークをカードに変えていく
    gameHub.client.voteUpdate = function (index) {
        var element = $("#cardstatus" + index).attr("src", "/Image/Component/Common/TeamCard.png");
    };

    // 投票結果を確認
    gameHub.client.voteComplete = function (result) {
        if (result) {
            userMassage("信任されました。ミッションを開始します。", "alert-success");
            $("#aftervotemessage").text("賛成多数により信任されました。\nミッションを開始します。");
            $("#aftervotebutton").removeClass("hidden");
            $("#votenextstepbutton").click(function () {
                $("#votemodal").modal("hide");
                gameHub.server.missionStart();
            });
        } else {
            $("#aftervotemessage").text("反対多数により不信任となりました。");
            $("#aftervotebutton").addClass("hidden");
            $("#votenextstepbutton").click(function () {
                $("#votemodal").modal("hide");
                // TODO: リーダーを変更して次の投票を開始する処理
                gameHub.server.reVote();
            });
        }

        userMassage("他プレイヤーの確認を待っています．．．", "alert-success");

        $("#votebutton").addClass("hidden");
        $("#beforevotemessage").addClass("hidden");
        $("#approvebutton").addClass("hidden");
        $("#rejectbutton").addClass("hidden");
        
        $("#aftervotemessage").removeClass("hidden");
        $("#votenextstepbutton").removeClass("hidden");
        $("#votemodal").modal("show");
    };

    // ミッションを開始
    gameHub.client.missionStart = function (members, memberindex) {
        var ismissionmember = false;
        for (var i = 0, len = members.length; i < len; i++) {
            if (members[i].Name == $("#hiddenplayername").val())
            {
                ismissionmember = true;
            }

            $("#carryoutmember" + i + "image").attr("src", "/Image/Component/Common/Player_img.png");
            $("#carryoutmember" + i + "image").attr("alt", members[i].Name);
            $("#carryoutmember" + i + "name").text(members[i].Name);
            $("#carryoutmember" + i).removeClass("hidden");
        }

        var cardstatuslist = $(".cardstatus");
        $.each(cardstatuslist, function (i, element) {
            if (memberindex.indexOf(i) >= 0) {
                $(element).removeClass("hidden");
            } else {
                $(element).addClass("hidden");
            }         
            $(element).attr("src", "/Image/Component/Common/Question.png");
        });

        if (ismissionmember) {
            $("#successbutton").removeClass("hidden");
            $("#failbutton").removeClass("hidden");
        } else {
            $("#successbutton").addClass("hidden");
            $("#failbutton").addClass("hidden");
        }

        $("#missionbutton").removeClass("hidden");
        $("#beforemissionmodal").modal("show");
    };

    // ミッション結果を確認できたら「？」マークをカードに変えていく
    gameHub.client.missionUpdate = function (index) {
        var element = $("#cardstatus" + index).attr("src", "/Image/Component/Common/TeamCard.png");
    };

    // 投票結果を確認
    gameHub.client.missionComplete = function (result, cards) {
        var successcount = 0,
            failcount = 0;       

        $("#missionresultmessage").removeClass("text-info");
        $("#aftermissionmodal").modal("show");

        for (var i = 0, len = cards.length; i < len; i++) {
            $("#missioncard" + i + "maskimage").removeClass("hidden");
            if (cards[i].IsTrue) {
                successcount++;
                $("#missioncard" + i + "image").attr("src", "/Image/Component/Common/Success.png");
            } else {
                failcount++;
                $("#missioncard" + i + "image").attr("src", "/Image/Component/Common/Fail.png");
            }
            
            //$("#missioncard" + i).removeClass("hidden");
            $("#missioncard" + i).delay(200 * (i + 1)).fadeIn(500);
        }


        if (result) {
            $("#missionresultmessage").addClass("text-primary");
            $("#missionresultmessage").text("成功");
        } else {
            $("#missionresultmessage").addClass("text-danger");
            $("#missionresultmessage").text("失敗");
        }
        $("#successmembercount").text(successcount);
        $("#failmembercount").text(failcount);
    };

    // ゲーム終了
    gameHub.client.gameover = function (message) {
        alert(message);
        //window.history.back(-1);
    };

    // 接続開始
    $.connection.hub.start().done(function () {
        gameHub.server.initialization();

        $("#roleclosebutton").click(function () {
            if ($(".missionmembercount").text().length === 0) {
                gameHub.server.setLeader();
            }
        });

        // リーダーの選択確定ボタン
        $("#leaderbutton").click(function () {
            gameHub.server.startVote();
            $(this).addClass("hidden");
        });

        // 再投票用のボタン
        $("#votebutton").click(function () {
            $("#votemodal").modal("show");
        });

        // 信任・不信任ボタン
        $("#approvebutton").click(function () {
            userMassage("投票の完了を待機しています．．．", "alert-success");
            gameHub.server.sendVote(true);
        });
        $("#rejectbutton").click(function () {
            userMassage("投票の完了を待機しています．．．", "alert-success");
            gameHub.server.sendVote(false);
        });

        // ミッション実行用のボタン
        $("#missionbutton").click(function () {
            $("#beforemissionmodal").modal("show");
        });

        // ミッション成功・失敗ボタン
        $("#successbutton").click(function () {
            userMassage("ミッションの結果を待機しています．．．", "alert-success");
            gameHub.server.missionUpdate(true);
        });
        $("#failbutton").click(function () {
            userMassage("ミッションの結果を待機しています．．．", "alert-success");
            gameHub.server.missionUpdate(false);
        });

        // 次のフェーズへボタン
        $("#nextphasebutton").click(function () {
            userMassage("他プレイヤーの確認を待っています．．．", "alert-success");
            gameHub.server.setLeader();
        });

        // UNDONE: 暫定的にリーダー確認処理を追加
        $("#checkleader").click(function () {
            gameHub.server.leaderCheck();
        });
    });
});
