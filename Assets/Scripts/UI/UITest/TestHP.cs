using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestHP : MonoBehaviour
{

    //  玩家最大生命值
    private float _maxHP = 100;
    //  玩家上一次生命值变化时的生命值
    private float _lastHP = 100;
    void Start()
    {
        PlayerHealthChangedEventStruct evt = new PlayerHealthChangedEventStruct
        {
            currentHealth = _lastHP,
            maxHealth = _maxHP
        };
        EventBus.Instance.Trigger<PlayerHealthChangedEventStruct>(evt);
    }
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            _lastHP += 1f;
            _lastHP = Mathf.Clamp(_lastHP, 0, 100);
            PlayerHealthChangedEventStruct evt = new PlayerHealthChangedEventStruct
            {
                currentHealth = _lastHP,
                maxHealth = _maxHP
            };
            EventBus.Instance.Trigger<PlayerHealthChangedEventStruct>(evt);
            Debug.Log("TestHP:Mouse Right Button Down\n" + 
            $"newHP:{_lastHP}, maxHP:{_maxHP}");
        }
        else if (Input.GetMouseButtonDown(1))
        {
            _lastHP -= 1f;
            _lastHP = Mathf.Clamp(_lastHP, 0, 100);
            PlayerHealthChangedEventStruct evt = new PlayerHealthChangedEventStruct
            {
                currentHealth = _lastHP,
                maxHealth = _maxHP
            };
            EventBus.Instance.Trigger<PlayerHealthChangedEventStruct>(evt);
            Debug.Log("TestHP:Mouse Right Button Down\n" + 
            $"newHP:{_lastHP}, maxHP:{_maxHP}");
        }
    }
}
