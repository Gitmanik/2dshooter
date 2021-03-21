using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Sprite[] PlayerSkins;

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

    public Transform blackMask;

    public void SetBlackMask(bool v) => blackMask.gameObject.SetActive(v);

    public AudioClip reloadSound;
    public AudioClip noAmmoSound;
    public AudioClip hurtSound;

    public static bool CheckScene()
    {
        if (Instance == null)
        {
            SceneManager.LoadScene("Preloader");
            return false;
        }
        return true;
    }
}