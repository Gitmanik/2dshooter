using UnityEngine;
using UnityEngine.UI;

public class GunSelectorElement : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMPro.TMP_Text text;

    private Gun gun;
    private GunSelector parent;
    private int idx;
    public void Setup(GunSelector parent, Gun gun, int idx)
    {
        this.idx = idx;
        this.gun = gun;
        this.parent = parent;
        transform.name = gun.title;
        image.sprite = gun.sprite;
        text.text = gun.title;
    }
    public void OnClick()
    {
        parent.Selected(gun, idx);
    }
}
