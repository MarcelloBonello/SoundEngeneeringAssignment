using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class StartQuitManager : MonoBehaviour
{

    [Header("Input Actions")]
    [SerializeField] private InputActionReference quitGameAction;
    public void switchToGame()
    {
        SceneManager.LoadScene("Game");
    }
    private void OnEnable()
    {
        if (quitGameAction != null)
        {
            quitGameAction.action.performed += OnQuitGame;
            quitGameAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (quitGameAction != null)
        {
            quitGameAction.action.performed -= OnQuitGame;
            quitGameAction.action.Disable();
        }
    }

     private void OnQuitGame(InputAction.CallbackContext context)
       {
        Debug.Log("[StartQuitManager] Quit Game requested.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
