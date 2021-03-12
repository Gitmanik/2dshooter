using UnityEngine;

public class AddHealthPickup : Pickupable
{
    public int healthAmount;
    public override void OnTrigger(Player player)
    {
        base.OnTrigger(player);
        player.health += healthAmount;
        CustomNetworkManager.instance.SpawnNotification($"{player.info.Nickname} has picked up HealthPack for {healthAmount}!", Color.yellow - new Color(0, 0, 0, 0.4f), 1f);
    }
}
