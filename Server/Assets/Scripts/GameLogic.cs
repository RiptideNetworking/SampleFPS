using RiptideNetworking;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLogic : MonoBehaviour
{
    private static GameLogic _singleton;
    public static GameLogic Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(GameLogic)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public bool IsGameInProgress => activeScene == 2;
    public Transform GreenSpawn { get; set; }
    public Transform OrangeSpawn { get; set; }
    public GameObject PlayerPrefab => playerPrefab;
    public GameObject BulletPrefab => bulletPrefab;
    public GameObject TeleporterPrefab => teleporterPrefab;
    public GameObject LaserPrefab => laserPrefab;

    [SerializeField] private float roundLengthSeconds;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject teleporterPrefab;
    [SerializeField] private GameObject laserPrefab;
    
    private byte activeScene;

    private void Awake()
    {
        Singleton = this;
    }

    public void PlayerCountChanged(ushort clientId)
    {
        SendActiveScene(clientId);

        if (NetworkManager.Singleton.Server.ClientCount >= 2 && activeScene == 1)
            StartCoroutine(LobbyCountdown()); // Start game when 2 or more players are connected
    }

    private IEnumerator LobbyCountdown()
    {
        yield return new WaitForSeconds(10f);

        if (NetworkManager.Singleton.Server.ClientCount >= 2)
        {
            StartCoroutine(LoadSceneInBackground(2));
            StartCoroutine(GameCountdown());
        }
    }

    private IEnumerator GameCountdown()
    {
        yield return new WaitForSeconds(roundLengthSeconds);

        StartCoroutine(LoadSceneInBackground(1));
        StartCoroutine(LobbyCountdown());
    }

    public void LoadScene(byte sceneBuildIndex)
    {
        StartCoroutine(LoadSceneInBackground(sceneBuildIndex));
    }

    private IEnumerator LoadSceneInBackground(byte sceneBuildIndex)
    {
        if (activeScene > 0)
            SceneManager.UnloadSceneAsync(activeScene);

        activeScene = sceneBuildIndex;
        SendNewActiveScene();

        AsyncOperation loadingScene = SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive);
        while (!loadingScene.isDone)
            yield return new WaitForSeconds(0.25f);

        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sceneBuildIndex));
        foreach (Player player in Player.list.Values)
            player.InstantRespawn();
    }

    private void SendActiveScene(ushort toClientId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.activeScene);
        message.AddByte(activeScene);
        NetworkManager.Singleton.Server.Send(message, toClientId);
    }

    private void SendNewActiveScene()
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.activeScene);
        message.AddByte(activeScene);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
}
