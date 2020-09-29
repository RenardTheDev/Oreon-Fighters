using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIOClient;

public class MultiplayerBase : MonoBehaviour
{
    public static MultiplayerBase mpBase;

    private void Awake()
    {
        if (!mpBase)
        {
            DontDestroyOnLoad(gameObject);
            mpBase = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private Client pioClient;
    private Connection pioConnection;

    public void ConnectToPIO(string playerName)
    {
        PlayerIO.Authenticate(
            "oreon-fighters-8basuqufgu2ji3hoswxcw", //game id
            "public",                               //connection id
            new Dictionary<string, string> {        //Authentication arguments
				{ "userId", playerName },
            },
            null,                                   //PlayerInsight segments
            OnConnectedToPlayerIO,
            PlayerIOErrorHangler
        );
    }

    public void CreateOrJoinGameOnDevServer(string roomtype)
    {
        pioClient.Multiplayer.DevelopmentServer = new ServerEndpoint("192.168.1.3", 8184);
        CreateOrJoinGame(roomtype);
    }

    public void CreateOrJoinGame(string roomtype)
    {
        pioClient.Multiplayer.ListRooms(roomtype, null, 0, 0, OnGetRoomList);
    }

    void OnGetRoomList(RoomInfo[] info)
    {
        if (info.Length > 0)
        {
            foreach (RoomInfo i in info)
            {
                if (i.OnlineUsers >= 20)
                {
                    continue;
                }
                else
                {
                    pioClient.Multiplayer.JoinRoom(i.Id, null, OnJoinedRoom, PlayerIOErrorHangler);
                    return;
                }
            }
        }

        //---if no room were found---
        pioClient.Multiplayer.CreateJoinRoom(null, "TDM", true, null, null, OnJoinedRoom, PlayerIOErrorHangler);
    }

    public delegate void PIOConnectionHandler(Client client);
    public event PIOConnectionHandler OnConnectedToPlayerIOEvent;
    void OnConnectedToPlayerIO(Client client)
    {
        Debug.Log("Successfully connected to Player.IO");
        pioClient = client;

        OnConnectedToPlayerIOEvent(client);
    }


    public delegate void PIOErrorHandler(PlayerIOError error);
    public event PIOErrorHandler OnPIOConnectionError;
    void PlayerIOErrorHangler(PlayerIOError error)
    {
        Debug.Log("Connection error:\n\n" + error.Message);

        OnPIOConnectionError(error);
    }

    public delegate void PIORoomConnectionHandler(Connection connection);
    public event PIORoomConnectionHandler OnConnectedToRoomEvent;
    void OnJoinedRoom(Connection connection)
    {
        OnConnectedToRoomEvent(connection);
    }
}
