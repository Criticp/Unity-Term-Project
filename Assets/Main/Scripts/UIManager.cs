using UnityEngine;

public class UIManager : MonoBehaviour
{
    void Start()
    {
        // Make the cursor invisible
        Cursor.visible = false;

        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
    }
}
