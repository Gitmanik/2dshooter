using Gitmanik.Notification;
using UnityEngine;

public class GiveAmmoPickup : Pickupable
{
    public int magazineCount;
    public override void OnTrigger(Player player)
    {
        if (!player.photonView.IsMine)
            return;



        //GunData newgundata = player.inventory.CurrentGunData;
        //newgundata.totalAmmo += player.inventory.CurrentGun.magazineCapacity * magazineCount;
        //player.inventory.CurrentGunData = newgundata;
        NotificationManager.Instance.RemoteSpawn($"{player.Nickname} has picked up AmmoPack!", Color.yellow - new Color(0, 0, 0, 0.4f), 1f);
        base.OnTrigger(player);
    }
}
