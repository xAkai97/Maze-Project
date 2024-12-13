using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager2D : MonoBehaviour
{
    private Pathfinding pathfinding;
    private GridGenerator gridGenerator;
    public List<Transform> enemies; // List of enemies
    public Transform player; // Reference to the player
    public GameObject chest; // Reference to the chest
    public GameObject winText; // Reference to the "You Win" text UI element
    public float endGameDelay = 3f; // Delay before loading the next scene

    void Start()
    {
        pathfinding = FindObjectOfType<Pathfinding>();
        gridGenerator = FindObjectOfType<GridGenerator>();
        winText.SetActive(false); // Ensure the win text is hidden at the start
    }

    void Update()
    {
        foreach (Transform currentEnemy in enemies)
        {
            StartPathfinding(currentEnemy);
        }

        // Check for ESC key press to return to the main menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainMenu();
        }
    }

    void StartPathfinding(Transform currentEnemy)
    {
        Vector2 playerPosition = player.position;
        Vector2 enemyPosition = currentEnemy.position;

        // Round positions to align with grid cells
        playerPosition = new Vector2(Mathf.Round(playerPosition.x), Mathf.Round(playerPosition.y));
        enemyPosition = new Vector2(Mathf.Round(enemyPosition.x), Mathf.Round(enemyPosition.y));

        // Clamp positions to ensure they stay within the grid bounds
        playerPosition = ClampPositionToGridBounds(playerPosition);
        enemyPosition = ClampPositionToGridBounds(enemyPosition);

        if (pathfinding != null)
        {
            pathfinding.SetPathPositions(enemyPosition, playerPosition);
            List<Vector2> path = pathfinding.GetFinalPath();
            if (path != null)
            {
                EnemyController enemyController = currentEnemy.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.SetDestination(playerPosition);
                }
            }
            else
            {
                Debug.Log("No valid path found");
            }
        }
    }

    Vector2 ClampPositionToGridBounds(Vector2 position)
    {
        float clampedX = Mathf.Clamp(position.x, 0, gridGenerator.GridWidth * gridGenerator.CellSize.x - 1);
        float clampedY = Mathf.Clamp(position.y, 0, gridGenerator.GridHeight * gridGenerator.CellSize.y - 1);
        return new Vector2(clampedX, clampedY);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == chest)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        Debug.Log("Game Over! Player touched the chest.");
        winText.SetActive(true); // Show the win text
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(endGameDelay);
        SceneManager.LoadScene("Lvl 1");
    }

    void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Replace "MainMenu" with the actual name of your main menu scene
    }
}
