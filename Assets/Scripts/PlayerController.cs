using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float walkSpeed = 3f; // Renamed from movementSpeed
    public float runSpeed = 6f; // Speed when running
    public GameObject bombPrefab; // Reference to the bomb prefab
    public float bombExplosionRadius = 2f; // Radius of the bomb explosion
    public float bombCooldown = 3f; // Cooldown time for bomb spawning
    public float bombFuseTime = 2.417f; // Fuse time for the bomb
    public float destructionTime = 1f;
    public LayerMask explosionLayerMask; // Layer mask for explosion detection
    public Tilemap destructibleTiles; // Tilemap for destructible tiles
    public GameObject destructiblePrefab; // Prefab for destructible objects

    private Rigidbody2D rb;
    private Animator animator;
    private float bombCooldownTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bombCooldownTimer = 0f;
    }

    void Update()
    {
        HandleMovement();
        HandleBombSpawn();
        UpdateBombCooldown();
    }

    void HandleMovement()
    {
        Vector2 movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (movementInput != Vector2.zero)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift); // Check if the run key is pressed
            MovePlayer(movementInput, isRunning);
            UpdateAnimation(movementInput);
        }
        else
        {
            rb.velocity = Vector2.zero; // Stop the player when no input is detected
            animator.SetBool("isWalking", false);
            if (animator.HasState(0, Animator.StringToHash("Idle_down")))
            {
                animator.Play("Idle_down"); // Play idle animation when not moving
            }
        }
    }

    void MovePlayer(Vector2 direction, bool isRunning)
    {
        direction.Normalize();

        // Determine the speed based on whether the player is running
        float speed = isRunning ? runSpeed : walkSpeed;

        // Move the player with the specified speed using Rigidbody2D
        rb.velocity = direction * speed;

        Debug.Log($"Moving in direction {direction}, running: {isRunning}");
    }

    void UpdateAnimation(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            animator.SetBool("isWalking", true);
            if (direction.x > 0)
            {
                if (animator.HasState(0, Animator.StringToHash("walkRight")))
                {
                    animator.Play("walkRight"); // Play the right animation
                }
                transform.localScale = new Vector3(-0.8f, 0.8f, 0); // Mirror the character to face right
            }
            else if (direction.x < 0)
            {
                if (animator.HasState(0, Animator.StringToHash("walkLeft")))
                {
                    animator.Play("walkLeft"); // Play the left animation
                }
                transform.localScale = new Vector3(0.8f, 0.8f, 0); // Ensure the character faces left
            }
            else if (direction.y > 0)
            {
                if (animator.HasState(0, Animator.StringToHash("walkUp")))
                {
                    animator.Play("walkUp");
                }
            }
            else if (direction.y < 0)
            {
                if (animator.HasState(0, Animator.StringToHash("walkDown")))
                {
                    animator.Play("walkDown");
                }
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
            if (animator.HasState(0, Animator.StringToHash("Idle_down")))
            {
                animator.Play("Idle_down"); // Play idle animation when not moving
            }
        }
    }

    void HandleBombSpawn()
    {
        if (Input.GetKeyDown(KeyCode.Space) && bombCooldownTimer <= 0f)
        {
            StartCoroutine(PlaceBomb());
        }
    }

    void UpdateBombCooldown()
    {
        if (bombCooldownTimer > 0f)
        {
            bombCooldownTimer -= Time.deltaTime;
        }
    }

    private IEnumerator PlaceBomb()
    {
        Vector2 position = transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        GameObject bomb = Instantiate(bombPrefab, position, Quaternion.identity);

        // Play bomb animation if it has an Animator component
        Animator bombAnimator = bomb.GetComponent<Animator>();
        if (bombAnimator != null)
        {
            bombAnimator.Play("BombAnimation"); // Replace "BombAnimation" with the actual animation name
        }

        yield return new WaitForSeconds(bombFuseTime);

        position = bomb.transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        Explode(position);

        Destroy(bomb);
        bombCooldownTimer = bombCooldown; // Reset the cooldown timer
    }

    private void Explode(Vector2 position)
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(position, new Vector2(bombExplosionRadius, bombExplosionRadius), 0, explosionLayerMask);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                EnemyController enemyController = collider.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.DestroyEnemy();
                }
            }
            else
            {
                ClearDestructible(collider.transform.position);
            }
        }
    }

    private void ClearDestructible(Vector2 position)
    {
        Vector3Int cell = destructibleTiles.WorldToCell(position);
        TileBase tile = destructibleTiles.GetTile(cell);

        if (tile != null)
        {
            Instantiate(destructiblePrefab, position, Quaternion.identity);
            destructibleTiles.SetTile(cell, null);
        }
    }
}
