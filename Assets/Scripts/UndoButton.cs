using UnityEngine;

public class UndoButton : MonoBehaviour
{
    public GameManager gameManager;
    void OnMouseDown()
    {
        gameManager.Undo();
    }
}
