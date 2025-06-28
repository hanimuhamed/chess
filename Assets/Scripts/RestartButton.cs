using UnityEngine;

public class RestartButton : MonoBehaviour
{
    public GameManager gameManager;
    void OnMouseDown()
    {
        gameManager.playAsWhite = !gameManager.playAsWhite; // Toggle play as white
        gameManager.Restart();
    }
}
