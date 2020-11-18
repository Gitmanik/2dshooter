using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public List<ParticleEntry> Particles;
    public static ParticleManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private static GameObject GetParticleObj(EParticleType type)
    {
        GameObject[] g = Instance.Particles.Find(x => x.ParticleType == type).gameObj;
        return g[UnityEngine.Random.Range(0, g.Length - 1)];
    }

    public static void Spawn(EParticleType type, Vector3 pos)
    {
        Instantiate(GetParticleObj(type), pos, Quaternion.identity);
    }

    public static void Spawn(EParticleType type, Transform parent)
    {
        Instantiate(GetParticleObj(type), parent);
    }
}

public enum EParticleType
{
    SHOOT,
    BLOOD
}

[Serializable]
public struct ParticleEntry
{
    public EParticleType ParticleType;
    public GameObject[] gameObj;
}