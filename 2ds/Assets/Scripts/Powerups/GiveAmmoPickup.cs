using Gitmanik.Networked;
using UnityEngine;

public class GiveAmmoPickup : Pickupable
{
    public int magazineCount;
    public override void OnTrigger(Player player)
    {
        if (!player.inventory.HasAnyGun)
            return;

        base.OnTrigger(player);
        GunData newgundata = player.inventory.CurrentGunData;
        newgundata.totalAmmo += player.inventory.CurrentGun.magazineCapacity * magazineCount;
        player.inventory.CurrentGunData = newgundata;
        NetworkedNotification.Spawn($"{player.Nickname} has picked up AmmoPack!", Color.yellow - new Color(0, 0, 0, 0.4f), 1f);
    }
}
