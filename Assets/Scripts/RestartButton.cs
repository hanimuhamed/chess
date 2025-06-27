using UnityEngine;

public class RestartButton : MonoBehaviour
{
    public GameManager gameManager;
    void OnMouseDown()
    {
        gameManager.Restart();
    }
}
