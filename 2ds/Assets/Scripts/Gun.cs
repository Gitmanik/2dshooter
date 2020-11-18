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
}
