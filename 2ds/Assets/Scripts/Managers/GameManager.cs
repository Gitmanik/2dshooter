using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Application.targetFrameRate = MaxFPS;
    }

    public Transform blackMask;

    public void SetBlackMask(bool v) => blackMask.gameObject.SetActive(v);

    public AudioClip reloadSound;
    public AudioClip noAmmoSound;
    public AudioClip hurtSound;

    public AudioClip[] footsteps;

    public AudioClip footstep { get => footsteps[Random.Range(0, footsteps.Length)]; }

    public static bool CheckScene()
    {
        if (Instance == null)
        {
            Debug.LogWarning("No GameManager Instance found.");
            SceneManager.LoadScene("Preloader");
            return false;
        }
        return true;
    }
}