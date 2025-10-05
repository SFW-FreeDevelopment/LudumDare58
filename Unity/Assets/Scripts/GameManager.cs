using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager I;
    [SerializeField] float runSeconds = 180f;
    [SerializeField] int candy;
    [SerializeField] int multiplier = 1;
    float tLeft; bool running;

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
        running=false;
        //ResultsUI.I.Show(candy);
    }
    public bool IsRunning => running;
}
