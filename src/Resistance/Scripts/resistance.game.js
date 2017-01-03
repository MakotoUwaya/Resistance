var windowoffcet = 30;

function bringToFlont(id) {
    var v = $('#' + id);
    v.appendTo(v.parent());
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
        //var playerlist = $(".playericon");

        //$.each(playerlist, function (i, element) {
        //    var iconelement = $(element);
        //    iconelement.css('background-color', 'whitesmoke');
        //    iconelement.children(".voted").text("false");
        //    var img = iconelement.children('img');
        //    img.css('opacity', '1.0');
        //    img.css('filter', 'alpha(opacity=100)');
        //    var span = iconelement.children('span');
        //    span.css('background', '');
        //    span.css('opacity', '0.0');
        //    img.css('filter', 'alpha(opacity=0)');
        //});

        if (leadername === $("#hiddenplayername").val()) {
            $(".leadername").text("あなた");

            var num = 0;
            for (num = 0; num < 10; num++) {
                if (!$("#playerpanel" + num).hasClass("hidden")) {

                    // 決定ボタンを非活性にする
                    $("#leaderbutton").prop("disabled", true);

                    // 選択ボタンにメソッドをセット
                    $("#selectbutton" + num).click(function () {
                        var panelid = $(this).attr('id').replace(/selectbutton/g, '');


                        if ($("#playerbackcolor" + panelid).hasClass("bg-primary")) {
                            currentselectcount--;
                            $("#playerbackcolor" + panelid).removeClass("bg-primary");
                        } else {
                            if (currentselectcount < maxselectcount) {
                                currentselectcount++;
                                $("#playerbackcolor" + panelid).addClass("bg-primary")
                            }                            
                        }                       
                        gameHub.server.updateSelectStatus("playerbackcolor" + panelid, $("#playerbackcolor" + panelid).hasClass("bg-primary"));

                        // 決定ボタンの活性状態を変更
                        $("#leaderbutton").prop("disabled", !(currentselectcount === maxselectcount));                       
                    });
                    $("#selectbutton" + num).removeClass("hidden");

                    if (leadername === $("#playername" + num).text()) {
                        leaderindex = num;

                        // リーダーボタンにメソッドをセット
                        $("#leaderbutton").click(function () {
                            gameHub.server.startVote();
                        });

                        $("#leaderbutton").removeClass("hidden");
                        $("#leader" + num).removeClass("hidden");
                    }
                }
            }
        } else {
            $(".leadername").text(leadername);

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
        $("#missionresultimage").attr("src", "../Image/Component/Jp/missionsuccess.png");
        $("#missionresultbutton").css('display', 'inline');
        $("#missionbutton").css('display', 'none');
    };

    gameHub.client.spyWins = function (point) {
        document.title = point;
        $("#score").text(point);
        $("#missionresultimage").attr("src", "../Image/Component/Jp/missionfailed.png");
        $("#missionresultbutton").css('display', 'inline');
        $("#missionbutton").css('display', 'none');
    };

    gameHub.client.updateSelectStatus = function (element, selected) {
        if (selected) {
            $("#" + element).addClass("bg-primary");
        } else {
            $("#" + element).removeClass("bg-primary");
        }
    };

    gameHub.client.startVote = function (players) {
        // playersでダイアログの中身を更新
        // ダイアログの件数も忘れずに。
        // Leaderは〇にする
        // 何度でも再投票できるようにする
        // callerから投票状況を問合せ、Vote可能であればPlayerリストを返す
        $('#votemodal').modal('show');
    };

    gameHub.client.voteUpdate = function (playerid, ok) {
        var element = $("#" + playerid + "icon");
        element.children(".voted").text(ok);
        var img = element.children('img');
        img.css('opacity', '0.8');
        img.css('filter', 'alpha(opacity=80)');
        var span = element.children('span');
        //span.stop().animate({ background: '' }, 500);
        span.css('background', 'url(../Image/Component/Jp/Question.png)');
        span.css('opacity', '1.0');
        img.css('filter', 'alpha(opacity=100)');
    };

    gameHub.client.voteComplete = function (result) {
        $("#voteokorngbutton").css('display', 'none');
        $("#votebutton").css('display', 'none');

        var playerlist = $(".playericon");
        $.each(playerlist, function (i, element) {
            if ($(element).children('.voted').text() === 'true') {
                var spanok = $(element).children('span');
                spanok.css('background', 'url(../Image/Component/Jp/OK.png)');
            }
            else {
                var spanng = $(element).children('span');
                spanng.css('background', 'url(../Image/Component/Jp/NG.png)');
            }
        });

        if (result) {
            $("#missionstartbutton").css('display', 'inline');
        } else {
            $("#revotebutton").css('display', 'inline');
        }
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
        var element = $("#" + playerid + "icon");
        var img = element.children('img');
        img.css('opacity', '0.8');
        img.css('filter', 'alpha(opacity=80)');
        var span = element.children('span');
        //span.stop().animate({ background: '' }, 500);
        span.css('background', 'url(../Image/Component/Jp/Question.png)');
        span.css('opacity', '1.0');
        img.css('filter', 'alpha(opacity=100)');
    };

    gameHub.client.gameover = function (message) {
        alert(message);
        window.history.back(-1);
    };

    $('#wrap').css('height', $(window).height() - windowoffcet * 3);

    $(document).ready(function () {
        $(".voteokngbox").hover(function () {
            $(this).stop().animate({ backgroundColor: 'yellow' }, 250);//ONマウス時の背景色
        }, function () {
            $(this).stop().animate({ backgroundColor: 'whitesmoke' }, 250);//OFFマウス時の背景色
        });

        $("#successimage").hover(function () {
            $(this).stop().animate({ backgroundColor: 'yellow' }, 250);//ONマウス時の背景色
        }, function () {
            $(this).stop().animate({ backgroundColor: 'blue' }, 250);//OFFマウス時の背景色
        });

        $("#failimage").hover(function () {
            $(this).stop().animate({ backgroundColor: 'yellow' }, 250);//ONマウス時の背景色
        }, function () {
            $(this).stop().animate({ backgroundColor: 'red' }, 250);//OFFマウス時の背景色
        });
    });

    $.connection.hub.start().done(function () {
        gameHub.server.initialization();

        $("#roleclosebutton").click(function () {
            if ($("#missionmembercount").text().length === 0) {
                gameHub.server.setLeader();
            }
        });

        var playerlist = $(".playericon");
        $.each(playerlist, function (i, element) {
            $(element).bind('tap', function () {
                if (isleader && !isvoted) {
                    var selected = $(element).children('.selected').val();
                    if (selected === 'false') {
                        if (currentselectcount < Number($("#missionmembercount").text())) {
                            $(element).children('.selected').val('true');
                            $(element).css('background-color', 'yellow');
                            gameHub.server.updateSelectStatus($(element).attr("id"), "yellow");
                            currentselectcount++;
                        }
                    }
                    else {
                        $(element).children('.selected').val('false');
                        $(element).css('background-color', 'whitesmoke');
                        gameHub.server.updateSelectStatus($(element).attr("id"), "whitesmoke");
                        currentselectcount--;
                    }
                }

                $("#missionselectbutton").prop("disabled", currentselectcount !== Number($("#missionmembercount").text()));
                return false;
            });
        });

        $("#selectimage").bind('tap', function () {
            if (isleader && !isvoted && currentselectcount === Number($("#missionmembercount").text())) {
                var playerlist = $(".playericon");
                $.each(playerlist, function (i, element) {
                    var selected = $(element).children('.selected').val();
                    if (selected === 'true') {
                        isvoted = true;
                        gameHub.server.vote(true);
                        $("#missionselectbutton").css('display', 'none');
                        $("#votebutton").css('display', 'inline');
                        gameHub.server.startVote($(element).children('div').text());
                    }
                });
            }
            return false;
        });

        $(".voteokng").bind('tap', function (sender) {
            gameHub.server.vote(sender.currentTarget.id === 'okimage');
            return false;
        });

        $("#revotebutton").bind('tap', function () {
            gameHub.server.revote();
            return false;
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
