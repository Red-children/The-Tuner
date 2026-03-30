//using UnityEngine;

//public class EnemyAnimationEvents : MonoBehaviour
//{
//    private FSM fsm;

//    void Awake()
//    {
//        // 从子物体向上查找父级上的 FSM 组件（因为 FSM 在根物体上）
//        fsm = GetComponentInParent<FSM>();
//        if (fsm == null)
//            Debug.LogError("EnemyAnimationEvents: 找不到父级的 FSM 组件！");
//    }

//    //public void OnAttackHit()
//    //{
        
//    //    if (fsm.currentState is EnemyMeleeAttackState meleeState)
//    //        meleeState.OnAttackHit();
//    //    else if (fsm.currentState is EnemyRangedAttackState rangedState)
//    //        rangedState.OnAttackHit();
//    //    // 其他攻击状态可继续添加
//    //}

//    //public void OnAttackEnd()
//    //{
//    //    if (fsm.currentState is EnemyMeleeAttackState meleeState)
//    //        meleeState.OnAttackEnd();
//    //    // 其他攻击结束可扩展
//    //}
//}