// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;

// public class PlayerController : MonoBehaviour
// {
//     [SerializeField] private float moveSpeed;

//     [Header("Components")]
//     [SerializeField] private Rigidbody2D rig;
//     [SerializeField] private SpriteRenderer spriteRenderer;
//     [SerializeField] private MouseUtilities mouseUtilities;

//     //For distance tracking
//     private Vector3 lastPos;
//     private Vector2 moveInput;

//     void Start()
//     {
//         lastPos = transform.position;
//     }

//     void Update ()
//     {
//         // ---- Movement ----
//         Vector2 mouseDirection = mouseUtilities.GetMouseDirection(transform.position);
//         spriteRenderer.flipX = mouseDirection.x < 0;

//         // ---- Distance Tracking ----
//         Vector3 currentPos = transform.position;
//         float distanceMoved = Vector3.Distance(currentPos, lastPos);

//         if (PlayerProfile.Instance != null)
//             PlayerProfile.Instance.AddDistance(distanceMoved);

//         lastPos = currentPos;
//     }

//     void FixedUpdate ()
//     {
//         Vector2 velocity = moveInput * moveSpeed;
//         rig.linearVelocity = velocity;
//     }

//     public void OnMoveInput (InputAction.CallbackContext context)
//     {
//         moveInput = context.ReadValue<Vector2>();
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float sprintMultiplier = 1.75f;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rig;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private MouseUtilities mouseUtilities;

    // For distance tracking
    private Vector3 lastPos;
    private Vector2 moveInput;
    private bool isSprinting;

    void Start()
    {
        lastPos = transform.position;
    }

    void Update()
    {
        // ---- Movement / Facing ----
        Vector2 mouseDirection = mouseUtilities.GetMouseDirection(transform.position);
        spriteRenderer.flipX = mouseDirection.x < 0;

        // ---- Distance Tracking ----
        Vector3 currentPos = transform.position;
        float distanceMoved = Vector3.Distance(currentPos, lastPos);

        if (PlayerProfile.Instance != null)
            PlayerProfile.Instance.AddDistance(distanceMoved);

        lastPos = currentPos;
    }

    void FixedUpdate()
    {
        float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        Vector2 velocity = moveInput * currentSpeed;
        rig.linearVelocity = velocity;
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnSprintInput(InputAction.CallbackContext context)
    {
        if (context.started)
            isSprinting = true;
        else if (context.canceled)
            isSprinting = false;
    }

    public void OnUltraSprintInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isSprinting = true;
            sprintMultiplier += 10;
        } 
        else if (context.canceled)
        {
            isSprinting = false;
            sprintMultiplier -= 10;
        }  
    }
}