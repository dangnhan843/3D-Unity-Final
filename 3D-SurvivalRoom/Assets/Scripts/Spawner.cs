using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Wave[] waves;
    public Enemy enemy;
    public event System.Action<int> OnNewWave;
    public bool devMode;
    LivingEntity playerEntity;
    Transform playerTransform;
    Wave currentWave;

    int currentWaveNumber;
    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;

    bool isCamping;
    bool isDisabled;

    float nextSpawnTime;
    float timeBetwwenCampingChecks = 2.0f;
    float nextCampCheckTime;
    float campThreshHoldDistance = 1.5f;


    Vector3 campPositionOld;
    MapGenerator map;

    void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerTransform = playerEntity.transform;
        nextCampCheckTime = timeBetwwenCampingChecks + Time.time;
        campPositionOld = playerTransform.position;
        playerEntity.OnDeath += OnPlayerDeath;
        map = FindObjectOfType<MapGenerator>();
        NextWave();
    }

    void Update()
    {
        if (!isDisabled)
        {
            if (Time.deltaTime > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + timeBetwwenCampingChecks;
                isCamping = (Vector3.Distance(playerTransform.position, campPositionOld)
                    > campThreshHoldDistance);
                campPositionOld = playerTransform.position;
            }
            if ((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;
                StartCoroutine("SpawnEnemy");
            }
        }

        if (devMode)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StopCoroutine("SpawnEnemy");
                /*foreach(Enemy enemy in FindObjectOfType<Enemy>())
                {
                    GameObject.Destroy(enemy.gameObject);
                }
                */
                NextWave();
            }
        }
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1.0f;
        float tileFlashSpeed = 4.0f;
        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping)
        {
            spawnTile = map.GetTimeFromPosition(playerTransform.position);
        }
        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initalColor = Color.white;
        Color flashColor = Color.red;
        float spawnTimer = 0.0f;

        while (spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initalColor, flashColor, Mathf.PingPong
                (spawnTimer * tileFlashSpeed, 1));
            spawnTimer += Time.deltaTime;
            yield return null;
        }
        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position +Vector3.up,
                Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;
        spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer,
            currentWave.enemyHealth, currentWave.skinColor);
    }
    void OnEnemyDeath()
    {
        enemiesRemainingAlive--;
        if (enemiesRemainingAlive == 0)
        {
            NextWave();
        }
    }

    void OnPlayerDeath()
    {
        isDisabled = true;
    }
    void ResetPlayerPosition ()
    {
        playerTransform.position = map.GetTimeFromPosition(Vector3.zero).position 
            + Vector3.up * 3;

    }
    void NextWave()
    {
        if (currentWaveNumber > 0)
        {
            AudioManager.instance.PlaySound2D("Level Complete");
        }
        currentWaveNumber++;
        if (currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];
            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;
            if (OnNewWave != null)
                {
                OnNewWave(currentWaveNumber);
            }
            ResetPlayerPosition();
        }
    }
    [System.Serializable]
    public class Wave
    {
        public bool infinite;
        public int enemyCount;
        public float timeBetweenSpawns;


        public float moveSpeed;
        public int hitsToKillPlayer;
        public float enemyHealth;
        public Color skinColor;
           
    }
}
