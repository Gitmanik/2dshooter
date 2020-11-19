using UnityEngine;

public class PlayerListEntry : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text nickname;
    [SerializeField] private TMPro.TMP_Text ping;
    [SerializeField] private TMPro.TMP_Text stats;

    public void Setup(PlayerInformation info)
    {
        nickname.text = info.Nickname;
        //ping.text = $"{(int)(NetworkTime.rtt * 1000)}ms";
        stats.text = $"{info.killCount}/{info.deathCount}";
    }
}