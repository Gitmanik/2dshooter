using UnityEngine;

public class IngameHUDManager : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text healthText;
    [SerializeField] private TMPro.TMP_Text ammoText;

    public void UpdateHealth(float value)
    {
        healthText.text = $"{value} HP";
    }

    public void UpdateAmmo(Gun gun)
    {
        ammoText.text = $"{gun.currentAmmoInMagazine}/{gun.ammoInMagazine} ({gun.magazineCount})";
    }
}
