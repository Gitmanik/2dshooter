using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class Gun : ScriptableObject
{
    public SkinIndex SkinIndex;
    public string title, desc;
    public Sprite uiSprite;
    public float viewRadius, viewAngle;

    public bool autofire;
    public float reloadTime;

    public int magazineCount;
    public int magazineCapacity;

    public bool melee;
    public AnimationCurve damageCurve;
    public float firerate;
    public float damage;

    public AudioClip shootSount;

    public GunHolder GetGunHolder()
    {
        return new GunHolder
        {
            totalAmmo = magazineCapacity * (magazineCount - 1),
            currentAmmo = magazineCapacity,
            gunIndex = (byte) GameManager.Instance.Guns.IndexOf(this)
        };
    }
}

[Serializable]
public class GunHolder
{
    public byte gunIndex;
    public int totalAmmo;
    public int currentAmmo;

    public static object Deserialize(byte[] data)
    {
        var result = new GunHolder();
        result.gunIndex = data[0];
        result.totalAmmo = BitConverter.ToInt32(data, 1);
        result.currentAmmo = result.totalAmmo = BitConverter.ToInt32(data, 4);
        return result;
    }

    public static byte[] Serialize(object customType)
    {
        List<byte> bytes = new List<byte>();
        var c = (GunHolder)customType;
        bytes.AddRange(new byte[] { c.gunIndex });
        bytes.AddRange(BitConverter.GetBytes(c.totalAmmo));
        bytes.AddRange(BitConverter.GetBytes(c.currentAmmo));
        return bytes.ToArray();
    }
}
public static class Extensions
{
    public static T[] SubArray<T>(this T[] array, int offset, int length)
    {
        T[] result = new T[length];
        Array.Copy(array, offset, result, 0, length);
        return result;
    }
}