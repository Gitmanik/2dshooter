using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }
    public float targetAspect;

    public Gun[] Guns;

    private void Awake()
    {
        Instance = this;

        foreach (Gun g in Guns)
        {
            g.totalAmmo = g.ammoInMagazine * g.magazineCount;
            g.currentAmmoInMagazine = g.ammoInMagazine;
        }
    }
    public Player player;
    public GunSelector GunSelector;
    public bool LockInput;

    public AudioClip gunshot;
    public AudioClip reloadSound;
}

[System.Serializable]
public class Gun
{
    public string name, desc;
    public Sprite sprite;
    public float viewRadius, viewAngle;
    public float magazineCount, ammoInMagazine;
    [HideInInspector] public float totalAmmo;
    [HideInInspector] public float currentAmmoInMagazine;


    public AnimationCurve damageCurve;
    public float firerate;
    public float damage;
}

//public class Character
//{
//    public Sprite sprite;
//    public string Name;

//}
