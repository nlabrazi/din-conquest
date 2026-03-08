using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NpcWander2D : MonoBehaviour
{
    [Header("Area")]
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Pokemon-style pacing")]
    [SerializeField] private int minSteps = 1;
    [SerializeField] private int maxSteps = 3;
    [SerializeField] private float stepDuration = 0.18f;
    [SerializeField] private float pauseDuration = 1.0f;
    [SerializeField] private float speed = 1.2f;

    [Header("Grid (optional)")]
    [SerializeField] private bool snapToGrid = true;
    [SerializeField] private float gridSize = 1f;

    [Header("Obstacle check")]
    [SerializeField] private float checkDistance = 0.35f;

    public bool IsPausedExternally { get; set; } = false;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 homePos;
    private Vector2 dir = Vector2.down;

    private enum State { Pause, Walk }
    private State state = State.Pause;

    private float stateTimer = 0f;
    private int stepsRemaining = 0;

    private Vector2 lastMoveDir = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        homePos = rb.position;
        EnterPause();
    }

    private void Update()
    {
        if (animator != null)
        {
            bool isMoving = !IsPausedExternally && state == State.Walk;
            animator.SetFloat("Speed", isMoving ? 1f : 0f);
            animator.SetFloat("MoveX", lastMoveDir.x);
            animator.SetFloat("MoveY", lastMoveDir.y);
        }
    }

    private void FixedUpdate()
    {
        if (IsPausedExternally)
        {
            rb.MovePosition(rb.position);
            return;
        }

        stateTimer -= Time.fixedDeltaTime;

        if (state == State.Pause)
        {
            rb.MovePosition(rb.position);

            if (stateTimer <= 0f)
                EnterWalk();

            return;
        }

        if (IsBlocked(dir))
        {
            EnterPause(blocked: true);
            return;
        }

        Vector2 nextPos = rb.position + dir * speed * Time.fixedDeltaTime;
        if (Vector2.Distance(homePos, nextPos) > wanderRadius)
        {
            EnterPause(blocked: true);
            return;
        }

        rb.MovePosition(nextPos);

        if (stateTimer <= 0f)
        {
            stepsRemaining--;

            if (stepsRemaining <= 0)
            {
                if (snapToGrid)
                    rb.MovePosition(Snap(rb.position));

                EnterPause();
            }
            else
            {
                stateTimer = stepDuration;
            }
        }
    }

    private void EnterPause(bool blocked = false)
    {
        state = State.Pause;
        stateTimer = blocked ? Mathf.Max(0.5f, pauseDuration * 0.7f) : pauseDuration;
    }

    private void EnterWalk()
    {
        state = State.Walk;

        dir = PickDirection4Way();
        lastMoveDir = dir;

        stepsRemaining = Random.Range(minSteps, maxSteps + 1);
        stateTimer = stepDuration;

        if (IsBlocked(dir))
            EnterPause(blocked: true);
    }

    private Vector2 PickDirection4Way()
    {
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        for (int i = 0; i < 8; i++)
        {
            Vector2 candidate = dirs[Random.Range(0, dirs.Length)];

            if (IsBlocked(candidate))
                continue;

            Vector2 next = rb.position + candidate * 0.5f;
            if (Vector2.Distance(homePos, next) > wanderRadius)
                continue;

            return candidate;
        }

        return Vector2.zero;
    }

    private bool IsBlocked(Vector2 d)
    {
        if (d == Vector2.zero) return true;

        return Physics2D.CircleCast(rb.position, 0.1f, d, checkDistance, obstacleMask);
    }

    private Vector2 Snap(Vector2 pos)
    {
        if (!snapToGrid || gridSize <= 0f) return pos;

        float x = Mathf.Round(pos.x / gridSize) * gridSize;
        float y = Mathf.Round(pos.y / gridSize) * gridSize;
        return new Vector2(x, y);
    }
}