using Gitmanik.Notification;
using UnityEngine;

public class AddHealthPickup : Pickupable
{
    public int healthAmount;
    public override void OnTrigger(Player player)
    {
        if (!player.photonView.IsMine)
            return;
        player.Health += healthAmount;
        NotificationManager.Instance.RemoteSpawn($"{player.Nickname} has picked up HealthPack for {healthAmount}!", Color.yellow - new Color(0, 0, 0, 0.4f), 1f);
        base.OnTrigger(player);
    }
}
