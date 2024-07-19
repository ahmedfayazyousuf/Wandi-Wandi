using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform initialPosition;
    public Transform centerWaypoint;
    private float speed = 500f; // Default speed

    private bool movingToCenter = true;

    void Start()
    {
        if (initialPosition == null)
        {
            initialPosition = new GameObject("InitialPosition").transform;
            initialPosition.position = transform.position;
        }

        // Register this enemy with the EnemyManager
        EnemyManager.Instance.RegisterEnemy(this);
    }

    void Update()
    {
        MoveEnemy();
    }

    void MoveEnemy()
    {
        Vector2 targetPosition = movingToCenter ? centerWaypoint.position : initialPosition.position;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            movingToCenter = !movingToCenter;
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void ResetPosition()
    {
        // Reset the enemy to its initial position
        transform.position = initialPosition.position;
        movingToCenter = true;
    }
}
