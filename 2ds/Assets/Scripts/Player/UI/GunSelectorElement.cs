using UnityEngine;
using UnityEngine.UI;

public class GunSelectorElement : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMPro.TMP_Text text;

    private IngameHUDManager parent;
    private int idx;
    
    public void Setup(IngameHUDManager parent, GunHolder gun, int idx)
    {
        this.idx = idx;
        this.parent = parent;
        transform.name = GameManager.Instance.Guns[gun.gunIndex].title;
        image.sprite = GameManager.Instance.Guns[gun.gunIndex].uiSprite;
        text.text = GameManager.Instance.Guns[gun.gunIndex].title;
    }
    public void OnClick()
    {
        parent.GunSelectorSelected(idx);
    }
}
