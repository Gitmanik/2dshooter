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

    public static string RecentRoomName
    {
        get => PlayerPrefs.GetString("RecentRoomName", "");
        set => PlayerPrefs.SetString("RecentRoomName", value);
    }

    public static float MainVolume
    {
        get => PlayerPrefs.GetFloat("MainVolume", 1f);
        set => PlayerPrefs.SetFloat("MainVolume", value);
    }
    
    public static void Load()
    {
        AudioListener.volume = MainVolume;
        PhotonNetwork.NickName = Name;
    }

}
