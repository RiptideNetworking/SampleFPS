using UnityEngine;

public enum WeaponType : byte
{
    none,
    pistol,
    teleporter,
    laser
}

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private GameObject weaponModels;

    [SerializeField] private GameObject pistolModel;
    [SerializeField] private AudioSource pistolAudioSource;
    [SerializeField] private Animator pistolMuzzleFlash;

    [SerializeField] private GameObject teleporterModel;
    [SerializeField] private AudioSource teleporterAudioSource;
    [SerializeField] private Animator teleporterMuzzleFlash;

    [SerializeField] private GameObject laserModel;
    [SerializeField] private AudioSource laserAudioSource;
    [SerializeField] private Animator laserMuzzleFlash;

    private void OnValidate()
    {
        if (pistolModel != null)
        {
            pistolAudioSource = pistolModel.transform.parent.GetComponent<AudioSource>(); // Using pistolModel.GetComponentInParent<AudioSource>() somehow causes issues with the prefab saving the assigned value, but this works even though it *should* function identically...must be a Unity bug?
            pistolMuzzleFlash = pistolModel.transform.parent.GetComponentInChildren<Animator>();
        }

        if (teleporterModel != null)
        {
            teleporterAudioSource = teleporterModel.transform.parent.GetComponent<AudioSource>(); // Using teleporterModel.GetComponentInParent<AudioSource>() somehow causes issues with the prefab saving the assigned value, but this works even though it *should* function identically...must be a Unity bug?
            teleporterMuzzleFlash = teleporterModel.transform.parent.GetComponentInChildren<Animator>();
        }

        if (laserModel != null)
        {
            laserAudioSource = laserModel.transform.parent.GetComponent<AudioSource>(); // Using laserModel.GetComponentInParent<AudioSource>() somehow causes issues with the prefab saving the assigned value, but this works even though it *should* function identically...must be a Unity bug?
            laserMuzzleFlash = laserModel.transform.parent.GetComponentInChildren<Animator>();
        }
    }

    public void EnableWeapons()
    {
        weaponModels.SetActive(true);
    }

    public void DisableWeapons()
    {
        weaponModels.SetActive(false);
        pistolMuzzleFlash.GetComponent<Light>().intensity = 0;
        teleporterMuzzleFlash.GetComponent<Light>().intensity = 0;
        laserMuzzleFlash.GetComponent<Light>().intensity = 0;
    }
    
    public void SetWeaponActive(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.none:
                pistolModel.SetActive(false);
                teleporterModel.SetActive(false);
                laserModel.SetActive(false);
                break;
            case WeaponType.pistol:
                pistolModel.SetActive(true);
                teleporterModel.SetActive(false);
                laserModel.SetActive(false);
                break;
            case WeaponType.teleporter:
                pistolModel.SetActive(false);
                teleporterModel.SetActive(true);
                laserModel.SetActive(false);
                break;
            case WeaponType.laser:
                pistolModel.SetActive(false);
                teleporterModel.SetActive(false);
                laserModel.SetActive(true);
                break;
            default:
                Debug.LogError($"Can't set unknown weapon type '{type}' as active!");
                break;
        }
    }

    public void Shot(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.none:
                break;
            case WeaponType.pistol:
                pistolAudioSource.Play();
                pistolMuzzleFlash.Play("MuzzleFlash", -1, 0);
                break;
            case WeaponType.teleporter:
                teleporterAudioSource.Play();
                teleporterMuzzleFlash.Play("MuzzleFlash", -1, 0);
                break;
            case WeaponType.laser:
                laserAudioSource.Play();
                laserMuzzleFlash.Play("MuzzleFlash", -1, 0);
                break;
            default:
                Debug.LogError($"Can't shoot unknown weapon type '{type}'!");
                break;
        }
    }
}
