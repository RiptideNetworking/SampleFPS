using RiptideNetworking;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform camTransform;

    private bool[] inputs;

    private void Start()
    {
        inputs = new bool[6];
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
            inputs[0] = true;

        if (Input.GetKey(KeyCode.S))
            inputs[1] = true;

        if (Input.GetKey(KeyCode.A))
            inputs[2] = true;

        if (Input.GetKey(KeyCode.D))
            inputs[3] = true;

        if (Input.GetKey(KeyCode.Space))
            inputs[4] = true;

        if (Input.GetKey(KeyCode.LeftShift))
            inputs[5] = true;


        if (Input.GetKeyDown(KeyCode.X))
            SendSwitchActiveWeapon(WeaponType.none);

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SendSwitchActiveWeapon(WeaponType.pistol);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SendSwitchActiveWeapon(WeaponType.teleporter);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SendSwitchActiveWeapon(WeaponType.laser);

        if (Input.GetMouseButtonDown(0))
            SendPrimaryUse();

        if (Input.GetKeyDown(KeyCode.R))
            SendReload();
    }

    private void FixedUpdate()
    {
        SendInput();

        for (int i = 0; i < inputs.Length; i++)
            inputs[i] = false;
    }

    #region Messages
    private void SendInput()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ClientToServerId.input);
        message.AddBools(inputs, false);
        message.AddVector3(camTransform.forward);
        NetworkManager.Singleton.Client.Send(message);
    }

    private void SendSwitchActiveWeapon(WeaponType newType)
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.switchActiveWeapon);
        message.AddByte((byte)newType);
        NetworkManager.Singleton.Client.Send(message);
    }

    private void SendPrimaryUse()
    {
        NetworkManager.Singleton.Client.Send(Message.Create(MessageSendMode.reliable, ClientToServerId.primaryUse));
    }

    private void SendReload()
    {
        NetworkManager.Singleton.Client.Send(Message.Create(MessageSendMode.reliable, ClientToServerId.reload));
    }
    #endregion
}
