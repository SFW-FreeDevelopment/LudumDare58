using UnityEngine;

public class BucketCollector : MonoBehaviour
{
    private CandyCatchMiniGame owner;

    public void Init(CandyCatchMiniGame game) => owner = game;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!owner) return;

        // Recognize candies by marker (preferred) or tag
        if (other && other.CompareTag("Candy"))
        {
            AudioManager.I?.PlayCandy(other.GetComponent<CandyMarker>().IsCrinkly);
            owner.NotifyCandyCaught(other.gameObject);
        }
    }
}
