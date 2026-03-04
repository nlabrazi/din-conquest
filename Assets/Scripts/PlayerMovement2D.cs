using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Lecture clavier (Input System)
        var kb = Keyboard.current;
        if (kb == null)
        {
            moveInput = Vector2.zero;
        }
        else
        {
            float x = 0f;
            float y = 0f;

            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;

            moveInput = new Vector2(x, y).normalized;
        }

        // IMPORTANT: toujours mettre à jour Speed
        if (animator != null)
            animator.SetFloat("Speed", moveInput.sqrMagnitude);

        // Si on bouge, on mémorise la direction
        if (moveInput.sqrMagnitude > 0.001f)
        {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * speed;
    }
}