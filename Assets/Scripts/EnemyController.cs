using UnityEngine;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    public float maxSpeed = 5f; // Maximum normal movement speed
    public float satisfactionRadius = 0.5f; // Radius to consider "arrived"
    public float smoothTurnSpeed = 5f; // Speed for smooth rotation
    public float walkSpeed = 3f; // Base speed for movement
    public float detectionRadius = 10f; // Radius to detect the player
    public Transform player; // Reference to the player
    public Transform chest; // Reference to the chest
    public float respawnDelay = 5f; // Delay before respawning the enemy

    private Vector2 destination; // The target position for the enemy
    private bool isMoving = false; // Indicates whether the enemy is currently moving
    private Pathfinding pathfinding;
    private List<Vector2> path;
    private int pathIndex;
    private RespawnManager respawnManager;

    void Start()
    {
        pathfinding = FindObjectOfType<Pathfinding>();
        respawnManager = FindObjectOfType<RespawnManager>();
        if (respawnManager == null)
        {
            Debug.LogError("RespawnManager not found in the scene.");
        }
        else
        {
            Debug.Log("RespawnManager successfully initialized.");
        }
    }

    void Update()
    {
        if (isMoving)
        {
            MoveEnemy();
        }
        else
        {
            MakeDecision();
        }
    }

    public void SetDestination(Vector2 targetPosition)
    {
        if (pathfinding != null)
        {
            Vector2 startPosition = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
            pathfinding.SetPathPositions(startPosition, targetPosition);
            path = pathfinding.GetFinalPath();
            pathIndex = 0;
            isMoving = path != null && path.Count > 0;
        }
    }

    void MoveEnemy()
    {
        if (path == null || pathIndex >= path.Count)
        {
            isMoving = false;
            return;
        }

        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = path[pathIndex];
        Vector2 directionToTarget = targetPosition - currentPosition;
        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget <= satisfactionRadius)
        {
            pathIndex++;
            if (pathIndex >= path.Count)
            {
                isMoving = false;
                return;
            }
            targetPosition = path[pathIndex];
            directionToTarget = targetPosition - currentPosition;
        }

        directionToTarget.Normalize();

        Vector2 finalDirection = directionToTarget;

        float angle = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothTurnSpeed);

        float adjustedSpeed = walkSpeed * (finalDirection.x != 0 && finalDirection.y != 0 ? 0.7071f : 1f);
        transform.position += (Vector3)(finalDirection * adjustedSpeed * Time.deltaTime);
    }

    void MakeDecision()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float distanceToChest = Vector2.Distance(transform.position, chest.position);

        if (distanceToPlayer <= detectionRadius)
        {
            SetDestination(player.position);
            AdjustDifficulty(distanceToPlayer);
        }
        else if (distanceToChest <= detectionRadius)
        {
            SetDestination(chest.position);
        }
    }

    void AdjustDifficulty(float distanceToPlayer)
    {
        float proximityFactor = Mathf.Clamp01(1 - (distanceToPlayer / detectionRadius));
        walkSpeed = Mathf.Lerp(3f, maxSpeed, proximityFactor);
    }

    public void DestroyEnemy()
    {
        gameObject.SetActive(false);
        Invoke(nameof(RespawnEnemy), respawnDelay);
    }

    void RespawnEnemy()
    {
        Vector2 respawnPosition = respawnManager.GetValidRespawnPosition();
        if (respawnPosition != Vector2.zero)
        {
            transform.position = respawnPosition;
            gameObject.SetActive(true);
            isMoving = false;
        }
        else
        {
            Debug.LogWarning("Failed to find a valid respawn position after multiple attempts.");
        }
    }
}
