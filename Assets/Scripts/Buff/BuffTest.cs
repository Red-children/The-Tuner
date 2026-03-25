using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffTest : MonoBehaviour
{
        public BuffData BuffDatas;
        public BuffData testBuff;  // 在Inspector中拖拽一个BuffData
        public PlayerIObject player;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                var buffManager = player.GetComponent<BuffManager>();
                if (buffManager != null)
                    buffManager.AddBuff(testBuff);
            }
        if (Input.GetKeyDown(KeyCode.B))
        {
            var buffManager = player.GetComponent<BuffManager>();
            if (buffManager != null)
                buffManager.AddBuff(testBuff);
        }
    }
    
}
