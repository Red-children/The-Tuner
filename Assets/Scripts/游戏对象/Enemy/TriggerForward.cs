//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class TriggerForward : MonoBehaviour
//{
   
//    public EnemyController controller; // 在 Inspector 中拖入子对象的 FSM
//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        if (other.CompareTag("Player"))
//        {
           
//            controller?.OnPlayerEnter(other.transform);

//        }
//    }
//        private void OnTriggerExit2D(Collider2D other)
//        {
//            if (other.CompareTag("Player"))
//            {
//                controller?.OnPlayerExit(other.transform);
//            }
//        }
//    private void Update()
//    {
//        if (controller == null) 
//        {
//            Destroy(gameObject);
//        }
//    }

//}
