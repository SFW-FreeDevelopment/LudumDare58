using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager I;

    [Header("Run Settings")]
    [SerializeField] float runSeconds = 180f;

    [Header("Score State")]
    [SerializeField] int candy;
    [SerializeField] int multiplier = 1;

    [Header("UI")]
    [SerializeField] ResultsWindow resultsWindow;   // assign in Inspector
    [SerializeField] GameObject mainHUDRoot;        // ← your timer/helper text parent
    [SerializeField] string mainMenuSceneName = "MainMenu";

    float tLeft;
    bool running;

    void Awake(){ I=this; }
    void Start(){ tLeft = runSeconds; running = true; }

    void Update(){
        if(!running) return;
        tLeft -= Time.deltaTime;
        UIHud.I.SetTimer(tLeft / runSeconds, Mathf.CeilToInt(tLeft));
        if(tLeft <= 0f) EndRun();
    }

    public void AddCandy(int amt){
        candy += amt * multiplier;
        UIHud.I.SetCandy(candy);
    }

    public void AddMultiplier(int delta){
        multiplier = Mathf.Max(1, multiplier + delta);
        UIHud.I.SetMultiplier(multiplier);
    }

    public void EndRun()
    {
        if (!running) return;
        running = false;
        
        AudioManager.I?.PlayGameOver(); 

        // Exit POV or minigame cleanly if needed
        if (GameController.I != null && GameController.I.State == GameState.DoorPOV)
            GameController.I.ExitDoorPOV();

        // Freeze player control
        if (GameController.I != null && GameController.I.player != null)
            GameController.I.player.EnableControl(false);

        // 🔸 Hide main HUD (timer, helper text, etc.)
        if (mainHUDRoot != null)
            mainHUDRoot.SetActive(false);

        // Show results window
        if (resultsWindow != null)
        {
            resultsWindow.Setup();
            resultsWindow.Show(candy, () =>
            {
                // Restore HUD in case of replay / next scene
                if (mainHUDRoot != null)
                    mainHUDRoot.SetActive(true);

                if (!string.IsNullOrEmpty(mainMenuSceneName))
                    SceneManager.LoadScene(mainMenuSceneName);
                else
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
        }
        else
        {
            Debug.LogWarning("[GameManager] ResultsWindow not assigned; reloading scene instead.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }


    public bool IsRunning => running;

    // (Optional) expose totals if you want other UI to query
    public int CandyTotal => candy;
    public int Multiplier => multiplier;
}
