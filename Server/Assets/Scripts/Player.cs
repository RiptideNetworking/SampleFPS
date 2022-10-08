using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team : byte
{
    none,
    green,
    orange
}

[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public string Username { get; private set; }
    public bool IsAlive => health > 0f;
    public PlayerMovement Movement => movement;

    [SerializeField] private float respawnSeconds;
    [SerializeField] private float health;
    [SerializeField] private float maxHealth;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private WeaponManager weaponManager;

    private Team team;

    private void OnValidate()
    {
        if (movement == null)
            movement = GetComponent<PlayerMovement>();
        if (weaponManager == null)
            weaponManager = GetComponent<WeaponManager>();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            health = 0f;
            Die();
        }
        else
            SendHealthChanged();
    }

    private void Die()
    {
        movement.Enabled(false);
        StartCoroutine(DelayedRespawn());
        SendDied();
    }

    private IEnumerator DelayedRespawn()
    {
        yield return new WaitForSeconds(respawnSeconds);

        InstantRespawn();
    }

    public void InstantRespawn()
    {
        TeleportToTeamSpawnpoint();
        movement.Enabled(true);

        health = maxHealth;
        weaponManager.ResetWeapons();
        SendRespawned();
    }

    private void TeleportToTeamSpawnpoint()
    {
        if (team == Team.green)
            movement.Teleport(GameLogic.Singleton.GreenSpawn.position);
        else if (team == Team.orange)
            movement.Teleport(GameLogic.Singleton.OrangeSpawn.position);
    }

    public static void Spawn(ushort id, string username)
    {
        foreach (Player otherPlayer in list.Values)
            otherPlayer.SendSpawned(id);

        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
        player.team = id % 2 == 0 ? Team.orange : Team.green; // NOTE: if players leave this can lead to uneven teams (eg: player 2 leaves, rejoins as player 3, now the only 2 players in the game are on the same team because both their IDs are odd numbers)

        if (GameLogic.Singleton.IsGameInProgress)
            player.TeleportToTeamSpawnpoint(); // Ensure players joining into an ongoing game are spawned at their team's spawnpoint

        player.SendSpawned();
        list.Add(id, player);
    }

    #region Messages
    private void SendSpawned()
    {
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawned)));
    }

    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawned)), toClientId);
    }

    private Message AddSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddByte((byte)team);
        message.AddVector3(transform.position);
        return message;
    }

    private void SendHealthChanged()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerHealthChanged);
        message.AddFloat(health);
        NetworkManager.Singleton.Server.Send(message, Id);
    }

    private void SendDied()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerDied);
        message.AddUShort(Id);
        message.AddVector3(transform.position);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void SendRespawned()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerRespawned);
        message.AddUShort(Id);
        message.AddVector3(transform.position);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message)
    {
        Spawn(fromClientId, message.GetString());
    }

    [MessageHandler((ushort)ClientToServerId.input)]
    private static void Input(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
            player.Movement.SetInput(message.GetBools(6), message.GetVector3());
    }

    [MessageHandler((ushort)ClientToServerId.switchActiveWeapon)]
    private static void SwitchActiveWeapon(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
            player.weaponManager.SetActiveWeapon((WeaponType)message.GetByte());
    }

    [MessageHandler((ushort)ClientToServerId.primaryUse)]
    private static void PrimaryUse(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
            player.weaponManager.PrimaryUsePressed();
    }

    [MessageHandler((ushort)ClientToServerId.reload)]
    private static void Reload(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
            player.weaponManager.Reload();
    }
    #endregion
}
