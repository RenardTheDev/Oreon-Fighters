using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayerIOClient;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    MultiplayerBase multiplayer;

    public string PlayerName = "";

    [Header("Prelogin ui")]
    public InputField inp_nickname;
    public Button btn_connect;
    public Text text_preloginStatus;
    public Image preloginFader;

    [Header("MainMenu ui")]
    public GameObject mainMenu;

    [Header("start game ui")]
    public Button btn_goLocal;
    public Button btn_goOnline;
    public Button btn_goDev;

    private void Awake()
    {
        Application.runInBackground = true;

        multiplayer = FindObjectOfType<MultiplayerBase>();
        multiplayer.OnConnectedToPlayerIOEvent += OnConnectedToPlayerIO;
        multiplayer.OnPIOConnectionError += OnConnectionError;

        btn_connect.onClick.AddListener(OnConnectionAttempt);
        inp_nickname.onEndEdit.AddListener(OnNickNameEditEnd);

        btn_goLocal.onClick.AddListener(StartLocalGame);
        btn_goOnline.onClick.AddListener(StartOnlineGame);
        btn_goDev.onClick.AddListener(StartDevGame);

        System.Random random = new System.Random();
        PlayerName = PlayerPrefs.GetString("NickName", "Player#" + random.Next(0, 10000).ToString("0000"));

        inp_nickname.text = PlayerName;

        text_preloginStatus.text = "Before you connect, you can change your name above.";
    }

    private void Update()
    {

    }

    void OnConnectionAttempt()
    {
        if (PlayerName.Length < 3)
        {
            return;
        }

        inp_nickname.interactable = false;
        btn_connect.interactable = false;
        btn_goLocal.interactable = false;
        btn_connect.GetComponentInChildren<Text>().text = "Connecting...";

        multiplayer.ConnectToPIO(PlayerName);
    }

    void StartLocalGame()
    {
        AsyncOperation sceneLoading = SceneManager.LoadSceneAsync(2);
        sceneLoading.completed += OnLocalLevelLoaded;

        btn_goDev.interactable = false;
        btn_goOnline.interactable = false;

    }

    void StartOnlineGame()
    {
        AsyncOperation sceneLoading = SceneManager.LoadSceneAsync(1);
        sceneLoading.completed += OnOnlineLevelLoaded;

        btn_goDev.interactable = false;
        btn_goOnline.interactable = false;
    }

    bool startDevGame = false;
    void StartDevGame()
    {
        startDevGame = true;
        AsyncOperation sceneLoading = SceneManager.LoadSceneAsync(1);
        sceneLoading.completed += OnOnlineLevelLoaded;

        btn_goDev.interactable = false;
        btn_goOnline.interactable = false;
    }

    void OnLocalLevelLoaded(AsyncOperation obj)
    {

    }

    void OnOnlineLevelLoaded(AsyncOperation obj)
    {
        if (startDevGame)
        {
            multiplayer.CreateOrJoinGameOnDevServer("TDM");
        }
        else
        {
            multiplayer.CreateOrJoinGame("TDM");
        }
    }

    void OnNickNameEditEnd(string newName)
    {
        if (inp_nickname.interactable)
        {
            PlayerName = newName;
            PlayerPrefs.SetString("NickName", PlayerName);
        }
    }

    void OnConnectedToPlayerIO(Client client)
    {
        StartCoroutine(onConnectedToPlayerIO());
    }

    IEnumerator onConnectedToPlayerIO()
    {
        btn_connect.gameObject.SetActive(false);
        btn_goLocal.gameObject.SetActive(false);
        inp_nickname.gameObject.SetActive(false);

        for (float i = 0; i < 3f; i += 0.02f)
        {
            var c = preloginFader.color;
            c.a = 1f - (i / 3f);
            preloginFader.color = c;

            yield return new WaitForSeconds(0.02f);
        }

        preloginFader.gameObject.SetActive(false);


        text_preloginStatus.gameObject.SetActive(false);

        mainMenu.SetActive(true);
    }

    void OnConnectionError(PlayerIOError error)
    {
        text_preloginStatus.text = "Connection error:\n" + error.Message;

        inp_nickname.interactable = true;
        btn_connect.interactable = true;
        btn_goLocal.interactable = true;
        btn_connect.GetComponentInChildren<Text>().text = "Connect";
    }
}
