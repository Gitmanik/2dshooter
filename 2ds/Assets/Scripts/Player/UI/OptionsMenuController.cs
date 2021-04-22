using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Slider VolumeSlider;
    [SerializeField] private Toggle FullscreenToggle;
    [SerializeField] private Dropdown ResolutionList;

    private List<Dropdown.OptionData> resolutions;

    private void OnEnable()
    {
        VolumeSlider.value = AudioListener.volume;
        VolumeSlider.onValueChanged.RemoveAllListeners();
        VolumeSlider.onValueChanged.AddListener(OnChangedVolume);

        FullscreenToggle.isOn = DataManager.Fullscreen;
        FullscreenToggle.onValueChanged.RemoveAllListeners();
        FullscreenToggle.onValueChanged.AddListener(OnFullscreenClicked);

        ResolutionList.ClearOptions();
        resolutions = new List<Resolution>(Screen.resolutions).ConvertAll((Resolution x) => $"{x.width}x{x.height}").Distinct().ToList().ConvertAll(x => new Dropdown.OptionData(x));
        ResolutionList.AddOptions(resolutions);
        ResolutionList.SetValueWithoutNotify(resolutions.IndexOf(resolutions.Find(x => x.text == $"{Screen.currentResolution.width}x{Screen.currentResolution.height}")));
        ResolutionList.onValueChanged.RemoveAllListeners();
        ResolutionList.onValueChanged.AddListener(OnResolutionSet);

    }

    private void OnResolutionSet(int arg0)
    {
        var res = resolutions[arg0].text.Split('x');
        DataManager.ResolutionHeight = Convert.ToInt32(res[0]);
        DataManager.ResolutionWidth = Convert.ToInt32(res[1]);
        Screen.SetResolution(Convert.ToInt32(res[0]), Convert.ToInt32(res[1]), DataManager.Fullscreen, 0);
    }

    private void OnChangedVolume(float newv)
    {
        AudioListener.volume = newv;
        DataManager.MainVolume = newv;
    }

    public void OnFullscreenClicked(bool newv)
    {
        DataManager.Fullscreen = newv;
        Screen.fullScreen = newv;
    }

    public void OnDisconnectClick()
    {
        PhotonNetwork.LeaveRoom();
        IngameHUDManager.Instance.ToggleOptionsMenu(false);
    }
}
