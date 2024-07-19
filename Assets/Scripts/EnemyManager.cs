using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // Singleton instance
    public static EnemyManager Instance;

    // List to hold all EnemyMovement instances
    private List<EnemyMovement> enemies = new List<EnemyMovement>();

    void Awake()
    {
        // Ensure only one instance of the singleton exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keeps the manager between scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Populate the enemies list
        EnemyMovement[] enemyArray = FindObjectsOfType<EnemyMovement>();
        enemies.AddRange(enemyArray);
    }

    public void RegisterEnemy(EnemyMovement enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
    }

    public void UpdateEnemySpeed(float speed)
    {
        foreach (EnemyMovement enemy in enemies)
        {
            enemy.SetSpeed(speed);
        }
    }
}
