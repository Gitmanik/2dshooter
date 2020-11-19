using System;
using UnityEngine;
[CreateAssetMenu()]
public class Gun : ScriptableObject
{
    public string title, desc;
    public Sprite sprite;
    public float viewRadius, viewAngle;

    public int magazineCount;
    public int magazineCapacity;

    public AnimationCurve damageCurve;
    public float firerate;
    public float damage;


    public GunData GenerateGunData()
    {
        return new GunData
        {
            totalAmmo = magazineCapacity * (magazineCount - 1),
            currentAmmo = magazineCapacity,
            gunIndex = GameManager.Instance.Guns.IndexOf(this)
        };
    }
}

[Serializable]
public class GunData
{
    public int gunIndex;
    public int totalAmmo;
    public int currentAmmo;
}
