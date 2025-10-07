using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultsWindow : MonoBehaviour
{
    [Header("Refs")]
    public GameObject root;           // the whole panel
    public TMP_Text resultText;       // e.g., "You collected 37 candies!"
    public Button returnButton;       // "Return to Main Menu"

    System.Action onReturn;
    
    public void Setup()
    {
        if (root) root.SetActive(false);
        if (returnButton) returnButton.onClick.AddListener(() => {
            onReturn?.Invoke();
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            onReturn?.Invoke();
    }

    /// <summary>Show the results window.</summary>
    public void Show(int candyTotal, System.Action onReturnToMenu)
    {
        onReturn = onReturnToMenu;
        if (resultText) resultText.text = $"{candyTotal}";
        gameObject.SetActive(true);
    }

    /// <summary>Hide without invoking the return callback.</summary>
    public void Hide()
    {
        if (root) root.SetActive(false);
        onReturn = null;
    }
}
