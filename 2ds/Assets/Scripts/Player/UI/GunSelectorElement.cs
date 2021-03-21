using UnityEngine;
using UnityEngine.UI;

public class GunSelectorElement : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMPro.TMP_Text text;

    private IngameHUDManager parent;
    private int idx;
    public void Setup(IngameHUDManager parent, Gun gun, int idx)
    {
        this.idx = idx;
        this.parent = parent;
        transform.name = gun.title;
        image.sprite = gun.uiSprite;
        text.text = gun.title;
    }
    public void OnClick()
    {
        parent.GunSelectorSelected(idx);
    }
}
