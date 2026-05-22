using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CastleDoorWay : MonoBehaviour
{
    [Tooltip("Name of the scene to load (must be added to Build Settings).")]
    [SerializeField]
    private string sceneName = "DungeonScene";

    [Tooltip("Tag used to identify the player GameObject.")]
    [SerializeField]
    private string playerTag = "Player";

    [Tooltip("Reference to the Interact Input Action (from an Input Actions asset).")]
    [SerializeField]
    private InputActionReference Interact;

    private bool _playerInRange;

    void Reset()
    {
        var col = GetComponent<BoxCollider2D>() ?? gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player entered");
            _playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player exited");
            _playerInRange = false;
        }
    }

    void OnEnable()
    {
        if (Interact != null && Interact.action != null)
        {
            Interact.action.performed += OnInteract;
            Interact.action.Enable();
        }
    }

    void OnDisable()
    {
        if (Interact != null && Interact.action != null)
        {
            Interact.action.performed -= OnInteract;
            Interact.action.Disable();
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log("Interact action performed");
        if (_playerInRange && !string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}