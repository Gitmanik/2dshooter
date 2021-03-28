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
        if (NetworkManager.singleton == null)
            return;

        switch (NetworkManager.singleton.mode)
        {
            case NetworkManagerMode.ServerOnly:
                NetworkManager.singleton.StopServer();
                break;
            case NetworkManagerMode.Host:
                NetworkManager.singleton.StopHost();
                break;
            case NetworkManagerMode.ClientOnly:
                NetworkManager.singleton.StopClient();
                break;
        }
        IngameHUDManager.Instance.ToggleOptionsMenu(false);
    }
}
