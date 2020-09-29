using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using PlayerIOClient;

public class GameManager : MonoBehaviour
{
    MultiplayerBase multiplayer;

    ProjectileManager proj;

    [Header("GLOBAL IO")]
    private Client pioClient;
    private Connection pioConnection;
    private List<Message> msgList = new List<Message>(); //  Messsage queue implementation
    private bool joinedroom = false;
    public float sendRateFPS = 30f;

    [Header("Player Data")]
    public string localUserID;
    public int localid;
    public PlayerControl lpControl;

    [Header("UI")]
    public float chatEntryStayTime = 60;
    public int chatEntrySize = 32;
    public Text chatTextField;
    private List<ChatEntry> chatEntries = new List<ChatEntry>();

    [Header("Network players")]
    public Dictionary<int, Player> player = new Dictionary<int, Player>();  // playerID / playerData

    [Header("Game items")]
    public GameObject[] shipPrefab;
    public Weapon[] weaponItems;
    Dictionary<int, Weapon> weaponByID;
    Dictionary<Weapon, int> idByWeapon;

    private void Awake()
    {
        multiplayer = FindObjectOfType<MultiplayerBase>();
        multiplayer.OnPIOConnectionError += PlayerIOErrorHangler;
        multiplayer.OnConnectedToRoomEvent += OnJoinedRoom;

        proj = FindObjectOfType<ProjectileManager>();
        lpControl = FindObjectOfType<PlayerControl>();

        weaponByID = new Dictionary<int, Weapon>();
        idByWeapon = new Dictionary<Weapon, int>();
        for (byte i = 0; i < weaponItems.Length; i++)
        {
            weaponByID.Add(i, weaponItems[i]);
            idByWeapon.Add(weaponItems[i], i);
        }
    }

    void Start()
    {
        player.Clear();

        // Create a random userid if it's not set
        if (Application.isEditor)
        {
            localUserID = "#UnityEditor#";
        }
        if (localUserID.Length < 1)
        {
            System.Random random = new System.Random();
            localUserID = "Player#" + random.Next(0, 10000).ToString("0000");

            SendMessageToLocal("Generated player name - \'" + localUserID + "\'");
        }

        InvokeRepeating("SendLocalPlayerState", 0, 1.0f / sendRateFPS);
        InvokeRepeating("UpdateChatWindow", 0, 0.1f);
    }

    void OnConnectedToPlayerIO(Client client)
    {
        pioClient = client;

        Debug.Log("Successfully connected to Player.IO");
        SendMessageToLocal("Successfully connected to Player.IO");

        Debug.Log("Create ServerEndpoint");
        SendMessageToLocal("Create ServerEndpoint");
        // Comment out the line below to use the live servers instead of your development server
        pioClient.Multiplayer.DevelopmentServer = new ServerEndpoint("192.168.1.3", 8184);

        Debug.Log("Create or Join Room");
        SendMessageToLocal("Create or Join Room");
        //Create or join the room 
        pioClient.Multiplayer.CreateJoinRoom(
            "TestID",           // Room id. If set to null a random roomid is used
            "TDM",              // The room type started on the server
            true,               // Should the room be visible in the lobby?
            null,               // roomData
            null,               // userData
            OnJoinedRoom,
            PlayerIOErrorHangler
        );
    }

    void OnJoinedRoom(Connection connection)
    {
        Debug.Log("Joined Room.");
        SendMessageToLocal("Joined Room.");
        // We successfully joined a room so set up the message handler
        pioConnection = connection;
        pioConnection.OnMessage += handlemessage;
        joinedroom = true;
    }

    void PlayerIOErrorHangler(PlayerIOError error)
    {
        Debug.Log("Error: " + error.ToString());
        SendMessageToLocal("Error: " + error.ToString());
    }

    void handlemessage(object sender, Message m)
    {
        msgList.Add(m);
    }

    void FixedUpdate()
    {
        // process message queue
        foreach (Message m in msgList)
        {
            int playerid = -1;
            string userID = "";

            switch (m.Type)
            {
                //Players CONNECTIONS
                case "OnPlayerJoined":
                    playerid = m.GetInt(0);
                    userID = m.GetString(1);

                    Debug.Log("OnPlayerJoined(" + playerid + ", " + userID + ")");

                    if (!player.ContainsKey(playerid))
                    {
                        var newPlayer = CreatePlayerShip(playerid, userID);

                        newPlayer.rig.isKinematic = true;
                        newPlayer.shipGO.tag = "OtherPlayer";
                    }
                    else Debug.LogError("Id \'" + playerid + "\' already exist in dictionary.");

                    SendMessageToLocal(player[playerid].PlayerName + " joined.");

                    break;

                case "OnPlayerJoinedLocal": //assign local player
                    playerid = m.GetInt(0);
                    localid = playerid;

                    Debug.Log("OnPlayerJoined(" + localid + ", " + localUserID + ")");

                    if (!player.ContainsKey(playerid))
                    {
                        var newPlayer = CreatePlayerShip(playerid, localUserID);

                        joinedroom = true;
                        newPlayer.shipGO.tag = "Player";
                        lpControl.AssignPlayer(newPlayer.ship);
                        newPlayer.weapon.ShipMadeShot += SendLocalPlayerShot;
                    }
                    else Debug.LogError("Id \'" + playerid + "\' already exist in dictionary.");

                    break;

                case "OnPlayerLeft":
                    playerid = m.GetInt(0);

                    Destroy(player[playerid].shipGO, 0f);

                    SendMessageToLocal(player[playerid].PlayerName + " left.");

                    player.Remove(playerid);

                    break;

                //Players GAME UPDATES and EVENTS
                case "PlayersUpdate":

                    int playercount = m.GetInt(0);

                    for (uint i = 0; i < playercount; i++)
                    {
                        int id = m.GetInt(1 + i * 8);

                        Vector3 pos = new Vector3(m.GetFloat(2 + i * 8), m.GetFloat(3 + i * 8), m.GetFloat(4 + i * 8));
                        Vector3 rot = new Vector3(m.GetFloat(5 + i * 8), m.GetFloat(6 + i * 8), m.GetFloat(7 + i * 8));
                        float health = m.GetFloat(8 + i * 8);

                        if (id != localid && player.ContainsKey(id))
                        {
                            player[id].rig.position = pos;
                            player[id].rig.rotation = Quaternion.Euler(rot);
                            player[id].ship.health = health;
                        }
                    }

                    break;

                case "OnPlayerGotHit":

                    break;

                case "OnPlayerGotKilled":

                    break;

                case "OnPlayersShot":

                    int shotCount = m.GetInt(0);

                    for (uint i = 0; i < shotCount; i++)
                    {
                        NetShot shot = new NetShot(
                            m.GetInt(1 + i * 8), m.GetInt(2 + i * 8),
                            new float[] { m.GetFloat(3 + i * 8), m.GetFloat(4 + i * 8), m.GetFloat(5 + i * 8) },
                            new float[] { m.GetFloat(6 + i * 8), m.GetFloat(7 + i * 8), m.GetFloat(8 + i * 8) }
                            );

                        if (shot.playerid != localid) proj.SpawnProjectile(
                                new Vector3(shot.pos[0], shot.pos[1], shot.pos[2]),
                                new Vector3(shot.dir[0], shot.dir[1], shot.dir[2]),
                                player[shot.playerid].ship, weaponByID[shot.weapid]
                                );
                    }

                    break;

                //Global EVENTS
                case "OnRoundStarted":

                    break;

                case "OnRoundEnded":

                    break;

                case "OnChatMessage":
                    playerid = m.GetInt(0);
                    string msg = m.GetString(1);

                    SendMessageToLocal("<color=green>" + player[playerid].PlayerName + "</color> >. " + msg);
                    break;

            }
        }

        // clear message queue after it's been processed
        msgList.Clear();
    }

    Player CreatePlayerShip(int playerid, string userID)
    {
        Player newPlayer = new Player(userID);

        newPlayer.shipGO = Instantiate(shipPrefab[0], Random.onUnitSphere * 500f, Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f));
        newPlayer.trans = newPlayer.shipGO.transform;
        newPlayer.rig = newPlayer.shipGO.GetComponent<Rigidbody>();
        newPlayer.ship = newPlayer.shipGO.GetComponent<Ship>();
        newPlayer.motor = newPlayer.shipGO.GetComponent<ShipMotor>();
        newPlayer.weapon = newPlayer.shipGO.GetComponent<ShipWeapon>();

        player.Add(playerid, newPlayer);

        return newPlayer;
    }

    void SendLocalPlayerState()
    {
        if (joinedroom && player.ContainsKey(localid))
        {
            //---ship status---
            Message update = Message.Create("PlayerUpdate");

            Vector3 pos = player[localid].rig.position;
            Vector3 rot = player[localid].rig.rotation.eulerAngles;
            float health = player[localid].ship.health;

            update.Add(pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, health);

            pioConnection.Send(update);

            //---shots---
            Message shotPack = Message.Create("OnPlayerShot", networkShot.Count);

            for (int i = 0; i < networkShot.Count; i++)
            {
                shotPack.Add(networkShot[i].playerid, networkShot[i].weapid,
                    networkShot[i].pos[0], networkShot[i].pos[1], networkShot[i].pos[2],
                    networkShot[i].dir[0], networkShot[i].dir[1], networkShot[i].dir[2]
                    );
            }

            pioConnection.Send(shotPack);

            networkShot.Clear();
        }
    }

    List<NetShot> networkShot = new List<NetShot>();
    public void SendLocalPlayerShot(Vector3 pos, Vector3 dir, Ship shooter, Weapon weaponData)
    {
        networkShot.Add(new NetShot(
            localid, idByWeapon[weaponData],
            new float[] { pos.x, pos.y, pos.z }, new float[] { dir.x, dir.y, dir.z }
            ));
    }

    public void SendMessageToLocal(string msg)
    {
        chatEntries.Add(new ChatEntry(Time.time, msg));

        UpdateChatWindow();
    }

    void ClearChat()
    {
        chatEntries.Clear();
        UpdateChatWindow();
    }

    void UpdateChatWindow()
    {
        chatTextField.text = "";

        chatEntries.RemoveAll(x => x.time < Time.time - chatEntryStayTime);

        if (chatEntries.Count > chatEntrySize)
        {
            chatEntries.RemoveRange(chatEntrySize, chatEntries.Count - chatEntrySize);
        }

        if (chatEntries.Count > 0)
        {
            chatTextField.text += chatEntries[0].text;
            for (int i = 1; i < chatEntries.Count; i++)
            {
                chatTextField.text += "\n" + chatEntries[i].text;
            }
        }

        Vector2 textFieldSize = chatTextField.rectTransform.sizeDelta;
        textFieldSize.y = chatTextField.preferredHeight + 8;
        chatTextField.rectTransform.sizeDelta = textFieldSize;
    }
}

public struct NetShot
{
    public int playerid;
    public int weapid;
    public float[] pos;
    public float[] dir;

    public NetShot(int playerid, int weapid, float[] pos, float[] dir)
    {
        this.playerid = playerid;
        this.weapid = weapid;
        this.pos = pos;
        this.dir = dir;
    }
}

public class Player
{
    public string PlayerName;
    public GameObject shipGO;
    public Transform trans;
    public Rigidbody rig;
    public Ship ship;
    public ShipMotor motor;
    public ShipWeapon weapon;

    public Player(string playerName)
    {
        PlayerName = playerName;
    }
}

public class ChatEntry
{
    public float time;
    public string text;

    public ChatEntry(float time, string text)
    {
        this.time = time;
        this.text = text;
    }
}