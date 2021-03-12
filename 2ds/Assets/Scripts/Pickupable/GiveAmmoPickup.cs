using UnityEngine;

public class GiveAmmoPickup : Pickupable
{
    public int magazineCount;
    public override void OnTrigger(Player player)
    {
        if (!player.i.HasAnyGun)
            return;

        base.OnTrigger(player);
        GunData newgundata = player.i.CurrentGunData;
        newgundata.totalAmmo += player.i.CurrentGun.magazineCapacity * magazineCount;
        player.i.CurrentGunData = newgundata;
        CustomNetworkManager.instance.SpawnNotification($"{player.info.Nickname} has picked up AmmoPack!", Color.yellow - new Color(0, 0, 0, 0.4f), 1f);
    }
}
