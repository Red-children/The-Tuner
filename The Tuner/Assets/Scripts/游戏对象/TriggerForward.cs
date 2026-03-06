using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerForward : MonoBehaviour
{
   
        public FSM fsm; // 在 Inspector 中拖入子对象的 FSM

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                fsm?.OnPlayerEnter(other.transform);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                fsm?.OnPlayerExit(other.transform);
            }
        }
    
}
