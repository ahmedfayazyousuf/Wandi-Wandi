using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    public GameScript gameScript;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("FinishLine"))
        {
            if (gameScript.player1Alive || gameScript.player2Alive)
            {
                Debug.Log("Player reached the Finish line!");
                gameScript.AdvanceLevel();
            }
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (gameObject.CompareTag("Player1"))
            {
                Debug.Log("Player 1 met the Enemy!");
                gameScript.HandlePlayerCollision(ref gameScript.player1Alive);
            }
            else if (gameObject.CompareTag("Player2"))
            {
                Debug.Log("Player 2 met the Enemy!");
                gameScript.HandlePlayerCollision(ref gameScript.player2Alive);
            }
        }
    }
}
