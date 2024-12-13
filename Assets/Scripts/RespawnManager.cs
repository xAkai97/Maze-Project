using UnityEngine;
using System.Collections.Generic;

public class RespawnManager : MonoBehaviour
{
    public Vector2 respawnAreaCenter; // Center coordinates for respawn area
    public float respawnAreaRadius = 5f; // Radius for respawn area
    public List<GameObject> enemyPrefabs; // List of enemy prefabs
    private GridGenerator gridGenerator;

    void Start()
    {
        gridGenerator = FindObjectOfType<GridGenerator>();
        if (gridGenerator == null)
        {
            Debug.LogError("GridGenerator not found in the scene.");
        }
        else
        {
            Debug.Log("GridGenerator successfully initialized.");
        }
    }

    public Vector2 GetValidRespawnPosition()
    {
        Vector2 respawnPosition;
        int attempts = 0;
        do
        {
            float angle = Random.Range(0f, Mathf.PI * 2);
            float radius = Random.Range(0f, respawnAreaRadius);
            respawnPosition = respawnAreaCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            attempts++;
        } while (!IsValidRespawnPosition(respawnPosition) && attempts < 20);

        return respawnPosition;
    }

    bool IsValidRespawnPosition(Vector2 position)
    {
        if (gridGenerator == null)
        {
            Debug.LogError("GridGenerator is not initialized.");
            return false;
        }

        Vector2 roundedPosition = new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));
        if (gridGenerator.GetCells().TryGetValue(roundedPosition, out Cell cell))
        {
            Debug.Log($"Checking cell at position: {roundedPosition}, isWall: {cell.isWall}");
            if (!cell.isWall)
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
        }
        return false;
    }

    public GameObject RespawnEnemy()
    {
        Vector2 respawnPosition = GetValidRespawnPosition();
        if (respawnPosition != Vector2.zero && enemyPrefabs.Count > 0)
        {
            int randomIndex = Random.Range(0, enemyPrefabs.Count);
            return Instantiate(enemyPrefabs[randomIndex], respawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Failed to find a valid respawn position or no enemy prefabs available.");
            return null;
        }
    }
}
