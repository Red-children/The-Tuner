using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
   
    


    [Header("��������")]
    public Collider2D roomTrigger;          // ��ڴ����������ڼ����ҽ��룩
    public WaveManager waveManager;
    public Door[] doors;
    public LayerMask obstacleMask;

    private Bounds cachedBounds;             // ���淿�䷶Χ�����ڵ������ɣ�
    private bool isActive = false;           // �����Ƿ��ѱ�����
   
    public List<EnemyBase> enemiesInRoom = new List<EnemyBase>();

    private int totalEnemies = 0;    // �����ɱ������
    private int killedCount = 0;     // �ѻ�ɱ��
    private bool isCleared = false;

    public void SetTotalEnemies(int total)
    {
        totalEnemies = total;
        killedCount = 0;
    }

    public void UnregisterEnemy(EnemyBase enemy)
    {
        enemiesInRoom.Remove(enemy);
        killedCount++;
        if (killedCount >= totalEnemies && !isCleared)
        {
            isCleared = true;
            OnRoomCleared();
        }
    }


    public void Init(RoomType roomType)
    {
        
    }



    private void Awake()
    {
        if (roomTrigger == null) roomTrigger = GetComponent<Collider2D>();
        if (waveManager == null) waveManager = GetComponent<WaveManager>();

        // ���淿�䷶Χ�����������ú��Կ�ʹ�ã�
        cachedBounds = roomTrigger.bounds;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isActive) return;  // �Ѿ�������������ظ�����

        ActivateRoom(other.transform);
    }

    private void ActivateRoom(Transform player)
    {
        if(isCleared) return;

        if(isActive) return;

        isActive = true;

        // ����
        foreach (var door in doors) door?.Close();

        // �����������е��ˣ������Ԥ�ȷ��õĵ��ˣ�
        foreach (var enemy in enemiesInRoom)
        {
          
            if (enemy != null )
            {
                enemy.target = player;
            }
        }

        // ��������
        waveManager?.StartWave(this);

        // ��ѡ�����ô������������ٴν��루Ҳ�ɱ�������Ϊ�� isActive ������
        // roomTrigger.enabled = false;
    }

    #region �õ������ڵ������
    //�õ���������������ڵ�������
    public Vector2 GetRandomValidPoint(float safeRadius = 0.5f)
    {
        // ʹ�û���ķ��䷶Χ
        for (int i = 0; i < 100; i++)
        {
            float x = Random.Range(cachedBounds.min.x, cachedBounds.max.x);
            float y = Random.Range(cachedBounds.min.y, cachedBounds.max.y);
            Vector2 point = new Vector2(x, y);

            // �����Ƿ��ڴ�����ԭʼ��Χ�ڣ�ʹ�� cachedBounds ������ȷ�������ã�
            // ���������״�����򣬽��鱣�� roomTrigger �� OverlapPoint �� roomTrigger ���ܱ�����
            // �����û���ı߽������жϣ����ھ��η����㹻��
            // �����Ҫ��ȷ�жϣ������� Awake ʱ��¡һ�����ص���ײ��ר�����ڼ�⡣

            Collider2D[] hits = Physics2D.OverlapCircleAll(point, safeRadius, obstacleMask);
            if (hits.Length == 0)
                return point;
        }
        return cachedBounds.center;
    }
    #endregion

    // ����ע��/ע���������ֲ���...
    public void RegisterEnemy(EnemyBase enemy) => enemiesInRoom.Add(enemy);

    #region ��շ������
    private void OnRoomCleared()
    {
        foreach (var door in doors) door?.Open();
        isActive = false;  // �������ã����������ٴν��루�����Ҫ��
        Debug.Log("��������գ����Ѵ�");
    }
    #endregion

    // ���ݷ����ȡ��Ӧ����
    public Door GetDoor(Door.Direction dir)
    {
        foreach (var door in doors)
        {
            if (door.direction == dir)
                return door;
        }
        return null;
    }

    // ��ȡ���п��õ��ţ����ڵ�ͼ���ɣ��ɸ�����Ҫɸѡ��
    public List<Door> GetAvailableDoors()
    {
        return new List<Door>(doors);
    }


}