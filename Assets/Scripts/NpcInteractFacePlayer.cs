using UnityEngine;
using UnityEngine.InputSystem;

public class NpcInteractFacePlayer : MonoBehaviour
{
    [SerializeField] private NpcWander2D wander;
    [SerializeField] private Transform player;

    private bool playerInRange;

    private void Reset()
    {
        wander = GetComponent<NpcWander2D>();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            // stop wander + face player
            if (wander != null) wander.Pause(true);

            if (player != null && wander != null)
            {
                Vector2 dir = (player.position - transform.position);
                // on veut la direction principale (4 dirs)
                dir = SnapToCardinal(dir);
                wander.FaceDirection(dir);
            }

            Debug.Log("NPC: (dialogue bientôt)");
        }
    }

    private Vector2 SnapToCardinal(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x >= 0 ? Vector2.right : Vector2.left;
        return dir.y >= 0 ? Vector2.up : Vector2.down;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;

        // quand le joueur s'éloigne, il reprend sa vie
        if (wander != null) wander.Pause(false);
    }
}