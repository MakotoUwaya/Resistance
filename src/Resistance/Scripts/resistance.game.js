﻿
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
            $("#cardstatus" + j).attr("src", "/Image/Component/Common/TeamCard.png");
            $("#plotcardlist" + j).empty();
        }
    };

    // 役割ダイアログの表示
    gameHub.client.showPlayerRole = function (players) {
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
                if (!$("#playerpanel" + num).hasClass("hidden")) {

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
                    }
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
        $("#missionmembercount").text(selectCount);
        $("#leaderimage").attr("src", "/Image/Component/Common/Player_img.png");
        $('#leadermodal').modal('show');
    };


    gameHub.client.resistanceWins = function (point) {
        document.title = point;
        $("#score").text(point);
        $("#missionresultimage").attr("src", "/Image/Component/Jp/missionsuccess.png");
        $("#missionresultbutton").css('display', 'inline');
        $("#missionbutton").css('display', 'none');
    };

    gameHub.client.spyWins = function (point) {
        document.title = point;
        $("#score").text(point);
        $("#missionresultimage").attr("src", "/Image/Component/Jp/missionfailed.png");
        $("#missionresultbutton").css('display', 'inline');
        $("#missionbutton").css('display', 'none');
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

        var playerlist = $(".cardstatus");
        $.each(playerlist, function (i, element) {
            $(element).attr("src", "/Image/Component/Common/Question.png");
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

    gameHub.client.voteComplete = function (result) {
        //$("#voteokorngbutton").css('display', 'none');
        //$("#votebutton").css('display', 'none');

        //var playerlist = $(".playericon");
        //$.each(playerlist, function (i, element) {
        //    if ($(element).children('.voted').text() === 'true') {
        //        var spanok = $(element).children('span');
        //        spanok.css('background', 'url(../Image/Component/Jp/OK.png)');
        //    }
        //    else {
        //        var spanng = $(element).children('span');
        //        spanng.css('background', 'url(../Image/Component/Jp/NG.png)');
        //    }
        //});

        //if (result) {
        //    $("#missionstartbutton").css('display', 'inline');
        //} else {
        //    $("#revotebutton").css('display', 'inline');
        //}
    };

    gameHub.client.missionStart = function (players) {
        $("#missionstartbutton").css('display', 'none');

        var playerlist = $(".playericon");
        $.each(playerlist, function (i, element) {
            var iconelement = $(element);
            iconelement.css('background-color', 'whitesmoke');
            iconelement.children(".voted").text("false");
            var img = iconelement.children('img');
            img.css('opacity', '1.0');
            img.css('filter', 'alpha(opacity=100)');
            var span = iconelement.children('span');
            span.css('background', '');
            span.css('opacity', '0.0');
            img.css('filter', 'alpha(opacity=0)');

            var name = iconelement.children('div').text();
            for (var j = 0, len = players.length; j < len; j++) {
                if (players[j].Name === name) {
                    iconelement.css('background-color', 'yellow');
                    break;
                }
            }
        });

        for (var i = 0, len = players.length; i < len; i++) {
            if (players[i].Name === $.cookie('username')) {
                $("#missionbutton").css('display', 'inline');
                break;
            }
        }
    };

    gameHub.client.missionUpdate = function (playerid) {
        //var element = $("#" + playerid + "icon");
        //var img = element.children('img');
        //img.css('opacity', '0.8');
        //img.css('filter', 'alpha(opacity=80)');
        //var span = element.children('span');
        ////span.stop().animate({ background: '' }, 500);
        //span.css('background', 'url(../Image/Component/Jp/Question.png)');
        //span.css('opacity', '1.0');
        //img.css('filter', 'alpha(opacity=100)');
    };

    gameHub.client.gameover = function (message) {
        alert(message);
        //window.history.back(-1);
    };

    $.connection.hub.start().done(function () {
        gameHub.server.initialization();

        $("#roleclosebutton").click(function () {
            if ($("#missionmembercount").text().length === 0) {
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
            gameHub.server.playerVote();
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

        $("#missionstartbutton").bind('tap', function () {
            gameHub.server.missionStart();
            return false;
        });

        $(".missionsuccessfail").bind('tap', function (sender) {
            gameHub.server.missionUpdate(sender.currentTarget.id === 'successimage');
            return false;
        });

        $("#missionresultbutton").bind('tap', function () {
            gameHub.server.setLeader();
            return false;
        });


    });

});
