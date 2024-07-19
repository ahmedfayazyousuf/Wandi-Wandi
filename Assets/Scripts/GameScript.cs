using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public class GameScript : MonoBehaviour
{
    // Player Movement
    public GameObject player1;
    public GameObject player2;
    public float moveSpeed = 500f;

    public GameObject leaderboardEntryPrefab;
    public Transform leaderboardEntriesParent;
    //public TMP_Text leaderboardLoadingText;

    private Vector2 movement1;
    private Vector2 movement2;

    public int GameCompleted = 0;

    // Player Lives
    public int playerLives = 3;
    public GameObject[] hearts; // Assign heart GameObjects in the Unity editor
    [HideInInspector] public bool player1Alive = true;
    [HideInInspector] public bool player2Alive = true;

    // Game Script
    public TMP_Text TeamName; // TextMeshPro Text for Team Name
    public TMP_Text timerText; // TextMeshPro Text for Timer
    public TMP_Text timerTextGameOver; // TextMeshPro Text for Timer
    public TMP_Text levelText; // TextMeshPro Text for displaying current level
    public TMP_Text levelTextGameOver; // TextMeshPro Text for displaying current level
    public TMP_Text GameDifficulty;
    public TMP_Text GameDifficultyLeaderboard;
    public TMP_Text EnemySpeed;
    private string GameDifficultyChosen; // User ID to pass to InputScript

    private float timer = 60f; // 60 seconds timer
    private UIController UIController; // Reference to the ScreenManager (or UIController)
    private EnemyMovement[] enemies; // Array of enemy movement scripts
    private string userId; // User ID to pass to InputScript

    // Input Script
    public TMP_InputField nameInputField; // Use TMP_InputField instead of InputField
    public Button submitButton;
    public TMP_Text statusText; // Use TMP_Text instead of Text

    private DatabaseReference databaseReference;

    // Boundary Collider
    public BoxCollider2D boundaryCollider;


    // Levels
    private int currentLevel = 1;
    private int maxLevel = 3;
    public GameObject finishLine; // Assign the finish line GameObject in the Unity editor

    // Player Reset Positions
    public Transform player1ResetPosition;
    public Transform player2ResetPosition;

    void Start()
    {

        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });

        // Display player name from PlayerPrefs
        string playerName = PlayerPrefs.GetString("PlayerName", "Unknown Player");
        TeamName.text = playerName;

        // Find and assign the ScreenManager (UIController) in the scene
        UIController = FindObjectOfType<UIController>();
        if (UIController == null)
        {
            Debug.LogError("ScreenManager (UIController) not found in the scene.");
        }

        // Find all EnemyMovement scripts in the scene
        enemies = FindObjectsOfType<EnemyMovement>();

        // Add a listener to the button
        submitButton.onClick.AddListener(OnSubmit);

        // Initialize hearts UI
        UpdateHeartsUI();

        // Update the level text initially
        UpdateLevelText();


        // Set initial enemy speed based on the current level
        UpdateEnemySpeed();
    }

    void InitializeFirebase()
    {
        // Set the database URL
        FirebaseApp app = FirebaseApp.Create(new AppOptions
        {
            DatabaseUrl = new System.Uri("https://wandiwandi-90f6f-default-rtdb.firebaseio.com/")
        });
        databaseReference = FirebaseDatabase.GetInstance(app).RootReference;
    }

    public void DifficultyEasy()
    {
        GameDifficulty.text = "Easy";
        GameDifficultyLeaderboard.text = "Easy";
        GameDifficultyChosen = "Easy";
        UIController.ShowGameScreen();
        // Start the timer
        StartCoroutine(StartTimer());
        UpdateEnemySpeed();
    }
    public void DifficultyMedium()
    {
        GameDifficulty.text = "Medium";
        GameDifficultyLeaderboard.text = "Easy";
        GameDifficultyChosen = "Medium";
        UIController.ShowGameScreen();
        // Start the timer
        StartCoroutine(StartTimer());
        UpdateEnemySpeed();
    }
    public void DifficultyHard()
    {
        GameDifficulty.text = "Hard";
        GameDifficultyLeaderboard.text = "Easy";
        GameDifficultyChosen = "Hard";
        UIController.ShowGameScreen();
        // Start the timer
        StartCoroutine(StartTimer());
        UpdateEnemySpeed();
    }

    public void FetchLeaderboard()
    {
        UIController.ShowLeaderboardScreen();
        if (string.IsNullOrEmpty(GameDifficultyChosen))
        {
            Debug.LogError("Game difficulty is not chosen. Cannot fetch leaderboard.");
            return;
        }

        GameDifficultyLeaderboard.text = GameDifficultyChosen;

        // Show loading text
        //leaderboardLoadingText.gameObject.SetActive(true);
        //leaderboardLoadingText.text = "Loading...";

        databaseReference.Child("users").OrderByChild("GameDifficulty").EqualTo(GameDifficultyChosen).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to fetch leaderboard data: " + task.Exception);
                //leaderboardLoadingText.text = "Failed to load leaderboard.";
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                // Clear existing leaderboard entries
                foreach (Transform child in leaderboardEntriesParent)
                {
                    Destroy(child.gameObject);
                }

                // Convert snapshot to list of leaderboard entries
                List<DataSnapshot> leaderboardEntries = new List<DataSnapshot>();
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    leaderboardEntries.Add(childSnapshot);
                }

                // Sort entries based on LevelsPassed and TimeTaken
                leaderboardEntries.Sort((a, b) =>
                {
                    int levelsA = int.Parse(a.Child("LevelsPassed").Value.ToString());
                    int levelsB = int.Parse(b.Child("LevelsPassed").Value.ToString());
                    if (levelsA != levelsB)
                    {
                        return levelsB.CompareTo(levelsA); // Sort descending by LevelsPassed
                    }
                    else
                    {
                        float timeA = float.Parse(a.Child("TimeTaken").Value.ToString());
                        float timeB = float.Parse(b.Child("TimeTaken").Value.ToString());
                        return timeA.CompareTo(timeB); // Sort ascending by TimeTaken
                    }
                });

                // Take the top 5 entries
                List<DataSnapshot> topEntries = leaderboardEntries.Take(5).ToList();

                // Populate leaderboard with top entries
                for (int i = 0; i < topEntries.Count; i++)
                {
                    DataSnapshot entry = topEntries[i];

                    GameObject entryObject = Instantiate(leaderboardEntryPrefab, leaderboardEntriesParent);
                    TMP_Text indexText = entryObject.transform.Find("Index").GetComponent<TMP_Text>();
                    TMP_Text nameText = entryObject.transform.Find("Name").GetComponent<TMP_Text>();
                    TMP_Text levelsPassedText = entryObject.transform.Find("LevelsPassed").GetComponent<TMP_Text>();
                    TMP_Text timeTakenText = entryObject.transform.Find("TimeTaken").GetComponent<TMP_Text>();

                    indexText.text = (i + 1).ToString();
                    nameText.text = entry.Child("name").Value.ToString();
                    levelsPassedText.text = entry.Child("LevelsPassed").Value.ToString();
                    timeTakenText.text = entry.Child("TimeTaken").Value.ToString();
                }

                //leaderboardLoadingText.gameObject.SetActive(false);
            }
        });
    }


    void OnSubmit()
    {
        string userName = nameInputField.text;

        if (string.IsNullOrEmpty(userName))
        {
            statusText.text = "Name cannot be empty.";
            return;
        }

        // Update PlayerPrefs with the new team name
        PlayerPrefs.SetString("PlayerName", userName);
        PlayerPrefs.Save();

        // Update TeamName text with the new team name
        TeamName.text = userName;

        // Generate a unique user ID
        userId = System.Guid.NewGuid().ToString();

        // Create a user object with name
        User user = new User(userName);

        // Convert the user object to JSON and save it to Firebase
        string json = JsonUtility.ToJson(user);
        databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Name saved successfully.");
            }
            else
            {
                //statusText.text = "Failed to save name: " + task.Exception.ToString();
                Debug.Log("Failed to save name!");
            }
        });

        // Show GameScreen
        UIController.ShowChooseDifficultyScreen();
    }

    // User class to hold user data
    [System.Serializable]
    public class User
    {
        public string name;

        public User(string name)
        {
            this.name = name;
        }
    }

    void Update()
    {
        // Reset movements each frame
        movement1 = Vector2.zero;
        movement2 = Vector2.zero;

        // Player 1 controls (WASD)
        if (player1Alive)
        {
            if (Input.GetKey(KeyCode.W))
            {
                movement1.y = 1;
            }
            if (Input.GetKey(KeyCode.S))
            {
                movement1.y = -1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                movement1.x = -1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                movement1.x = 1;
            }
        }

        // Player 2 controls (Arrow keys)
        if (player2Alive)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                movement2.y = 1;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                movement2.y = -1;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                movement2.x = -1;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                movement2.x = 1;
            }
        }

        // Normalize the movement vectors to ensure consistent movement speed in all directions
        movement1.Normalize();
        movement2.Normalize();
    }

    void FixedUpdate()
    {
        // Move Player 1
        MovePlayer(player1, movement1);

        // Move Player 2
        MovePlayer(player2, movement2);
    }

    void MovePlayer(GameObject player, Vector2 movement)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        Vector2 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

        // Clamp the position within the boundary collider
        newPosition.x = Mathf.Clamp(newPosition.x, boundaryCollider.bounds.min.x, boundaryCollider.bounds.max.x);
        newPosition.y = Mathf.Clamp(newPosition.y, boundaryCollider.bounds.min.y, boundaryCollider.bounds.max.y);

        rb.MovePosition(newPosition);
    }

    IEnumerator StartTimer()
    {
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI();
            yield return null;
        }

        timer = 0;
        UpdateTimerUI();
        OnTimerEnd();
    }

    void UpdateTimerUI()
    {
        timerText.text = "Time: " + Mathf.Ceil(timer).ToString();
    }

    void OnTimerEnd()
    {
        // Show game end screen using UIController function
        if (UIController != null)
        {
            UIController.ShowGameEndScreen();
            SaveGameStatsToFirebase();
            //timerTextGameOver.text = "Time: 60";
            timerTextGameOver.text = "TIME TAKEN: " + Mathf.Ceil(60 - timer);
        }
        else
        {
            Debug.LogError("ScreenManager (UIController) reference is null.");
        }

    }

    void OnDestroy()
    {
        // Clean up listeners
        if (UIController != null)
        {
            // No need to unsubscribe OnDifficultySelected as it was removed
        }
    }

    public void SetUserId(string id)
    {
        userId = id;
    }

    public void HandlePlayerCollision(ref bool playerAlive)
    {
        if (playerAlive)
        {
            playerAlive = false;

            // Change color of player1 if it's not alive
            Image player1Image = player1.GetComponent<Image>();
            if (player1Image != null && !player1Alive)
            {
                player1Image.color = UnityEngine.Color.red;
            }

            Image player2Image = player2.GetComponent<Image>();
            if (player2Image != null && !player2Alive)
            {
                player2Image.color = UnityEngine.Color.red;
            }

            // Check if both players are not alive
            if (!player1Alive && !player2Alive)
            {
                playerLives--;

                if (playerLives > 0)
                {
                    // Reset the level if lives are left
                    player1Image.color = UnityEngine.Color.white;
                    player2Image.color = UnityEngine.Color.white;
                    ResetLevel();
                }
                else
                {
                    Debug.Log("Game Over!");
                    // Show game end screen
                    if (UIController != null)
                    {
                        UIController.ShowGameEndScreen();
                        SaveGameStatsToFirebase();
                        timerTextGameOver.text = "TIME TAKEN: " + Mathf.Ceil(60 - timer);
                    }
                }

                UpdateHeartsUI();
            }
        }
    }

    void UpdateHeartsUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < playerLives)
            {
                hearts[i].SetActive(true);
            }
            else
            {
                hearts[i].SetActive(false);
            }
        }
    }

    public void AdvanceLevel()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            UpdateEnemySpeed(); // Call UpdateEnemySpeed after advancing level
            ResetPlayers();
        }
        else
        {
            // If the max level is reached, show the game end screen
            if (UIController != null)
            {
                UIController.ShowGameEndScreen();
                GameCompleted = 1;
                SaveGameStatsToFirebase();
                timerTextGameOver.text = "TIME TAKEN: " + Mathf.Ceil(60 - timer);
            }
        }


        UpdateLevelText();
    }

    public void UpdateEnemySpeed()
    {
        float speed = 500f;
        if (GameDifficultyChosen == "Easy")
        {
            switch (currentLevel)
            {
                case 1:
                    speed = 500f;
                    EnemySpeed.text = "1x";
                    break;
                case 2:
                    speed = 750f;
                    EnemySpeed.text = "1.5x";
                    break;
                case 3:
                    speed = 1000f;
                    EnemySpeed.text = "2x";
                    break;
            }
        }

        if (GameDifficultyChosen == "Medium")
        {
            switch (currentLevel)
            {
                case 1:
                    speed = 500f;
                    EnemySpeed.text = "1x";
                    break;
                case 2:
                    speed = 1000f;
                    EnemySpeed.text = "2x";
                    break;
                case 3:
                    speed = 1500f;
                    EnemySpeed.text = "3x";
                    break;
            }
        }

        if (GameDifficultyChosen == "Hard")
        {
            switch (currentLevel)
            {
                case 1:
                    speed = 500f;
                    EnemySpeed.text = "1x";
                    break;
                case 2:
                    speed = 1500f;
                    EnemySpeed.text = "3x";
                    break;
                case 3:
                    speed = 2500f;
                    EnemySpeed.text = "5x";
                    break;
            }
        }
        Debug.Log("Updated enemy speed to: " + speed); // Log the final speed set

        // Ensure the EnemyManager instance is initialized
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UpdateEnemySpeed(speed);
        }
        else
        {
            Debug.LogError("EnemyManager instance is not initialized.");
        }
    }

    void ResetPlayers()
    {
        player1Alive = true;
        player2Alive = true;

        // Reset player positions to initial positions
        if (player1ResetPosition != null)
        {
            player1.transform.position = player1ResetPosition.position;
        }
        else
        {
            Debug.LogError("Player 1 reset position is not assigned.");
        }

        if (player2ResetPosition != null)
        {
            player2.transform.position = player2ResetPosition.position;
        }
        else
        {
            Debug.LogError("Player 2 reset position is not assigned.");
        }

        Image player1Image = player1.GetComponent<Image>();
        if (player1Image != null)
        {
            player1Image.color = UnityEngine.Color.white;
        }

        Image player2Image = player2.GetComponent<Image>();
        if (player2Image != null)
        {
            player2Image.color = UnityEngine.Color.white;
        }
    }

    void UpdateLevelText()
    {
        levelText.text = "Level: " + currentLevel.ToString();
        levelTextGameOver.text = "Level: " + currentLevel.ToString();
    }

    void ResetLevel()
    {
        player1Alive = true;
        player2Alive = true;

        // Reset player positions
        if (player1ResetPosition != null)
        {
            player1.transform.position = player1ResetPosition.position;
        }
        else
        {
            Debug.LogError("Player 1 reset position is not assigned.");
        }

        if (player2ResetPosition != null)
        {
            player2.transform.position = player2ResetPosition.position;
        }
        else
        {
            Debug.LogError("Player 2 reset position is not assigned.");
        }

        // Reset enemy positions and states if needed
        foreach (EnemyMovement enemy in enemies)
        {
            enemy.ResetPosition(); // Ensure you have a method in EnemyMovement to reset position
        }
    }

    public class GameStats
    {
        public float TimeTaken;
        public int LevelsPassed;
        public string GameDifficulty;

        public GameStats(float timeTaken, int levelsPassed, string gameDifficulty)
        {
            this.TimeTaken = timeTaken;
            this.LevelsPassed = levelsPassed;
            this.GameDifficulty = gameDifficulty;
        }
    }

    public void SaveGameStatsToFirebase()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not set. Cannot save game stats.");
            return;
        }

        // Calculate TimeTaken
        float timeTaken = Mathf.Ceil(60 - timer);

        // Calculate LevelsPassed
        int levelsPassed;

        if(GameCompleted == 1)
        {
            levelsPassed = 3;
        }
        else
        {
            levelsPassed = currentLevel - 1;
        }

        // Save Difficulty Type
        string gameDifficultyChosen = GameDifficultyChosen;

        // Create a game stats object
        GameStats gameStats = new GameStats(timeTaken, levelsPassed, gameDifficultyChosen);

        // Manually create the dictionary
        Dictionary<string, object> gameStatsDict = new Dictionary<string, object>
    {
        { "TimeTaken", gameStats.TimeTaken },
        { "LevelsPassed", gameStats.LevelsPassed },
        { "GameDifficulty", gameStats.GameDifficulty }
    };

        // Update the user's document in Firebase
        databaseReference.Child("users").Child(userId).UpdateChildrenAsync(gameStatsDict).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Game stats saved successfully.");
                GameCompleted = 1;
            }
            else
            {
                Debug.LogError("Failed to save game stats: " + task.Exception.ToString());
            }
        });

    }
}