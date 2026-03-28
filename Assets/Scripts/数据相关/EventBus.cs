using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




//事件中心架构
//即事件的发出者不在乎谁受到了影响，而事件的受影响者也不在乎是谁发出了，他只关注产生对应的影响
//事件中心充当他们交流的踏板，这里存在时事件的多播
public class EventBus
{
    //单例模式
    private static EventBus instance;

    public static EventBus Instance 
    {
        get 
        {
            if(instance == null) 
            {
                instance = new EventBus();
            }
            return instance;
        }
    }

    //维护一个事件字典用于存储所有的触发事件
    private Dictionary<Type, Delegate> _events = new Dictionary<Type, Delegate>();


    // 订阅事件：所有事件都使用 Action<T> 委托
    // 这里的泛型规定为是结果体是为了方便数据的传递
    // 即所有的相关数据我们都保存为结构体来传输
    public void Subscribe<T>(Action<T> handler) where T : struct
    {
        Type type = typeof(T);
        //多线程安全，没啥用
        lock (_events)
        {
            //判断对应的事件是否已经存在
            if (_events.ContainsKey(type))
                //加入该方法
                _events[type] = Delegate.Combine(_events[type], handler);
            else
                _events[type] = handler;

        }
    }

    // 取消订阅
    //同理
    public void Unsubscribe<T>(Action<T> handler) where T : struct
    {
        Type type = typeof(T);
        lock (_events)
        {
            if (_events.TryGetValue(type, out Delegate del))
            {
                //拿出对应的函数
                del = Delegate.Remove(del, handler);
                //没有函数能被移除，即字典为空
                if (del == null)
                    _events.Remove(type);
                else
                    _events[type] = del;
            }
        }
    }

    // 触发事件：传递事件数据实例
    public void Trigger<T>(T eventData) where T : struct
    {
        Type type = typeof(T);
        Delegate del;
        lock (_events)
        {
            //取出对应的委托
            if (!_events.TryGetValue(type, out del))
                return;
        }
        // 安全调用
        (del as Action<T>)?.Invoke(eventData);
    }

}
