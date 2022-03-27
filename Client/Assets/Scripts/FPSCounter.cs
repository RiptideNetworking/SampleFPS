using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private Text fpsText;
    [SerializeField] private float updateFrequency = 1f;
    [SerializeField] private bool showMs = false;

    private float timeSinceUpdate = 0.0f;
    private int framesSinceUpdate = 0;

    private void OnValidate()
    {
        if (fpsText == null)
            fpsText = GetComponent<Text>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
            fpsText.enabled = !fpsText.enabled;

        timeSinceUpdate += Time.deltaTime;
        framesSinceUpdate++;

        if (timeSinceUpdate >= updateFrequency)
        {
            if (showMs && NetworkManager.Singleton.Client.IsConnected)
                fpsText.text = $"{framesSinceUpdate / timeSinceUpdate:0.0}fps\n" +
                    $"{Mathf.Max(0f, NetworkManager.Singleton.Client.SmoothRTT):0}ms";
            else
                fpsText.text = $"{framesSinceUpdate / timeSinceUpdate:0.0}fps";
            
            timeSinceUpdate = 0.0f;
            framesSinceUpdate = 0;
        }
    }
}
