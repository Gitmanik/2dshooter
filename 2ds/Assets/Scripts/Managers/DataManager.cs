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
        set => PlayerPrefs.GetFloat("MainVolume", value);
    }

    public static int MaxFPS
    {
        get => PlayerPrefs.GetInt("MaxFPS");
        set => PlayerPrefs.SetInt("MaxFPS", value);
    }
}
