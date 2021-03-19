using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Preloader : MonoBehaviour
{
    [SerializeField] private Slider loadingBar;
    [Scene] [SerializeField] private string SceneName;

    void Start()
    {
        DataManager.Load();
        loadingBar.value = 1;
        SceneManager.LoadScene(SceneName);
    }
}
