using RiptideNetworking;
using UnityEngine;

public enum WeaponType : byte
{
    none,
    pistol,
    teleporter,
    laser
}

public class Gun : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private WeaponType type;
    [SerializeField] private float shotSpeed;
    [SerializeField] private byte loadedAmmo;
    [SerializeField] private byte maxLoadedAmmo;
    [SerializeField] private ushort totalAmmo;

    private ushort maxTotalAmmo;

    private void OnValidate()
    {
        if (player == null)
            player = GetComponentInParent<Player>();
    }

    private void Start()
    {
        maxTotalAmmo = totalAmmo;
    }

    public void Shoot()
    {
        if (loadedAmmo > 0 && player.IsAlive)
        {
            Projectile.Spawn(player, type, transform.position, transform.forward * shotSpeed * Time.fixedDeltaTime);
            loadedAmmo--;
            SendAmmoUpdated();
        }
    }

    public void Reload()
    {
        if (totalAmmo > 0)
        {
            byte amountToReload = (byte)Mathf.Min(maxLoadedAmmo - loadedAmmo, totalAmmo);
            loadedAmmo += amountToReload;
            totalAmmo -= amountToReload;
            SendAmmoUpdated();
        }
    }

    public void ResetAmmo()
    {
        loadedAmmo = maxLoadedAmmo;
        totalAmmo = maxTotalAmmo;
        SendAmmoUpdated();
    }

    private void SendAmmoUpdated()
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerAmmoChanged);
        message.AddByte((byte)type);
        message.AddByte(loadedAmmo);
        message.AddUShort(totalAmmo);
        NetworkManager.Singleton.Server.Send(message, player.Id);
    }
}
