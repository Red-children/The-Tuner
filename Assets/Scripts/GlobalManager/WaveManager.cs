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
        // 可以扩展：精英怪比例、特殊条件等
    }

    public WaveConfig[] waves;           // 配置好的波次（可在Inspector中编辑）
    public Transform[] spawnPoints;      // 生成点
    public float restDuration = 5f;      // 休息阶段时长

    private int currentWaveIndex = 0;
    private int enemiesRemaining = 0;
    public bool isWaveActive = false;
    private bool isResting = false;

    private Room currentRoom;
    private void Start()
    {
    }

    private void Update()
    {
        // 检测波次结束（敌人全部死亡）
        if (isWaveActive && enemiesRemaining <= 0)
        {
            EndWave();
        }
    }

    public void OnEnable()
    {
        EventBus.Instance.Subscribe<EnemyDiedStruct>(DecreasedEnemyNumber);
    }

    public void OnDisable()
    {
        EventBus.Instance.Unsubscribe<EnemyDiedStruct>(DecreasedEnemyNumber);
    }

    #region 开始下一波
    private void StartNextWave()
    {
        if (currentWaveIndex >= waves.Length)
        {
            // 所有波次完成，可进入Boss战或循环
            Debug.Log("所有波次完成！");
            return;
        }

        WaveConfig wave = waves[currentWaveIndex];
        enemiesRemaining = wave.enemyCount;
        isWaveActive = true;
        StartCoroutine(SpawnEnemies(wave));
    }
    #endregion

    #region 生成敌人方法
    private IEnumerator SpawnEnemies(WaveConfig wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            // 随机选择敌人类型
            GameObject enemyPrefab = wave.enemyPrefabs[Random.Range(0, wave.enemyPrefabs.Length)];

            FSM enemyFSM = enemyPrefab.GetComponent<FSM>();
            // 将敌人注册到当前房间
            currentRoom.RegisterEnemy(enemyFSM);

            // 随机选择生成点
            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(enemyPrefab, spawn.position, spawn.rotation);
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }
    #endregion

    #region 波次结束
    private void EndWave()
    {
        isWaveActive = false;
        currentWaveIndex++;
        StartCoroutine(RestPeriod());
    }
    #endregion

    #region 波次休息间隙
    private IEnumerator RestPeriod()
    {
        isResting = true;
        // 触发休息事件（用于UI显示、商店等）
        EventBus.Instance.Trigger(new WaveRestEvent { restDuration = restDuration });
        yield return new WaitForSeconds(restDuration);
        isResting = false;
        StartNextWave();
    }
    #endregion

    #region 敌人数量减少方法
    public void DecreasedEnemyNumber(EnemyDiedStruct t)
    {
        enemiesRemaining--;
    }
    #endregion

    public void StartWave(Room room)
    {
        currentRoom = room;
        currentWaveIndex = 0;          // 从第一波开始
        StartNextWave();
    }
}
