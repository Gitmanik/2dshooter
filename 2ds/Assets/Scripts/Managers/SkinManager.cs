using System;
using UnityEngine;

[Serializable]
public class Skin
{
    public Sprite[] Sprites;
    public string Name;
}

public enum SkinIndex
{
    STAND,
    HOLD,
    GUN,
    SILENCER,
    MACHINE,
    RELOAD
}

public class SkinManager : MonoBehaviour
{
    public Skin[] AllSkins;

    public static SkinManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    internal Sprite GetSprite(int playerIndex, SkinIndex gunIndex) => AllSkins[playerIndex].Sprites[(int) gunIndex];
}
