using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    [SerializeField] private Text text;
    [SerializeField] private Image background;

    private void OnValidate()
    {
        if (text == null)
            text = GetComponentInChildren<Text>();
        if (background == null)
            background = GetComponent<Image>();
    }

    public void SetActive(bool value)
    {
        background.enabled = value;
    }

    public void UpdateAmmo(byte loaded, ushort total)
    {
        text.text = $"{loaded} / {total}";
    }
}
