using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace MonsterFlow
{
	/// <summary>
	///    Prepares scene, spawns objects and holds the scores.
	/// </summary>
	public class GameController : MonoBehaviour
	{
		/// <summary> Set reloadLevel to false for testing. </summary>
		public bool reloadLevel;

        /// <summary> Set keepPlayerPrefs to fals for testing. </summary>
        public bool keepPlayerPrefs;

        /// <summary> Reference to the GameController singleton for other objects. </summary>
        public static GameController instance;

		/// <summary> Array of enemy prefabs. </summary>
		public GameObject[] enemyPrefabs;
		/// <summary> Prefab of the object spawn indicator object. </summary>
		public GameObject indicatorPrefab;
		/// <summary> Reference to the Score text ui object. </summary>
		public TextMeshProUGUI scoreTextObject;
		/// <summary> Reference to the highScore text ui object. </summary>
		public TextMeshProUGUI highScoreTextObject;

		/// <summary> Reference to the highScoreTime text ui object. </summary>
		public TextMeshProUGUI highScoreTextObjectTime;
        /// <summary> Reference to the NewHighScoreCirclePrefab UI object. </summary>
        public GameObject NewHighScoreCirclePrefab;
        /// <summary> Reference to the SpeedIncreasePrefab UI object. </summary>
        public GameObject SpeedIncreasePrefab;
        public GameObject NewStartPrefab;
        /// <summary> Reference to the Time Score text ui object. </summary>
        public TextMeshProUGUI scoreTextObjectMinutes;
		public TextMeshProUGUI scoreTextObjectSeparation;
		public TextMeshProUGUI scoreTextObjectSeconds;

        /// <summary> Time for how long Tomatoes spawn. </summary>
        public float waveTime;
		/// <summary> Time before the first wave starts spawning at game start. </summary>
		public float startWait;
		/// <summary> Duration of the pause between the first and second wave. </summary>
		public float initialWaveWaitTime;
		/// <summary> Max wave number where waveWait is being interpolated. </summary>
		public int lastWaveNumber;
		/// <summary> Duration of the pause between waves of lastWaveNumber and above. </summary>
		public float finalWaveWaitTime;
		/// <summary> Time between enemy spawns. </summary>
		public float spawnWait;
		/// <summary> Amount of enemies to spawn per wave. </summary>
		public int tomatoCount;
		/// <summary> Time in seconds before a spawn, at which the indicator appears. </summary>
		public float indicatorBeforeSpawnTime;

		/// <summary> Percentage of move speed increase per wave. </summary>
		public float flowSpeedIncrease;
        private float fixedDeltaTime;

		// Array with predefined spawn positions.
		private readonly Vector2[] spawnPositions;

		private Sounds sounds;

		private bool continueSpawning;
        private bool waitWave = true;
		private int  currentHighestScore;
		private float currentHighestScoreTime;
		/// <summary> Duration of the pause between two waves. Is being calculated. </summary>
		private float currentWaveWait;

		private Vector2 halfScreenSizeInUnits;
		private int     lastSpawnPosIndex;
		private int     nextSpawnPosIndex;

		private int Score;
		private float playTime;
		private int secondLastSpawnPosIndex;

		private GameController()
		{
			// 24 spawn positions in circular distance around origin
			this.spawnPositions = new[]
			{
				new Vector2(0.6f,   4.8f), new Vector2(1.9f,   4.4f), new Vector2(2.9f,   3.8f),
				new Vector2(3.85f,  2.9f), new Vector2(4.5f,   1.8f), new Vector2(4.8f,   0.6f),
				new Vector2(4.8f,   -0.6f), new Vector2(4.5f,  -1.8f), new Vector2(3.85f, -2.9f),
				new Vector2(2.9f,   -3.8f), new Vector2(1.9f,  -4.4f), new Vector2(0.6f,  -4.8f),
				new Vector2(-0.6f,  -4.8f), new Vector2(-1.9f, -4.4f), new Vector2(-2.9f, -3.8f),
				new Vector2(-3.85f, -2.9f), new Vector2(-4.5f, -1.8f), new Vector2(-4.8f, -0.6f),
				new Vector2(-4.8f,  0.6f), new Vector2(-4.5f,  1.8f), new Vector2(-3.85f, 2.9f),
				new Vector2(-2.9f,  3.8f), new Vector2(-1.9f,  4.4f), new Vector2(-0.6f,  4.8f),
			};
		}

		/// <summary>
		///    Multiplier for movement speed of objects.
		/// </summary>
		public float CurrentSpeedMultiplier { get; private set; } = 1;

		private void Awake()
		{
			// Set this as the GameController instance and destroy any existing one.
			if (instance != null) Destroy(instance.gameObject);
			instance = this;

            // Make a copy of the fixedDeltaTime, it defaults to 0.02f, but it can be changed in the editor
            //this.fixedDeltaTime = Time.fixedDeltaTime;
        }

		private void Start()
		{
            if (!this.keepPlayerPrefs)
            {
                PlayerPrefs.DeleteAll();
            }

            // Set screen collider to correct size
            this.halfScreenSizeInUnits.y                   = Camera.main.orthographicSize;
			this.halfScreenSizeInUnits.x                   = Camera.main.aspect         * this.halfScreenSizeInUnits.y;
			Camera.main.GetComponent<BoxCollider2D>().size = this.halfScreenSizeInUnits * 2f;

            // Reset score at start
            UpdateScore();

			// Load highScore from Playerprefs
			SetCurrentHighscoreText();

			// Start spawning Waves
			StartCoroutine(SpawnWaves());

			// Reset level by pressing R on keyboard
			//ControlsManager.Inputs.Standard.Reset.performed += context => ReloadWithReset();

			// Set first waveWait time to max time of waveWait
			this.currentWaveWait = this.initialWaveWaitTime;

            this.sounds = FindObjectOfType<Sounds>();
			if (this.sounds == null) Debug.Log("Cannot find Sounds script.");
		}

		private void Update()
		{
            this.playTime += Time.deltaTime;
		}

		private IEnumerator SpawnWaves()
		{
			// Set toggle for breaking the loop
			this.continueSpawning = true;

			yield return new WaitForSeconds(this.startWait);

			while (this.continueSpawning)
			{
                StartCoroutine(WaitForWave(this.waveTime));

                //for (int i = 0; i < this.tomatoCount; i++)
                while (this.waitWave)
				{
					// Initialize every tomato from the array of enemies
					GameObject tomato = this.enemyPrefabs[Random.Range(0, this.enemyPrefabs.Length)];

					// Choose spawnPosition randomly and not in range of 2 from last and secondLast PosIndex
					while ((this.nextSpawnPosIndex      == this.lastSpawnPosIndex)
						 || (this.nextSpawnPosIndex     == this.secondLastSpawnPosIndex)
						 || (this.nextSpawnPosIndex - 1 == this.lastSpawnPosIndex)
						 || (this.nextSpawnPosIndex - 1 == this.secondLastSpawnPosIndex)
						 || (this.nextSpawnPosIndex + 1 == this.lastSpawnPosIndex)
						 || (this.nextSpawnPosIndex + 1 == this.secondLastSpawnPosIndex)
						 || (this.nextSpawnPosIndex - 2 == this.lastSpawnPosIndex)
						 || (this.nextSpawnPosIndex - 2 == this.secondLastSpawnPosIndex)
						 || (this.nextSpawnPosIndex + 2 == this.lastSpawnPosIndex)
						 || (this.nextSpawnPosIndex + 2 == this.secondLastSpawnPosIndex)

                         || (this.nextSpawnPosIndex == 0 && this.lastSpawnPosIndex == 23)
                         || (this.nextSpawnPosIndex == 0 && this.secondLastSpawnPosIndex == 23)
                         || (this.nextSpawnPosIndex == 0 && this.lastSpawnPosIndex == 22)
                         || (this.nextSpawnPosIndex == 0 && this.secondLastSpawnPosIndex == 22)
                         || (this.nextSpawnPosIndex == 1 && this.lastSpawnPosIndex == 23)
                         || (this.nextSpawnPosIndex == 1 && this.secondLastSpawnPosIndex == 23)

                         || (this.lastSpawnPosIndex == 0 && this.nextSpawnPosIndex == 23)
                         || (this.lastSpawnPosIndex == 0 && this.secondLastSpawnPosIndex == 23)
                         || (this.lastSpawnPosIndex == 0 && this.nextSpawnPosIndex == 22)
                         || (this.lastSpawnPosIndex == 0 && this.secondLastSpawnPosIndex == 22)
                         || (this.lastSpawnPosIndex == 1 && this.nextSpawnPosIndex == 23)
                         || (this.lastSpawnPosIndex == 1 && this.secondLastSpawnPosIndex == 23)

                         || (this.secondLastSpawnPosIndex == 0 && this.nextSpawnPosIndex == 23)
                         || (this.secondLastSpawnPosIndex == 0 && this.lastSpawnPosIndex == 23)
                         || (this.secondLastSpawnPosIndex == 0 && this.nextSpawnPosIndex == 22)
                         || (this.secondLastSpawnPosIndex == 0 && this.lastSpawnPosIndex == 22)
                         || (this.secondLastSpawnPosIndex == 1 && this.nextSpawnPosIndex == 23)
                         || (this.secondLastSpawnPosIndex == 1 && this.lastSpawnPosIndex == 23))
					{
						this.nextSpawnPosIndex = Random.Range(0, 24);
					}

					// Show spawn indicator
					GameObject newIndicator = Instantiate(this.indicatorPrefab,
																	  this.spawnPositions[this.nextSpawnPosIndex], Quaternion.identity);
					newIndicator.transform.up = (newIndicator.transform.position - Vector3.zero).normalized;
					if (Physics2D.Raycast(newIndicator.transform.position, -newIndicator.transform.up, 10)
							 is RaycastHit2D hit) newIndicator.transform.position = hit.point;

					yield return new WaitForSeconds(this.indicatorBeforeSpawnTime);

					// Spawn enemy
					Instantiate(tomato, this.spawnPositions[this.nextSpawnPosIndex], Quaternion.identity)
					  .transform.GetChild(0).Rotate(0, 0, Random.Range(0.0f, 1.0f));
					this.secondLastSpawnPosIndex = this.lastSpawnPosIndex;
					this.lastSpawnPosIndex       = this.nextSpawnPosIndex;

					// Wait seconds until next enemy spawns
					yield return new WaitForSeconds(this.spawnWait);
                }

                // Wait half until next wave starts
                yield return new WaitForSeconds(this.currentWaveWait/2);

                this.sounds.PlaySound(2);

                // Spawn NewHighScoreCircle from Prefab
                if (this.SpeedIncreasePrefab != null)
                {
                    Instantiate(this.SpeedIncreasePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                }
                else
                {
                    print("Could not find SpeedIncreasePrefab");
                }

                // Wait half until next wave starts
                yield return new WaitForSeconds(this.currentWaveWait/2);

                // ApplyflowSpeedIncrease
                //this.spawnWait -= this.flowSpeedIncrease / 2;
                //this.CurrentSpeedMultiplier += this.flowSpeedIncrease;
                Time.timeScale += this.flowSpeedIncrease;
                //Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
                //Time.fixedDeltaTime += this.flowSpeedIncrease;

                // Adjust waveWait time
                //this.currentWaveWaitTime -= (this.initialWaveWaitTime - this.finalWaveWaitTime) / this.lastWaveNumber;
                this.waitWave = true;
            }
		}

        private IEnumerator WaitForWave(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            this.waitWave = false;
        }

        /// <summary>
        ///    Adds the specified amount to the score or sets to 0 if no value is provided.
        /// </summary>
        /// <param name="x"> Amount to add to the score. </param>
        public void UpdateScore(int x = 0)
		{
			this.Score                += x == 0 ? -this.Score : x;
			this.scoreTextObject.text =  this.Score.ToString();
		}

		/// <summary>
		///    Stops the spawn loop, set new highscore if needed and reloads the level.
		/// </summary>
		public void GameOver()
		{
			this.continueSpawning = false;

            Time.timeScale = 1.0f;
            //Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;

            // Set highScore to score if score is higher (old)
            if (this.Score > this.currentHighestScore)
			{
				// Save the new score into the PlayerPrefs to send over to the next play session
				PlayerPrefs.SetInt("highScore", this.Score);
			}

			// Set highScoreTime to playTime if score is higher
			if (this.playTime > 10f && this.playTime > this.currentHighestScoreTime)
			{
                // Save the new Time Score into the PlayerPrefs to send over to the next play session
                PlayerPrefs.SetFloat("highScoreTime", this.playTime);

                this.sounds.PlaySound(1);

                TriggerNewHighScoreCircle();
            }
			else
			{
				this.sounds.PlaySound(0);

                // Spawn NewStart from Prefab
                if (this.NewStartPrefab != null)
                {
                    Instantiate(this.NewStartPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                }
                else
                {
                    print("Could not find NewStartPrefab");
                }
            }

            SetCurrentHighscoreText();

            this.playTime = 0f;

            // Reload level after game over, if checkbox is set (uncheck for testing)
            if (reloadLevel)
			{
                ReloadLevel();
			}
		}

		private void SetCurrentHighscoreText()
		{
			// Load Score from the PlayerPrefs, if no Int of this name exists, it is set to 0
			this.currentHighestScore = PlayerPrefs.GetInt("highScore", 0);
			this.highScoreTextObject.text = this.currentHighestScore.ToString();

			// Load Time Score from the PlayerPrefs, if no Float of this name exists, it is set to 0
			this.currentHighestScoreTime = PlayerPrefs.GetFloat("highScoreTime", 0);
			int highScoreMinutes = Mathf.FloorToInt(this.currentHighestScoreTime / 60F);
			int highScoreSeconds = Mathf.FloorToInt(this.currentHighestScoreTime - highScoreMinutes * 60);

            if (highScoreMinutes < 1)
                this.highScoreTextObjectTime.text = string.Format("{0:00}", highScoreSeconds);
            else
                this.highScoreTextObjectTime.text = string.Format("{0:0}:{1:00}", highScoreMinutes, highScoreSeconds);
        }

        private void TriggerNewHighScoreCircle()
        {
            // Load Time Score from the PlayerPrefs, if no Float of this name exists, it is set to 0
            int highScoreMinutes = Mathf.FloorToInt(this.playTime / 60F);
            int highScoreSeconds = Mathf.FloorToInt(this.playTime - highScoreMinutes * 60);

            if (highScoreMinutes < 1)
            {
                if (this.NewHighScoreCirclePrefab != null)
                {
                    this.NewHighScoreCirclePrefab.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Format("{0:00}", highScoreSeconds);
                    this.NewHighScoreCirclePrefab.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = string.Format("{0:00}", highScoreSeconds);
                }
            }
            else
            {
                if (this.NewHighScoreCirclePrefab != null)
                {
                    this.NewHighScoreCirclePrefab.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Format("{0:0}:{1:00}", highScoreMinutes, highScoreSeconds);
                    this.NewHighScoreCirclePrefab.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = string.Format("{0:0}:{1:00}", highScoreMinutes, highScoreSeconds);
                }
            }

            // Spawn NewHighScoreCircle from Prefab
            if (this.NewHighScoreCirclePrefab != null)
            {
                Instantiate(this.NewHighScoreCirclePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            }
            else
            {
                print("Could not find NewHighScoreCirclePrefab");
            }
        }

        /// <summary>
        ///    Uses time as score and updates OnGUI.
        /// </summary>
        void OnGUI()
		{
			// Obtain the current playTime.
			int minutes = Mathf.FloorToInt(this.playTime / 60F);
			int seconds = Mathf.FloorToInt(this.playTime - minutes * 60);

			this.scoreTextObjectSeconds.text = string.Format("{0:00}", seconds);

			if (minutes < 1)
			{
				this.scoreTextObjectMinutes.text = "";
				this.scoreTextObjectSeparation.text = "";
			} else
			{
				this.scoreTextObjectMinutes.text = string.Format("{0:0}", minutes);
				this.scoreTextObjectSeparation.text = ":";
			}
		}

		private void ReloadLevel() {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ReloadWithReset()
        {
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

		// private IEnumerator LoadAsyncScene()
		// {
		// 	// The application loads the scene in the background as the current scene runs
		// 	AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
		//
		// 	// Wait until the scene fully loads
		// 	while (!asyncLoad.isDone) yield return null;
		// }
	}
}