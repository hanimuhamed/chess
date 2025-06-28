using UnityEngine;

public class RestartButton : MonoBehaviour
{
    public GameManager gameManager;
    void OnMouseDown()
    {
        gameManager.playAsWhite = !gameManager.playAsWhite; // Toggle play as white
        gameManager.Restart();
    }

    private float scale = 0.11111f;
    private void OnMouseEnter()
    {
        transform.localScale = Vector3.one * (1.0f + scale);
    }

    private void OnMouseExit()
    {
        transform.localScale = Vector3.one;
    }
}
