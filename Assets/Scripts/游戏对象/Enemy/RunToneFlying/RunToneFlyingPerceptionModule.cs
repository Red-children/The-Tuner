using UnityEngine;

/// <summary>
/// 感知模块 - 处理敌人的感知能力
/// </summary>
public class RunToneFlyingPerceptionModule
{
    private RunToneFlyingInsect owner;
    private RunToneFlyingInsectDataManager dataManager;
    
    /// <summary>
    /// 是否能看到玩家
    /// </summary>
    public bool CanSeePlayer { get; private set; }
    
    /// <summary>
    /// 玩家位置
    /// </summary>
    public Vector3 PlayerPosition { get; private set; }
    
    /// <summary>
    /// 与玩家的距离
    /// </summary>
    public float DistanceToPlayer { get; private set; }
    
    /// <summary>
    /// 初始化感知模块
    /// </summary>
    /// <param name="owner">拥有者</param>
    public RunToneFlyingPerceptionModule(RunToneFlyingInsect owner)
    {
        this.owner = owner;
        this.dataManager = owner.GetComponent<RunToneFlyingInsectDataManager>();
    }
    
    /// <summary>
    /// 更新感知状态
    /// </summary>
    public void Update()
    {
        DetectPlayer();
    }
    
    /// <summary>
    /// 检测玩家
    /// </summary>
    private void DetectPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerPosition = player.transform.position;
            DistanceToPlayer = Vector3.Distance(owner.transform.position, PlayerPosition);
            CanSeePlayer = DistanceToPlayer <= dataManager.DetectionRange;
        }
        else
        {
            CanSeePlayer = false;
            DistanceToPlayer = float.MaxValue;
        }
    }
}

