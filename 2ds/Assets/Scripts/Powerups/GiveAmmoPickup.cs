using Gitmanik.Notification;
using UnityEngine;

public class GiveAmmoPickup : Pickupable
{
    public int magazineCount;
    public override void OnTrigger(Player player)
    {
        if (!player.photonView.IsMine || player.CurrentGun == null)
            return;

        player.CurrentGun.totalAmmo += player.CurrentGunSO.magazineCapacity * magazineCount;
        NotificationManager.Instance.RemoteSpawn($"{player.Nickname} has picked up AmmoPack!", Color.yellow - new Color(0, 0, 0, 0.4f), 1f);
        IngameHUDManager.Instance.UpdateAmmo();
        base.OnTrigger(player);
    }
}
