using UnityEngine;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    public float maxSpeed = 5f; // Maximum normal movement speed
    public float satisfactionRadius = 0.5f; // Radius to consider "arrived"
    public float avoidRadius = 3f; // Radius to detect and avoid obstacles
    public float smoothTurnSpeed = 5f; // Speed for smooth rotation
    public float walkSpeed = 3f; // Base speed for movement
    public float obstacleAvoidanceStrength = 1.5f; // Strength of the avoidance force
    public float detectionRadius = 10f; // Radius to detect the player
    public Transform player; // Reference to the player
    public Transform chest; // Reference to the chest
    public float respawnDelay = 5f; // Delay before respawning the enemy
    public Vector2 respawnAreaMin; // Minimum coordinates for respawn area
    public Vector2 respawnAreaMax; // Maximum coordinates for respawn area

    private Vector2 destination; // The target position for the enemy
    private bool isMoving = false; // Indicates whether the enemy is currently moving
    private Pathfinding pathfinding;
    private List<Vector2> path;
    private int pathIndex;
    private GridGenerator gridGenerator;

    void Start()
    {
        pathfinding = FindObjectOfType<Pathfinding>();
        gridGenerator = FindObjectOfType<GridGenerator>();
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

        // Detect nearby obstacles
        Vector2 avoidanceForce = Vector2.zero;
        Collider2D[] nearbyObstacles = Physics2D.OverlapCircleAll(currentPosition, avoidRadius);
        foreach (Collider2D obstacle in nearbyObstacles)
        {
            if (obstacle.CompareTag("Obstacle"))
            {
                Vector2 obstacleDirection = currentPosition - (Vector2)obstacle.transform.position;
                float obstacleDistance = obstacleDirection.magnitude;
                float avoidanceStrength = Mathf.Clamp01(1 - (obstacleDistance / avoidRadius));
                avoidanceForce += obstacleDirection.normalized * avoidanceStrength * obstacleAvoidanceStrength;
            }
        }

        Vector2 finalDirection = (directionToTarget + avoidanceForce).normalized;

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
        Vector2 respawnPosition;
        int attempts = 0;
        do
        {
            respawnPosition = new Vector2(
                Random.Range(respawnAreaMin.x, respawnAreaMax.x),
                Random.Range(respawnAreaMin.y, respawnAreaMax.y)
            );
            attempts++;
        } while (!IsValidRespawnPosition(respawnPosition) && attempts < 20);

        if (IsValidRespawnPosition(respawnPosition))
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

    bool IsValidRespawnPosition(Vector2 position)
    {
        Vector2 roundedPosition = new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));
        if (gridGenerator.GetCells().TryGetValue(roundedPosition, out Cell cell) && !cell.isWall)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(roundedPosition, 0.1f);
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Wall") || collider.CompareTag("Obstacle"))
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }
}
