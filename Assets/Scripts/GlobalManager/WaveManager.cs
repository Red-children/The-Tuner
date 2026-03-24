using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;



public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class WaveConfig
    {
        public int enemyCount;          // 本波敌人数
        public GameObject[] enemyPrefabs; // 可生成的敌人类型
        public float spawnInterval;      // 生成间隔
    }

    public WaveConfig[] waves;           // 配置好的波次

    private int currentWaveIndex = 0;
    private int enemiesRemaining = 0;
    public bool isWaveActive = false;




    private Room currentRoom;

    private void Update()
    {
        // 检测波次结束（当前波敌人全部死亡）
        if (isWaveActive && enemiesRemaining <= 0)
        {
            EndWave();
        }
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<EnemyDiedStruct>(DecreasedEnemyNumber);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<EnemyDiedStruct>(DecreasedEnemyNumber);
    }

    #region 开始波次方法
    // 由房间调用，开始第一波
    public void StartWave(Room room)
    {
        currentRoom = room;
        currentWaveIndex = 0;

        //把需要的击杀数传入 用来防止跨波次时房门被打开
        int total = 0;
        foreach (var wave in waves)
            total += wave.enemyCount;
        currentRoom.SetTotalEnemies(total);

        StartNextWave();
    }
    #endregion

    #region 开始下一波
    private void StartNextWave()
    {

        if (currentWaveIndex >= waves.Length)
        {
            // 所有波次完成，停止活动
            isWaveActive = false;
            Debug.Log("所有波次完成");
            return;
        }

        WaveConfig wave = waves[currentWaveIndex];
        enemiesRemaining = wave.enemyCount;
        isWaveActive = true;
        StartCoroutine(SpawnEnemies(wave));
    }
    #endregion

    private IEnumerator SpawnEnemies(WaveConfig wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            Vector2 spawnPos = currentRoom.GetRandomValidPoint();
            GameObject enemyPrefab = wave.enemyPrefabs[Random.Range(0, wave.enemyPrefabs.Length)];
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            EnemyController enemyController = enemyObj.GetComponent<TriggerForward>().controller;
            if (enemyController != null)
            {
                enemyController.ownerRoom = currentRoom;   // 设置所属房间
                currentRoom.RegisterEnemy(enemyController);
            }
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    private void EndWave()
    {
        currentWaveIndex++;
        // 立即开始下一波（如果有），否则停止
        if (currentWaveIndex < waves.Length)
        {
            StartNextWave();
        }
        else
        {
            isWaveActive = false;
        }
    }

    public void DecreasedEnemyNumber(EnemyDiedStruct t)
    {
        enemiesRemaining--;
    }
}