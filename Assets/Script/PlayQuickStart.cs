using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using LeanCloud.Play;

public class PlayQuickStart : MonoBehaviour {
    public Text idText = null;
    public Text scoreText = null;
    public Text resultText = null;

    // 获取客户端 SDK 实例
    private Play play = Play.Instance;

	// Use this for initialization
	void Start () {
        // 设置 SDK 日志委托
        LeanCloud.Play.Logger.LogDelegate = (level, log) =>
        {
            if (level == LogLevel.Debug) {
                Debug.LogFormat("[DEBUG] {0}", log);
            } else if (level == LogLevel.Warn) {
                Debug.LogFormat("[WARN] {0}", log);
            } else if (level == LogLevel.Error) {
                Debug.LogFormat("[ERROR] {0}", log);
            }
        };

        // App Id
        var APP_ID = "315XFAYyIGPbd98vHPCBnLre-9Nh9j0Va";
        // App Key
        var APP_KEY = "Y04sM6TzhMSBmCMkwfI3FpHc";
        // App 节点地区
        var APP_REGION = Region.EastChina;
        // 初始化
        play.Init(APP_ID, APP_KEY, APP_REGION);

        // 这里使用随机数作为 userId
        var random = new System.Random();
        var randId = string.Format("{0}", random.Next(10000000));
        play.UserId = randId;
        this.idText.text = string.Format("Id: {0}", randId);

        play.On(LeanCloud.Play.Event.CONNECTED, (evtData) =>
        {
            Debug.Log("connected");
            // 根据当前时间（时，分）生成房间名称
            var now = System.DateTime.Now;
            var roomName = string.Format("{0}_{1}", now.Hour, now.Minute);
            play.JoinOrCreateRoom(roomName);
        });
        play.On(LeanCloud.Play.Event.ROOM_JOINED, (evtData) =>
        {
            Debug.Log("joined room");
        });
        // 注册新玩家加入房间事件
        play.On(LeanCloud.Play.Event.PLAYER_ROOM_JOINED, (evtData) =>
        {
            var newPlayer = evtData["newPlayer"] as Player;
            Debug.LogFormat("new player: {0}", newPlayer.UserId);
            if (play.Player.IsMaster) {
                // 获取房间内玩家列表
                var playerList = play.Room.PlayerList;
                for (int i = 0; i < playerList.Count; i++) {
                    var player = playerList[i];
                    var props = new Dictionary<string, object>();
                    // 判断如果是房主，则设置 10 分，否则设置 5 分
                    if (player.IsMaster) {
                        props.Add("point", 10);
                    } else {
                        props.Add("point", 5);
                    }
                    player.SetCustomProperties(props);
                }
                var data = new Dictionary<string, object>();
                data.Add("winnerId", play.Room.Master.ActorId);
                var opts = new SendEventOptions();
                opts.ReceiverGroup = ReceiverGroup.All;
                play.SendEvent("win", data, opts);
            }
        });
        // 注册「玩家属性变更」事件
        play.On(LeanCloud.Play.Event.PLAYER_CUSTOM_PROPERTIES_CHANGED, (evtData) => {
            var player = evtData["player"] as Player;
            // 判断如果玩家是自己，则做 UI 显示
            if (player.IsLocal) {
                // 得到玩家的分数
                long point = (long)player.CustomProperties["point"];
                Debug.LogFormat("{0} : {1}", player.UserId, point);
                this.scoreText.text = string.Format("Score: {0}", point);
            }
        });
        // 注册自定义事件
        play.On(LeanCloud.Play.Event.CUSTOM_EVENT, (evtData) =>
        {
            // 得到事件参数
            var eventId = evtData["eventId"] as string;
            if (eventId == "win") {
                var eventData = evtData["eventData"] as Dictionary<string, object>;
                // 得到胜利者 Id
                int winnerId = (int)(long)eventData["winnerId"];
                // 如果胜利者是自己，则显示胜利 UI；否则显示失败 UI
                if (play.Player.ActorId == winnerId) {
                    Debug.Log("win");
                    this.resultText.text = "Win";
                } else {
                    Debug.Log("lose");
                    this.resultText.text = "Lose";
                }
                play.Disconnect();
            }
        });

        // 连接服务器
        play.Connect();
	}
	
	// Update is called once per frame
	void Update () {
        play.HandleMessage();
	}
}
