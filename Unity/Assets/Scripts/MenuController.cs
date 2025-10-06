using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Button PlayButton;

    private void Start()
    {
        PlayButton.onClick.AddListener(() => {
            LoadLevel();
        });
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            LoadLevel();
    }

    private void LoadLevel() => SceneManager.LoadScene("Game");
}
