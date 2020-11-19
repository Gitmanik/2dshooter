using UnityEngine;

public class IngameHUDManager : MonoBehaviour
{
    [Header("Alive")]
    [SerializeField] private GameObject alivePanel;
    [SerializeField] private TMPro.TMP_Text healthText;
    [SerializeField] private TMPro.TMP_Text ammoText;

    [Header("Dead")]
    [SerializeField] private GameObject deadPanel;
    [SerializeField] private TMPro.TMP_Text killedBy;

    private Player owner;

    public void Setup(Player o)
    {
        owner = o;
        UpdateHealth();
        UpdateAmmo();
    }

    public void ToggleAlive(bool v)
    {
        alivePanel.SetActive(v);
        deadPanel.SetActive(!v);
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

    public void UpdateKilledBy(string by)
    {
        killedBy.text = $"You were killed by: {by}";
    }
}
