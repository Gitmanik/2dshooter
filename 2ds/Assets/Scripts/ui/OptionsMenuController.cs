using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuController : MonoBehaviour
{
    [SerializeField] private Slider VolumeSlider;
    private void OnEnable()
    {
        VolumeSlider.value = AudioListener.volume;
        VolumeSlider.onValueChanged.RemoveAllListeners();
        VolumeSlider.onValueChanged.AddListener(OnChangedVolume);
    }
    private void OnChangedVolume(float newv)
    {
        AudioListener.volume = newv;
        DataManager.MainVolume = newv;
    }

    public void OnDisconnectClick()
    {
        NetworkManager manager = NetworkManager.singleton;
        if (manager == null)
            return;

        switch (manager.mode)
        {
            case NetworkManagerMode.ServerOnly:
                manager.StopServer();
                break;
            case NetworkManagerMode.Host:
                manager.StopHost();
                break;
            case NetworkManagerMode.ClientOnly:
                manager.StopClient();
                break;
        }
        IngameHUDManager.Instance.ToggleOptionsMenu(false);
    }
}
