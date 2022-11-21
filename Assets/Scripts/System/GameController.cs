using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonsterFlow.System
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance;
        public bool reloadLevel;
        public bool keepPlayerPrefs;
        public GameObject[] enemyPrefabs;
        public GameObject indicatorPrefab;
        public TextMeshProUGUI scoreTextObject;
        public TextMeshProUGUI highScoreTextObject;
        public TextMeshProUGUI highScoreTextObjectTime;
        public GameObject newHighScoreCirclePrefab;
        public GameObject speedIncreasePrefab;
        public GameObject newStartPrefab;
        public TextMeshProUGUI scoreTextObjectMinutes;
        public TextMeshProUGUI scoreTextObjectSeparation;
        public TextMeshProUGUI scoreTextObjectSeconds;

        public float waveTime;
        public float startWait;
        public float initialWaveWaitTime;
        public int lastWaveNumber;
        public float finalWaveWaitTime;
        public float spawnWait;
        public int tomatoCount;
        public float indicatorBeforeSpawnTime;
        public float flowSpeedIncrease;
        private readonly Vector2[] _spawnPositions;
        private bool _continueSpawning;
        private int _currentHighestScore;
        private float _currentHighestScoreTime;
        private float _currentWaveWait;
        private Vector2 _halfScreenSizeInUnits;
        private int _lastSpawnPosIndex;
        private int _nextSpawnPosIndex;
        private float _playTime;
        private int _score;
        private int _secondLastSpawnPosIndex;
        private Sounds _sounds;
        private bool _waitWave = true;

        private GameController()
        {
            // 24 spawn positions in circular distance around origin
            _spawnPositions = new[]
            {
                new Vector2(0.6f, 4.8f), new Vector2(1.9f, 4.4f), new Vector2(2.9f, 3.8f),
                new Vector2(3.85f, 2.9f), new Vector2(4.5f, 1.8f), new Vector2(4.8f, 0.6f),
                new Vector2(4.8f, -0.6f), new Vector2(4.5f, -1.8f), new Vector2(3.85f, -2.9f),
                new Vector2(2.9f, -3.8f), new Vector2(1.9f, -4.4f), new Vector2(0.6f, -4.8f),
                new Vector2(-0.6f, -4.8f), new Vector2(-1.9f, -4.4f), new Vector2(-2.9f, -3.8f),
                new Vector2(-3.85f, -2.9f), new Vector2(-4.5f, -1.8f), new Vector2(-4.8f, -0.6f),
                new Vector2(-4.8f, 0.6f), new Vector2(-4.5f, 1.8f), new Vector2(-3.85f, 2.9f),
                new Vector2(-2.9f, 3.8f), new Vector2(-1.9f, 4.4f), new Vector2(-0.6f, 4.8f)
            };
        }
        
        public float CurrentSpeedMultiplier { get; } = 1;

        private void Awake()
        {
            if (Instance != null) Destroy(Instance.gameObject);
            Instance = this;
        }

        private void Start()
        {
            if (!keepPlayerPrefs) PlayerPrefs.DeleteAll();

            // Set screen collider to correct size
            _halfScreenSizeInUnits.y = Camera.main.orthographicSize;
            _halfScreenSizeInUnits.x = Camera.main.aspect * _halfScreenSizeInUnits.y;
            Camera.main.GetComponent<BoxCollider2D>().size = _halfScreenSizeInUnits * 2f;

            // Reset score at start
            UpdateScore();

            // Load highScore from Playerprefs
            SetCurrentHighscoreText();

            // Start spawning Waves
            StartCoroutine(SpawnWaves());

            // Set first waveWait time to max time of waveWait
            _currentWaveWait = initialWaveWaitTime;

            _sounds = FindObjectOfType<Sounds>();
            if (_sounds == null) Debug.Log("Cannot find Sounds script.");
        }

        private void Update()
        {
            _playTime += Time.deltaTime;
        }
        
        private void OnGUI()
        {
            // Obtain the current playTime.
            var minutes = Mathf.FloorToInt(_playTime / 60F);
            var seconds = Mathf.FloorToInt(_playTime - minutes * 60);

            scoreTextObjectSeconds.text = string.Format("{0:00}", seconds);

            if (minutes < 1)
            {
                scoreTextObjectMinutes.text = "";
                scoreTextObjectSeparation.text = "";
            }
            else
            {
                scoreTextObjectMinutes.text = string.Format("{0:0}", minutes);
                scoreTextObjectSeparation.text = ":";
            }
        }

        private IEnumerator SpawnWaves()
        {
            // Set toggle for breaking the loop
            _continueSpawning = true;

            yield return new WaitForSeconds(startWait);

            while (_continueSpawning)
            {
                StartCoroutine(WaitForWave(waveTime));
                
                while (_waitWave)
                {
                    // Initialize every tomato from the array of enemies
                    var tomato = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

                    // Choose spawnPosition randomly and not in range of 2 from last and secondLast PosIndex
                    while (_nextSpawnPosIndex == _lastSpawnPosIndex
                           || _nextSpawnPosIndex == _secondLastSpawnPosIndex
                           || _nextSpawnPosIndex - 1 == _lastSpawnPosIndex
                           || _nextSpawnPosIndex - 1 == _secondLastSpawnPosIndex
                           || _nextSpawnPosIndex + 1 == _lastSpawnPosIndex
                           || _nextSpawnPosIndex + 1 == _secondLastSpawnPosIndex
                           || _nextSpawnPosIndex - 2 == _lastSpawnPosIndex
                           || _nextSpawnPosIndex - 2 == _secondLastSpawnPosIndex
                           || _nextSpawnPosIndex + 2 == _lastSpawnPosIndex
                           || _nextSpawnPosIndex + 2 == _secondLastSpawnPosIndex
                           || _nextSpawnPosIndex == 0 && _lastSpawnPosIndex == 23
                           || _nextSpawnPosIndex == 0 && _secondLastSpawnPosIndex == 23
                           || _nextSpawnPosIndex == 0 && _lastSpawnPosIndex == 22
                           || _nextSpawnPosIndex == 0 && _secondLastSpawnPosIndex == 22
                           || _nextSpawnPosIndex == 1 && _lastSpawnPosIndex == 23
                           || _nextSpawnPosIndex == 1 && _secondLastSpawnPosIndex == 23
                           || _lastSpawnPosIndex == 0 && _nextSpawnPosIndex == 23
                           || _lastSpawnPosIndex == 0 && _secondLastSpawnPosIndex == 23
                           || _lastSpawnPosIndex == 0 && _nextSpawnPosIndex == 22
                           || _lastSpawnPosIndex == 0 && _secondLastSpawnPosIndex == 22
                           || _lastSpawnPosIndex == 1 && _nextSpawnPosIndex == 23
                           || _lastSpawnPosIndex == 1 && _secondLastSpawnPosIndex == 23
                           || _secondLastSpawnPosIndex == 0 && _nextSpawnPosIndex == 23
                           || _secondLastSpawnPosIndex == 0 && _lastSpawnPosIndex == 23
                           || _secondLastSpawnPosIndex == 0 && _nextSpawnPosIndex == 22
                           || _secondLastSpawnPosIndex == 0 && _lastSpawnPosIndex == 22
                           || _secondLastSpawnPosIndex == 1 && _nextSpawnPosIndex == 23
                           || _secondLastSpawnPosIndex == 1 && _lastSpawnPosIndex == 23)
                        _nextSpawnPosIndex = Random.Range(0, 24);

                    // Show spawn indicator
                    var newIndicator = Instantiate(indicatorPrefab,
                        _spawnPositions[_nextSpawnPosIndex], Quaternion.identity);
                    newIndicator.transform.up = (newIndicator.transform.position - Vector3.zero).normalized;
                    if (Physics2D.Raycast(newIndicator.transform.position, -newIndicator.transform.up, 10)
                        is RaycastHit2D hit) newIndicator.transform.position = hit.point;

                    yield return new WaitForSeconds(indicatorBeforeSpawnTime);

                    // Spawn enemy
                    Instantiate(tomato, _spawnPositions[_nextSpawnPosIndex], Quaternion.identity)
                        .transform.GetChild(0).Rotate(0, 0, Random.Range(0.0f, 1.0f));
                    _secondLastSpawnPosIndex = _lastSpawnPosIndex;
                    _lastSpawnPosIndex = _nextSpawnPosIndex;

                    // Wait seconds until next enemy spawns
                    yield return new WaitForSeconds(spawnWait);
                }

                // Wait half until next wave starts
                yield return new WaitForSeconds(_currentWaveWait / 2);

                _sounds.PlaySound(2);

                // Spawn NewHighScoreCircle from Prefab
                if (speedIncreasePrefab != null)
                    Instantiate(speedIncreasePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                else
                    print("Could not find SpeedIncreasePrefab");

                // Wait half until next wave starts
                yield return new WaitForSeconds(_currentWaveWait / 2);

                // Apply flowSpeedIncrease
                Time.timeScale += flowSpeedIncrease;
                
                _waitWave = true;
            }
        }

        private IEnumerator WaitForWave(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            _waitWave = false;
        }
        
        public void UpdateScore(int x = 0)
        {
            _score += x == 0 ? -_score : x;
            scoreTextObject.text = _score.ToString();
        }
        
        public void GameOver()
        {
            _continueSpawning = false;

            Time.timeScale = 1.0f;

            // Set highScore to score if score is higher (old)
            if (_score > _currentHighestScore)
                // Save the new score into the PlayerPrefs to send over to the next play session
                PlayerPrefs.SetInt("highScore", _score);

            // Set highScoreTime to playTime if score is higher
            if (_playTime > 10f && _playTime > _currentHighestScoreTime)
            {
                // Save the new Time Score into the PlayerPrefs to send over to the next play session
                PlayerPrefs.SetFloat("highScoreTime", _playTime);

                _sounds.PlaySound(1);

                TriggerNewHighScoreCircle();
            }
            else
            {
                _sounds.PlaySound(0);

                // Spawn NewStart from Prefab
                if (newStartPrefab != null)
                    Instantiate(newStartPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                else
                    print("Could not find NewStartPrefab");
            }

            SetCurrentHighscoreText();

            _playTime = 0f;

            // Reload level after game over, if checkbox is set (uncheck for testing)
            if (reloadLevel) ReloadLevel();
        }

        private void SetCurrentHighscoreText()
        {
            // Load Score from the PlayerPrefs, if no Int of this name exists, it is set to 0
            _currentHighestScore = PlayerPrefs.GetInt("highScore", 0);
            highScoreTextObject.text = _currentHighestScore.ToString();

            // Load Time Score from the PlayerPrefs, if no Float of this name exists, it is set to 0
            _currentHighestScoreTime = PlayerPrefs.GetFloat("highScoreTime", 0);
            var highScoreMinutes = Mathf.FloorToInt(_currentHighestScoreTime / 60F);
            var highScoreSeconds = Mathf.FloorToInt(_currentHighestScoreTime - highScoreMinutes * 60);

            if (highScoreMinutes < 1)
                highScoreTextObjectTime.text = string.Format("{0:00}", highScoreSeconds);
            else
                highScoreTextObjectTime.text = string.Format("{0:0}:{1:00}", highScoreMinutes, highScoreSeconds);
        }

        private void TriggerNewHighScoreCircle()
        {
            // Load Time Score from the PlayerPrefs, if no Float of this name exists, it is set to 0
            var highScoreMinutes = Mathf.FloorToInt(_playTime / 60F);
            var highScoreSeconds = Mathf.FloorToInt(_playTime - highScoreMinutes * 60);

            if (highScoreMinutes < 1)
            {
                if (newHighScoreCirclePrefab != null)
                {
                    newHighScoreCirclePrefab.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                        string.Format("{0:00}", highScoreSeconds);
                    newHighScoreCirclePrefab.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text =
                        string.Format("{0:00}", highScoreSeconds);
                }
            }
            else
            {
                if (newHighScoreCirclePrefab != null)
                {
                    newHighScoreCirclePrefab.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                        string.Format("{0:0}:{1:00}", highScoreMinutes, highScoreSeconds);
                    newHighScoreCirclePrefab.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text =
                        string.Format("{0:0}:{1:00}", highScoreMinutes, highScoreSeconds);
                }
            }

            // Spawn NewHighScoreCircle from Prefab
            if (newHighScoreCirclePrefab != null)
                Instantiate(newHighScoreCirclePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            else
                print("Could not find NewHighScoreCirclePrefab");
        }

        private void ReloadLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ReloadWithReset()
        {
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}