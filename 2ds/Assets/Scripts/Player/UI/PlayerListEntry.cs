using UnityEngine;

public class PlayerListEntry : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text nickname;
    [SerializeField] private TMPro.TMP_Text ping;
    [SerializeField] private TMPro.TMP_Text stats;

    public void Setup(Player p)
    {
        nickname.text = p.Nickname;
        ping.text = $"{p.Ping}ms";
        stats.text = $"{(int) p.photonView.Owner.CustomProperties["Kills"]}/{(int)p.photonView.Owner.CustomProperties["Deaths"]}";
    }
}