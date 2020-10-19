using UnityEngine;
using UnityEngine.UI;

public class GunSelectorElement : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMPro.TMP_Text text;

    private Gun gun;
    private GunSelector parent;
    public void Setup(GunSelector parent, Gun gun)
    {
        this.gun = gun;
        this.parent = parent;
        transform.name = gun.name;
        image.sprite = gun.sprite;
        text.text = gun.name;
    }
    public void OnClick()
    {
        parent.Selected(gun);
    }
}
