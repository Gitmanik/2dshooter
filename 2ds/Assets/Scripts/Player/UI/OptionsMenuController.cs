using Photon.Pun;
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
        PhotonNetwork.LeaveRoom();
        IngameHUDManager.Instance.ToggleOptionsMenu(false);
    }
}
