using RiptideNetworking;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Gun pistol;
    [SerializeField] private Gun teleporter;
    [SerializeField] private Gun laser;
    
    private WeaponType activeType;
    private Gun activeWeapon;

    private void OnValidate()
    {
        if (player == null)
            player = GetComponent<Player>();
    }

    public void SetActiveWeapon(WeaponType type)
    {
        if (activeType == type)
            return;

        switch (type)
        {
            case WeaponType.none:
                activeWeapon = null;
                break;
            case WeaponType.pistol:
                activeWeapon = pistol;
                break;
            case WeaponType.teleporter:
                activeWeapon = teleporter;
                break;
            case WeaponType.laser:
                activeWeapon = laser;
                break;
            default:
                Debug.LogError($"Can't set unknown weapon type '{type}' as active!");
                return;
        }

        activeType = type;
        SendActiveWeaponUpdate(type);
    }

    public void PrimaryUsePressed()
    {
        if (activeWeapon == null)
            return;

        activeWeapon.Shoot();
    }

    public void Reload()
    {
        if (activeWeapon == null)
            return;

        activeWeapon.Reload();
    }

    public void ResetWeapons()
    {
        pistol.ResetAmmo();
        teleporter.ResetAmmo();
        laser.ResetAmmo();
    }

    private void SendActiveWeaponUpdate(WeaponType type)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerActiveWeaponUpdated);
        message.AddUShort(player.Id);
        message.AddByte((byte)type);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
}
