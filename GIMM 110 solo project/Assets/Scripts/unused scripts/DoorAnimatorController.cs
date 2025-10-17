using UnityEngine;
using System.Collections;

public class DoorAnimatorController : MonoBehaviour
{
    [Header("Animator Settings")]
    public Animator animator;          // Reference to Animator
    private bool isOpen = false;
    private bool isBroken = false;

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public float interactRange = 3f;
    public float autoCloseDelay = 3f;  // Time before door auto-closes
    public LayerMask playerLayer;

    private Transform player;
    private Coroutine autoCloseCoroutine;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (isBroken) return; // Can't interact after being broken

        HandlePlayerInteraction();
    }

    void HandlePlayerInteraction()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        if (distance <= interactRange && Input.GetKeyDown(interactKey))
        {
            ToggleDoor();
        }
    }

    void ToggleDoor()
    {
        isOpen = !isOpen;
        animator.SetBool("IsOpen", isOpen);

        if (isOpen)
        {
            if (autoCloseCoroutine != null)
                StopCoroutine(autoCloseCoroutine);

            autoCloseCoroutine = StartCoroutine(AutoClose());
        }
        else
        {
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;
            }
        }
    }

    IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        isOpen = false;
        animator.SetBool("IsOpen", false);
        autoCloseCoroutine = null;
    }

    public void OnShot()
    {
        if (isBroken) return;

        isBroken = true;
        animator.SetTrigger("Broken");

        // Cancel auto-close if it's running
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            OnShot();
            Destroy(collision.gameObject); // Optional: destroy bullet
        }
    }
}

