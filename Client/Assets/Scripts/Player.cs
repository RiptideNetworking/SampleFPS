using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public enum Team : byte
{
    none,
    green,
    orange
}

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }
    public bool IsAlive => health > 0f;
    public WeaponManager WeaponManager => weaponManager;

    [SerializeField] private float maxHealth;
    [SerializeField] private GameObject model;
    [SerializeField] private MeshRenderer headband;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private PlayerAnimationManager animationManager;
    [SerializeField] private Transform camTransform;

    [Header("Team Colors")]
    [SerializeField] private Material none;
    [SerializeField] private Material green;
    [SerializeField] private Material orange;

    private string username;
    private float health;

    private void OnValidate()
    {
        if (weaponManager == null)
            weaponManager = GetComponent<WeaponManager>();
        if (animationManager == null)
            animationManager = GetComponent<PlayerAnimationManager>();
    }

    private void Start()
    {
        health = maxHealth;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    private void Move(Vector3 newPosition, Vector3 forward)
    {
        transform.position = newPosition;
        
        if (!IsLocal)
            camTransform.forward = forward;
        
        animationManager.AnimateBasedOnSpeed();
    }

    public void SetHealth(float amount)
    {
        health = Mathf.Clamp(amount, 0f, maxHealth);
        UIManager.Singleton.HealthUpdated(health, maxHealth, true);
    }

    public void Died(Vector3 position)
    {
        transform.position = position;
        health = 0f;
        model.SetActive(false);
        weaponManager.DisableWeapons();

        if (IsLocal)
            UIManager.Singleton.HealthUpdated(health, maxHealth, true);
    }

    public void Respawned(Vector3 position)
    {
        transform.position = position;
        health = maxHealth;
        model.SetActive(true);
        weaponManager.EnableWeapons();

        if (IsLocal)
            UIManager.Singleton.HealthUpdated(health, maxHealth, false);
    }

    public static void Spawn(ushort id, string username, Team team, Vector3 position)
    {
        Player player;
        if (id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = true;
        }
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocal = false;
        }

        player.name = $"Player {id} (username)";
        player.Id = id;
        player.username = username;

        switch (team)
        {
            case Team.none:
                player.headband.material = player.none;
                break;
            case Team.green:
                player.headband.material = player.green;
                break;
            case Team.orange:
                player.headband.material = player.orange;
                break;
            default:
                break;
        }

        list.Add(id, player);
    }

    #region Messages
    [MessageHandler((ushort)ServerToClientId.activeScene)]
    private static void ActiveScene(Message message)
    {
        GameLogic.Singleton.LoadScene(message.GetByte());
    }

    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), (Team)message.GetByte(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
            player.Move(message.GetVector3(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.playerHealthChanged)]
    private static void PlayerHealthChanged(Message message)
    {
        if (list.TryGetValue(NetworkManager.Singleton.Client.Id, out Player player))
            player.SetHealth(message.GetFloat());
    }

    [MessageHandler((ushort)ServerToClientId.playerActiveWeaponUpdated)]
    private static void PlayerActiveWeaponUpdated(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
        {
            WeaponType newType = (WeaponType)message.GetByte();
            player.WeaponManager.SetWeaponActive(newType);

            if (player.IsLocal)
                UIManager.Singleton.ActiveWeaponUpdated(newType);
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerAmmoChanged)]
    private static void PlayerAmmoChanged(Message message)
    {
        UIManager.Singleton.AmmoUpdated((WeaponType)message.GetByte(), message.GetByte(), message.GetUShort());
    }

    [MessageHandler((ushort)ServerToClientId.playerDied)]
    private static void PlayerDied(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
            player.Died(message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.playerRespawned)]
    private static void PlayerRespawned(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
            player.Respawned(message.GetVector3());
    }
    #endregion
}
