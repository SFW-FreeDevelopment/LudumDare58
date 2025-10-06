using UnityEngine;
using UnityEngine.UI;

public class ButtonClick : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => {
            AudioManager.I?.PlayClick();
        });       
    }
}