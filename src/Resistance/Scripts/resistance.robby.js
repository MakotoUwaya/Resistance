$(function () {
    // 同期を取るルーム名を指定 ここではURL
    $.connection.hub.qs = { robby: location.href };

    // デバッグ用にログを出力
    $.connection.hub.logging = true;

    // SignalR Hubの指定
    var robby = $.connection.robby;

    // サーバからクライアントへのRPC
    robby.client.buzz = function (user, message) {
        $("#chatlist").prepend("<li class=\"list-group-item\">" + formatDate(new Date(), 'MM/DD hh:mm') + " [ " + user + " ] <br />" + message + "</li>");
    };

    robby.client.reset = function () {
        $("#roomlistbody").empty();
    };

    robby.client.roomUpdate = function (roomname, count, user, message) {
        var row = $("<tr></tr>", {
            "id": "roomrow-" + roomname
        });

        // 既に存在していればinnerHTMLを削除
        var target = $("#roomrow-" + roomname);
        if (target.length === 0) {
            $("#roomlistbody").append(row);
            target = $("#roomrow-" + roomname);
        } else {
            target.empty();
        }

        var cell1 = $("<td></td>");

        cell1.append($("<a></a>",{
            "class": "dialoglink btn btn-link",
            "data-toggle": "modal",
            "data-target": "#roomdialog",
            "text": roomname,
            "id": "dialoglink-" + roomname
        }));
        target.append(cell1);

        var cell2 = $("<td></td>");
        cell2.append(count + "人");
        target.append(cell2);

        var cell3 = $("<td></td>");
        var joinbutton = $("<button></button>",{
            "id": "joinroom-" + roomname,
            "class": "joinroom btn btn-default"
        });

        joinbutton.append($("<span></span>",{
            "class": "glyphicon glyphicon-log-in"
        }));
        cell3.append(joinbutton);
        target.append(cell3);

        var cell4 = $("<td></td>");
        var leavebutton = $("<button></button>",{
            "id": "leaveroom-" + roomname,
            "class": "leaveroom btn btn-default"
        });

        leavebutton.append($("<span></span>", {
            "class": "glyphicon glyphicon-log-out"
        }));
        cell4.append(leavebutton);
        target.append(cell4);      
  
        $(".dialoglink").click(function () {
            var roomname = $(this).attr('id').replace(/dialoglink-/g, '');
            robby.server.dialogupdate(roomname);
        });

        $(".joinroom").click(function () {
            robby.server.join($(this).attr('id').replace(/joinroom-/g, ''));
        });

        $(".leaveroom").click(function () {
            robby.server.withdraw($(this).attr('id').replace(/leaveroom-/g, ''));
        });

        if (message !== "")
        {
            $("#chatlist").prepend("<li class=\"list-group-item\">" + formatDate(new Date(), 'MM/DD hh:mm') + " [ " + user + " ] <br />" + message + "</li>");
        }

        if ($("#roomdialogtitle").text() === roomname) {
            robby.server.dialogupdate(roomname);
        }
    };

    robby.client.dialogupdate = function (roomname, players, canstart) {
        $("#roomdialogtitle").text(roomname);
        $("#roomdialogmembercount").text(players.length);
        $("#roomdialogplayerlist").empty();
        $.each(players, function (i, p) {
            $("#roomdialogplayerlist").append('<div class="thumbnailcontainer"><div><img src="/Image/Component/Common/Player_img.png" alt="'
                + p.Name + '" class="img-thumbnail thumbnailminisize" /></div>' + p.Name + '</div>');
        });

        $("#startgame").prop("disabled", !canstart);
    };

    robby.client.removeRoom = function (roomname) {
        var target = $("#roomrow-" + roomname);
        if (target.length === 1) {
            target.remove();
        }
    };

    robby.client.gameStart = function (roomname) {
        window.location.href = "/Home/GameStart?roomName=" + roomname;
    };

    // クライアントからサーバへのRPC
    $.connection.hub.start().done(function () {
        robby.server.windowLoaded();

        $("#createroom").click(function () {
            robby.server.create($("#roomname").val());
        });

        $("#startgame").click(function () {
            robby.server.gameStart($("#roomdialogtitle").text());
        });

        $("#resetgame").click(function () {
            robby.server.reset();
        });

    });
});

// 日付文字列整形
var formatDate = function (date, format) {
    if (!format) format = 'YYYY-MM-DD hh:mm:ss.SSS';
    format = format.replace(/YYYY/g, date.getFullYear());
    format = format.replace(/MM/g, ('0' + (date.getMonth() + 1)).slice(-2));
    format = format.replace(/DD/g, ('0' + date.getDate()).slice(-2));
    format = format.replace(/hh/g, ('0' + date.getHours()).slice(-2));
    format = format.replace(/mm/g, ('0' + date.getMinutes()).slice(-2));
    format = format.replace(/ss/g, ('0' + date.getSeconds()).slice(-2));
    if (format.match(/S/g)) {
        var milliSeconds = ('00' + date.getMilliseconds()).slice(-3);
        var length = format.match(/S/g).length;
        for (var i = 0; i < length; i++) format = format.replace(/S/, milliSeconds.substring(i, i + 1));
    }
    return format;
};

