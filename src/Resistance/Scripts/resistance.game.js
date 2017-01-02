﻿var windowoffcet = 16;

function bringToFlont(id) {
    var v = $('#' + id);
    v.appendTo(v.parent());
}

$(function () {
    var gameHub = $.connection.gameHub,
		messageFrequency = 10,
		updateRate = 1000 / messageFrequency,
		shapeModel = {
            left: 0,
            top: 0
		},
		elementname = "",
		moved = false,
		isleader = false,
        isvoted = false,
        maxselectcount = 0,
		currentselectcount = 0;

    gameHub.client.showPlayerRole = function (players) {
        for (var i = 0, len = players.length; i < len; i++) {
            if (players[i].Role === 2) {
                var currentColor = $("#player" + (i + 1) + "icon").css('background-Color');
                $("#player" + (i + 1) + "icon").animate({ backgroundColor: 'red' }, 900);
                $("#player" + (i + 1) + "icon").animate({ backgroundColor: currentColor }, 100);
            }
        }
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

    gameHub.client.updateShape = function (model, elementtarget) {
        shapeModel = model;
        $("#" + elementtarget + "icon").animate({ 'left': shapeModel.left, 'top': shapeModel.top }, { duration: updateRate, queue: false });
        bringToFlont(elementtarget);
    };

    gameHub.client.positionInitialization = function (model, target, name) {
        shapeModel = model;
        var v = $("#" + target + "icon");
        v.css('display', 'inline');
        v.animate({ 'left': shapeModel.left, 'top': shapeModel.top }, { duration: updateRate, queue: false });
        $("#" + target + "name").text(name);
        bringToFlont(target + "icon");
    };

    gameHub.client.setLeader = function (leadername, selectCount) {
        isleader = leadername === $.cookie('username');
        isvoted = false;
        maxselectcount = selectCount;

        // 状態の初期化
        $("#votebutton").css('display', 'none');
        $("#revotebutton").css('display', 'none');
        $("#missionstartbutton").css('display', 'none');
        $("#missionresultbutton").css('display', 'none');
        $("#voteokorngbutton").css('display', 'none');

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

            // 親の枠を太くする処理
            var name = iconelement.children('div').text();
            if (name === leadername) {
                iconelement.css('border', '4px solid red');
            }
            else {
                iconelement.css('border', '2px solid black');
            }
        });

        //var playerlist = $(".playericon");
        //$.each(playerlist, function (i, element) {
        //$(element).bind('tap', function () {
        //    var name = $(element).children('div').text();
        //    if (name === $.cookie('username')) {
        //        var img = $(element).children('img');
        //        img.css('opacity', '0.8');
        //        img.css('filter', 'alpha(opacity=80)');
        //        var span = $(element).children('span');
        //        span.css('background', 'url(../Image/Component/Jp/OK.png)');
        //        span.css('opacity', '1.0');
        //        img.css('filter', 'alpha(opacity=100)');
        //    }
        //    return false;
        //});
        //});

        if (isleader) {
            $("#selectbutton").css('display', 'inline');
            $("#selectimage").css('opacity', '0.4');
        } else {
            $("#selectbutton").css('display', 'none');
        }
    };

    gameHub.client.updateSelectStatus = function (element, color) {
        $("#" + element).css('background-color', color);
    };

    gameHub.client.startVote = function () {
        $("#voteokorngbutton").css('display', 'inline');
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

    $('#wrap').css('height', $(window).height() - windowoffcet);

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
        gameHub.server.playerInitialization($(window).width() - windowoffcet, $(window).height() - windowoffcet);
        gameHub.server.setLeader();

        var playerlist = $(".playericon");
        $.each(playerlist, function (i, element) {
            $(element).bind('tap', function () {
                if (isleader && !isvoted) {
                    var selected = $(element).children('.selected').val();
                    if (selected === 'false') {
                        if (currentselectcount < maxselectcount) {
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

                if (currentselectcount === maxselectcount) {
                    $("#selectimage").css('opacity', '1.0');
                } else {
                    $("#selectimage").css('opacity', '0.4');
                }

                return false;
            });
        });

        $("#selectimage").bind('tap', function () {
            if (isleader && !isvoted && currentselectcount === maxselectcount) {
                var playerlist = $(".playericon");
                $.each(playerlist, function (i, element) {
                    var selected = $(element).children('.selected').val();
                    if (selected === 'true') {
                        isvoted = true;
                        gameHub.server.vote(true);
                        $("#selectbutton").css('display', 'none');
                        $("#votebutton").css('display', 'inline');
                        gameHub.server.startVote($(element).children('div').text());
                    }
                });
            }
            return false;
        });

        $(".playername").bind('tap', function (sender) {
            if (sender.currentTarget.innerText === $.cookie('username')) {
                gameHub.server.showPlayerRole();
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

        //for (var i = 0, len = playerlist.length; i < len; i++) {
        //    var player = $(playerlist[i]);
        //    player.draggable({
        //        drag: function () {
        //            elementname = player.attr("id");
        //            shapeModel = player.position();
        //            bringToFlont(elementname);
        //            moved = true;
        //        },
        //        containment: 'parent'
        //    });
        //}

        setInterval(updateServerModel, updateRate);

        var timer = false;
        $(window).resize(function () {
            if (timer !== false) {
                clearTimeout(timer);
            }
            timer = setTimeout(function () {
                $('#wrap').css('height', $(window).height() - windowoffcet);
                gameHub.server.playerInitialization($(window).width() - windowoffcet, $(window).height() - windowoffcet);
            }, 300);
        });
    });

    function updateServerModel() {
        if (moved) {
            gameHub.server.updateModel(shapeModel, elementname);
            moved = false;
        }
    }
});