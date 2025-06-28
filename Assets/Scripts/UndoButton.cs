using UnityEngine;

public class UndoButton : MonoBehaviour
{
    public GameManager gameManager;
    void OnMouseDown()
    {
        if (GameManager.gameOver) return;
        gameManager.Undo();
    }

    private float scale = 0.11111f;
    private void OnMouseEnter()
    {
        if (GameManager.gameOver) return;
        transform.localScale = Vector3.one * (1.0f + scale);
    }

    private void OnMouseExit()
    {
        if (GameManager.gameOver) return;
        transform.localScale = Vector3.one;
    }
}
