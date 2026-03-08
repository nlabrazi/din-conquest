using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Grid movement")]
    [SerializeField] private float gridSize = 1f;                 // 1 = 1 case Unity
    [SerializeField] private float speed = 3.5f;                  // unités/sec
    [SerializeField] private LayerMask obstacleMask;              // Obstacles + NPC
    [SerializeField] private float collisionCheckRadius = 0.18f;  // ajuste selon tes colliders

    private Rigidbody2D rb;
    private Animator animator;

    private bool isMoving;
    private Vector2 moveDir = Vector2.down;
    private Vector2 targetPos;
    public bool CanMove { get; set; } = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        targetPos = rb.position;
    }

    private void Update()
    {
        if (!CanMove)
        {
            SetAnimIdle();
            return;
        }
        // si on est déjà en train de bouger -> on ignore les inputs (style Pokémon)
        if (isMoving) return;

        Vector2 input = ReadInputNoDiagonal();
        if (input == Vector2.zero)
        {
            SetAnimIdle();
            return;
        }

        // prépare le prochain pas
        moveDir = input;
        Vector2 desiredTarget = SnapToGrid(rb.position + moveDir * gridSize);

        // si la case est bloquée -> ne démarre pas le pas (bump)
        if (Physics2D.OverlapCircle(desiredTarget, collisionCheckRadius, obstacleMask))
        {
            // On oriente le perso vers l'obstacle puis on reste idle
            SetAnimMoving(moveDir);
            SetAnimIdle();
            return;
        }

        targetPos = desiredTarget;
        SetAnimMoving(moveDir);
        isMoving = true;
    }

    private void FixedUpdate()
    {
        // Si on ne marche pas, on neutralise toute poussée physique (anti-glisse)
        if (!isMoving)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 before = rb.position;

        Vector2 newPos = Vector2.MoveTowards(before, targetPos, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        // Si on n'a pas bougé => bloqué => on annule le pas (anti moonwalk infini)
        if (Vector2.Distance(before, rb.position) <= 0.0001f)
        {
            isMoving = false;
            SetAnimIdle();
            return;
        }

        // Arrivé à la case cible
        if (Vector2.Distance(rb.position, targetPos) <= 0.001f)
        {
            rb.MovePosition(targetPos);
            isMoving = false;
            SetAnimIdle();
        }
    }

    private Vector2 ReadInputNoDiagonal()
    {
        var kb = Keyboard.current;
        if (kb == null) return Vector2.zero;

        int x = 0, y = 0;

        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x = -1;
        else if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x = 1;

        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y = 1;
        else if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y = -1;

        // pas de diagonale : priorité horizontale si x != 0, sinon verticale
        if (x != 0) return new Vector2(x, 0);
        if (y != 0) return new Vector2(0, y);
        return Vector2.zero;
    }

    private Vector2 SnapToGrid(Vector2 pos)
    {
        float x = Mathf.Round(pos.x / gridSize) * gridSize;
        float y = Mathf.Round(pos.y / gridSize) * gridSize;
        return new Vector2(x, y);
    }

    private void SetAnimMoving(Vector2 dir)
    {
        if (animator == null) return;

        animator.SetFloat("Speed", 1f);
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
    }

    private void SetAnimIdle()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", 0f);
        // MoveX/MoveY restent sur la dernière direction
    }

#if UNITY_EDITOR
    // Juste pour visualiser le cercle de check dans la scène
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.DrawWireSphere(targetPos, collisionCheckRadius);
    }
#endif
}