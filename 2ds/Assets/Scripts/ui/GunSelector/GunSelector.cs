using UnityEngine;

public class GunSelector : MonoBehaviour
{
    [SerializeField] private GameObject element;
    [SerializeField] private GameObject gunSelectorTransform;

    public static GunSelector Instance;

    public bool Active { get { return gunSelectorTransform.gameObject.activeSelf; } }

    private void Start()
    {
        Instance = this;
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
        GameManager.Instance.LockInput = true;
        gunSelectorTransform.SetActive(true);
        Populate();
    }

    public void Disable()
    {
        GameManager.Instance.LockInput = false;
        gunSelectorTransform.SetActive(false);
    }

    public void Populate()
    {
        foreach (Transform child in gunSelectorTransform.transform)
        {
            Destroy(child.gameObject);
        }

        for (int a = 0; a < GameManager.Instance.Guns.Length; a++)
        {
            Instantiate(element, gunSelectorTransform.transform).GetComponent<GunSelectorElement>().Setup(this, GameManager.Instance.Guns[a], a);
        }
    }

    internal void Selected(Gun gun, int idx)
    {
        Player.Local.CmdSetGun(idx);
        Disable();
    }
}
