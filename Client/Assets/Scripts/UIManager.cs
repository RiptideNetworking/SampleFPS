using Riptide;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _singleton;
    public static UIManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(UIManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    [Header("Connect")]
    [SerializeField] private GameObject connectUI;
    [SerializeField] private InputField usernameField;
    [Header("Game")]
    [SerializeField] private GameObject gameUI;
    [SerializeField] private Slider healthbar;
    [SerializeField] private WeaponUI pistolUI;
    [SerializeField] private WeaponUI teleporterUI;
    [SerializeField] private WeaponUI laserUI;
    [SerializeField] private Image hitmarker;
    [SerializeField] private AudioSource hitAudio;
    [SerializeField] private AudioSource hurtAudio;

    private void Awake()
    {
        Singleton = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            GetComponent<Canvas>().enabled = !GetComponent<Canvas>().enabled;
    }

    public void ConnectClicked()
    {
        usernameField.interactable = false;
        connectUI.SetActive(false);
        gameUI.SetActive(true);

        NetworkManager.Singleton.Connect();
    }

    public void BackToMain()
    {
        usernameField.interactable = true;
        connectUI.SetActive(true);
        gameUI.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HealthUpdated(float health, float maxHealth, bool playHurtSound)
    {
        healthbar.value = health / maxHealth;

        if (playHurtSound)
            hurtAudio.Play();
    }

    public void ActiveWeaponUpdated(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.none:
                pistolUI.SetActive(false);
                teleporterUI.SetActive(false);
                laserUI.SetActive(false);
                break;
            case WeaponType.pistol:
                pistolUI.SetActive(true);
                teleporterUI.SetActive(false);
                laserUI.SetActive(false);
                break;
            case WeaponType.teleporter:
                pistolUI.SetActive(false);
                teleporterUI.SetActive(true);
                laserUI.SetActive(false);
                break;
            case WeaponType.laser:
                pistolUI.SetActive(false);
                teleporterUI.SetActive(false);
                laserUI.SetActive(true);
                break;
            default:
                Debug.Log($"Can't set UI as active for unknown weapon type '{type}'!");
                break;
        }
    }

    public void AmmoUpdated(WeaponType type, byte loaded, ushort total)
    {
        switch (type)
        {
            case WeaponType.pistol:
                pistolUI.UpdateAmmo(loaded, total);
                break;
            case WeaponType.teleporter:
                teleporterUI.UpdateAmmo(loaded, total);
                break;
            case WeaponType.laser:
                laserUI.UpdateAmmo(loaded, total);
                break;
            default:
                Debug.Log($"Can't update ammo display for unknown weapon type '{type}'!");
                break;
        }
    }

    public void ShowHitmarker()
    {
        hitAudio.Play();
        hitmarker.enabled = true;
        StartCoroutine(HideHitmarker());
    }

    private IEnumerator HideHitmarker()
    {
        yield return new WaitForSeconds(0.1f);
        hitmarker.enabled = false;
    }

    public void SendName()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ClientToServerId.name);
        message.AddString(usernameField.text);
        NetworkManager.Singleton.Client.Send(message);
    }
}
