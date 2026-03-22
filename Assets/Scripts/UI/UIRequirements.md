---
marp: true
theme: gaia
math: katex
paginate: true
style: |
  /* 移除无关的image样式（不影响单结构体展示） */
  .image {
    display: none;
  }
  /* 每张幻灯片整体居中，内容垂直居中 */
  section {
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    height: 100%;
    padding: 20px;
    margin: 0;
  }
  /* 标题样式（可选，增强幻灯片辨识度） */
  h2 {
    color: #333;
    margin: 0 0 20px 0;
    font-size: 24px;
    font-weight: 600;
  }
  /* 核心：大字体黑底白字代码块 + 保留制表符 */
  pre {
    white-space: pre; /* 保留制表符/空格/换行 */
    white-space: pre-wrap; /* 自动换行不溢出 */
    tab-size: 4; /* 制表符宽度 */
    -moz-tab-size: 4;
    -o-tab-size: 4;
    width: 85%; /* 单结构体占满幻灯片宽度（适配） */
    font-size: 160px; /* 大字体 */
    font-family: Consolas, "Microsoft YaHei", monospace; /* 中英文适配 */
    line-height: 1.6; /* 行高舒适 */
    padding: 25px; /* 充足内边距 */
    background: #1e1e1e; /* 黑底 */
    color: #f8f8f2; /* 白字 */
    border-radius: 8px; /* 圆角更美观 */
    box-shadow: 0 4px 12px rgba(0,0,0,0.3); /* 阴影增强层次感 */
  }
  /* 语法高亮优化 */
  code .keyword { color: #569cd6; }    /* 关键字：蓝色 */
  code .comment { color: #6a9955; }    /* 注释：绿色 */
  code .string { color: #ce9178; }     /* 字符串：橙色 */
  code .number { color: #b5cea8; }     /* 数字：浅绿 */
  /* 确保代码样式继承 */
  code {
    tab-size: inherit;
    white-space: inherit;
    color: inherit;
    background: transparent;
    font-size: inherit;
    font-family: inherit;
  }

---
其中 ID是快捷键位,没变
```csharp
public struct ChangeWeaponStruct
{
    public int lastWeaponID;
    public int currentWeaponID;
    public ChangeWeaponStruct(int lastID, int currentID)
    {
        lastWeaponID = lastID;
        currentWeaponID = currentID;
    }
}
```

---
没变
```csharp
public struct PlayerHealthChangedEventStruct
{
    public float currentHealth;     //  变化后血量  
    public float maxHealth;         //  血量最大值  
    public float healthPercent => currentHealth / maxHealth;
}
```
---
<!-- （ 新增）现通过Find("Player") + GetComponent()直接获得引用，可以不实现。 -->
切换武器的业务逻辑有问题导致UI响应变化在0.5s延迟后。
正确逻辑是切武器进冷却计时而不是延迟后切换
```csharp
//  TODO:   已直接获取引用
//  射击和换弹时广播弹药变化
public struct ChangeAmmoCapEvent
{
    public int currentAmmo; //  Capacity Before shooting
    public int nextAmmo;    //  Capacity After shooting
    public int reserveAmmo; //  备弹
    public int weaponId;    //  
}
```

---
换弹时的UI动画要用
```csharp
//  换弹时通知UI播放动画
public struct PlayerReloadEvent
{
    public float duration;  //  换弹持续时间()
}
```
---
已更新在Scripts/UI/UIEvents.cs中
后面再遇到数据传输问题会在这里更新