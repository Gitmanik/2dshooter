using UnityEngine;

public class GunSelector : MonoBehaviour
{
    [SerializeField] private GameObject element;
    [SerializeField] private Transform gunSelectorTransform;
    
    public bool Active { get { return gunSelectorTransform.gameObject.activeSelf; } }

    private void Start()
    {
        Game.Instance.GunSelector = this;
    }

    public void Toggle()
    {
        if (Active)
            Disable();
        else
            Enable();
    }

    public void Enable()
    {
        Game.Instance.LockInput = true;
        gunSelectorTransform.gameObject.SetActive(true);
        Populate();
    }

    public void Disable()
    {
        Game.Instance.LockInput = false;
        gunSelectorTransform.gameObject.SetActive(false);
    }

    public void Populate()
    {
        foreach (Transform child in gunSelectorTransform)
        {
            Destroy(child.gameObject);
        }
        foreach (Gun gun in Game.Instance.Guns)
        {
            Instantiate(element, gunSelectorTransform).GetComponent<GunSelectorElement>().Setup(this, gun);
        }
    }

    internal void Selected(Gun gun)
    {
        Debug.Log($"Selected {gun.name}");
        Game.Instance.player.SetGun(gun);
        Disable();
    }
}
