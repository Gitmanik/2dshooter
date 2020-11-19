﻿using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<Gun> Guns;

    [Header("Game Info")]
    public int GameVersion = 0;
    [SerializeField] private int MaxFPS = 150;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        DataManager.MaxFPS = MaxFPS;
        Application.targetFrameRate = DataManager.MaxFPS;
    }

    public bool LockInput;

    public Transform blackMask;
    public GameObject OptionsMenu;

    public void ToggleOptionsMenu(bool v)
    {
        OptionsMenu.SetActive(v);
        LockInput = v;
    }

    public void SetBlackMask(bool v) => blackMask.gameObject.SetActive(v);

    public AudioClip gunshot;
    public AudioClip reloadSound;
    public AudioClip noAmmoSound;
    public AudioClip hurtSound;
}