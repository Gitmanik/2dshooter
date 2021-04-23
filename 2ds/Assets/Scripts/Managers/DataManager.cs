using Photon.Pun;
using System;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static string Name
    {

        get
        {
#if UNITY_EDITOR
            if (ParrelSync.ClonesManager.IsClone())
                return "EDITOR CLONE";
            else
                return "EDITOR HOST";
#else
            return PlayerPrefs.GetString("PlayerName", "Player");
#endif
        }
        set => PlayerPrefs.SetString("PlayerName", value);
    }

    public static string RecentIP
    {
        get => PlayerPrefs.GetString("RecentIP", "");
        set => PlayerPrefs.SetString("RecentIP", value);
    }

    public static float MainVolume
    {
        get => PlayerPrefs.GetFloat("MainVolume", 1f);
        set => PlayerPrefs.SetFloat("MainVolume", value);
    }

    public static int MaxFPS
    {
        get => PlayerPrefs.GetInt("MaxFPS");
        set => PlayerPrefs.SetInt("MaxFPS", value);
    }
    public static bool Fullscreen
    {
        get => PlayerPrefs.GetInt("Fullscreen", 0) == 1;
        set => PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);
    }

    public static int ResolutionWidth
    {
        get => PlayerPrefs.GetInt("ResWidth", 640);
        set => PlayerPrefs.SetInt("ResWidth", value);
    }

    public static int ResolutionHeight
    {
        get => PlayerPrefs.GetInt("ResHeight", 480);
        set => PlayerPrefs.SetInt("ResHeight", value);
    }

    public static void Load()
    {
        AudioListener.volume = MainVolume;
        Screen.SetResolution(ResolutionWidth, ResolutionHeight, Fullscreen, 0);
        PhotonNetwork.NickName = Name;
    }

}
