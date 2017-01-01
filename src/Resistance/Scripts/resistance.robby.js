$(function () {
    // クッキーからユーザ名を取得
    $("#username").val($.cookie('username'));

    // 同期を取るルーム名を指定 ここではURL
    //$.connection.hub.qs = { room: location.href };
    $.connection.hub.qs = { room: $("#gametitle").text() };

    // デバッグ用にログを出力
    $.connection.hub.logging = true;

    // SignalR Hubの指定
    var robby = $.connection.robby;

    // サーバからクライアントへのRPC
    robby.client.received = function (user, message, count, canstart) {
        $("#list").prepend("<li class=\"list-group-item\">" + formatDate(new Date(), 'MM/DD hh:mm') + " [ " + user + " ] ：" + message + "</li>");
        $("#membercount").text(count);
        $("#startgame").prop("disabled", !canstart);
    };

    robby.client.buzz = function (user, message) {
        $("#list").prepend("<li class=\"list-group-item\">" + formatDate(new Date(), 'MM/DD hh:mm') + " [ " + user + " ] ：" + message + "</li>");
    };

    robby.client.result = function (isjoined) {
        $("#username").attr("readonly", isjoined);
        $("#roomlogin").prop("disabled", isjoined);
        $("#roomlogout").prop("disabled", !isjoined);
    };

    robby.client.statusUpdate = function (count, canstart) {
        $("#membercount").text(count);
        $("#startgame").prop("disabled", !canstart);
    };

    robby.client.gameStart = function () {
        window.location.href = "Contact";
    };

    // クライアントからサーバへのRPC
    $.connection.hub.start().done(function () {
        robby.server.windowLoaded();

        $(".joinroom").click(function () {
            var afaffa = $(this).attr('id');
            alert(afaffa);
        });

        $(".leaveroom").click(function () {
            var afaffa = $(this);
            alert(afaffa);
        });

        $("#roomlogout").click(function () {
            robby.server.withdraw($("#username").val());
        });

        $("#sendmessage").click(function () {
            var user = $("#username").val();
            if (user !== '') {
                robby.server.message(user, $("#messagetext").val());
                $("#messagetext").val('');
            }
            else {
                alert('ユーザー名を入力して下さい。');
            }
        });

        $("#startgame").click(function () {
            robby.server.gameStart();
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