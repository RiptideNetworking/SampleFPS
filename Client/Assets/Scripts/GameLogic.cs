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

    public GameObject LocalPlayerPrefab => localPlayerPrefab;
    public GameObject PlayerPrefab => playerPrefab;
    public GameObject BulletPrefab => bulletPrefab;
    public GameObject TeleporterPrefab => teleporterPrefab;
    public GameObject LaserPrefab => laserPrefab;

    [Header("Prefabs")]
    [SerializeField] private GameObject localPlayerPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject teleporterPrefab;
    [SerializeField] private GameObject laserPrefab;

    private byte activeScene;

    private void Awake()
    {
        Singleton = this;
    }

    public void LoadScene(byte sceneBuildIndex)
    {
        StartCoroutine(LoadSceneInBackground(sceneBuildIndex));
    }

    public void UnloadActiveScene()
    {
        if (activeScene > 0)
        {
            SceneManager.UnloadSceneAsync(activeScene);
            activeScene = 0;
        }
    }

    private IEnumerator LoadSceneInBackground(byte sceneBuildIndex)
    {
        UnloadActiveScene();

        activeScene = sceneBuildIndex;
        AsyncOperation loadingScene = SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive);
        while (!loadingScene.isDone)
            yield return new WaitForSeconds(0.25f);

        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sceneBuildIndex));
    }
}
