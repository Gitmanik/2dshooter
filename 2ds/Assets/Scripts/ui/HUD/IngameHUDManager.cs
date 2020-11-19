using UnityEngine;

public class IngameHUDManager : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text healthText;
    [SerializeField] private TMPro.TMP_Text ammoText;

    private Player owner;

    public void Setup(Player o)
    {
        owner = o;
        UpdateHealth();
        UpdateAmmo();
    }

    public void UpdateHealth()
    {
        healthText.text = $"{owner.health} HP";
    }

    public void UpdateAmmo()
    {
        if (!owner.i.HasAnyGun)
        {
            ammoText.text = "--";
        }
        ammoText.text = $"{owner.i.CurrentGunData.currentAmmo}/{owner.i.CurrentGun.magazineCapacity} ({owner.i.CurrentGunData.totalAmmo})";
    }
}
