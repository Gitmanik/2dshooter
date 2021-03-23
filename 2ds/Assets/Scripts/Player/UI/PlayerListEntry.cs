using UnityEngine;

public class PlayerListEntry : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text nickname;
    [SerializeField] private TMPro.TMP_Text ping;
    [SerializeField] private TMPro.TMP_Text stats;

    public void Setup(Player p)
    {
        nickname.text = p.Nickname;
        ping.text = $"{p.ping}ms";
        stats.text = $"{p.killCount}/{p.deathCount}";
    }
}