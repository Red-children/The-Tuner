# The Tuner - Game Design Document

## 1. 游戏概述

**项目名称：** The Tuner
**游戏类型：** 俯视角射击 + 节奏地牢探险游戏
**核心玩法：** 玩家在程序生成的地牢中与敌人战斗，通过跟随音乐节拍进行射击来获得伤害加成。游戏融合了弹幕射击、类Rogue元素和节奏游戏机制。

**目标平台：** PC (Windows/Mac/Linux)
**视角：** 俯视角正交 (Top-Down 2D)

---

## 2. 核心游戏循环

1. **进入地牢** → 玩家从起点房间开始
2. **探索房间** → 触发房间事件（战斗/宝箱/商店）
3. **波次战斗** → 在房间内击败所有波次的敌人
4. **获得奖励** → 完成房间后选择Buff或进入下一房间
5. **重复流程** → 直到击败Boss或角色死亡

---

## 3. 节奏系统 (Rhythm System)

### 3.1 系统参数
| 参数 | 数值 |
|------|------|
| BPM | 120 |
| 节拍间隔 | 0.5秒 |
| 预览提前量 | 0.38秒 |
| 输入延迟补偿 | 0.05秒 |

### 3.2 判定等级
| 等级 | 窗口范围 | 伤害倍率 |
|------|----------|----------|
| Perfect | ±50ms | 1.5x |
| Great | ±100ms | 1.2x |
| Good | ±150ms | 1.0x |
| Miss | 窗口外 | 0x |

### 3.3 节奏机制
- 音乐播放时触发节拍预览事件 (BeatPreviewEvent)
- 玩家在节拍窗口内射击可获得伤害倍率加成
- 伤害倍率影响所有武器输出

---

## 4. 玩家系统 (Player)

### 4.1 玩家属性 (PlayerStats)
| 属性 | 默认值 |
|------|--------|
| 最大生命值 | 100 |
| 基础攻击力 | 10 |
| 移动速度 | 5 |
| 和谐能量 | 0-1 |

### 4.2 子系统模块
- **PlayerMovement** - 移动控制
- **PlayerAttack** - 攻击系统
- **PlayerWeapon** - 武器管理
- **PlayerDash** - 冲刺技能
- **PlayerHealth** - 生命值管理
- **PlayerAnimation** - 动画控制

### 4.3 冲刺系统 (PlayerDash)
| 参数 | 数值 |
|------|------|
| 冲刺距离 | 3单位 |
| 冲刺时间 | 0.3秒 |
| 最大能量 | 2 |
| 能量恢复率 | 1/秒 |
| 能量消耗 | 1/次 |

- 冷却期间在节拍窗口内可免费冲刺
- 可通过Buff减少冷却时间

---

## 5. 武器系统 (Weapon System)

### 5.1 武器类型
| 类型 | 特点 |
|------|------|
| Pistol (手枪) | 单发，稳定性高 |
| Shotgun (霰弹枪) | 多发子弹扇形发射 |
| Rifle (步枪) | 高射速，连射 |

### 5.2 武器属性 (WeaponStats)
```csharp
- damage: 基础伤害
- fireRate: 射击间隔
- maxAmmo: 最大弹药量
- attackType: Single(单发) / Multi(多发)
- multiBulletCount: 多发子弹数量
- bulletPrefab: 子弹预制体
- reloadTime: 换弹时间
- shakeIntensity: 后坐力震动强度
```

---

## 6. 敌人系统 (Enemy System)

### 6.1 敌人类型

#### 近战敌人 (MeleeEnemyData)
- 高生命值
- 低移动速度
- 接近玩家后进行近战攻击

#### 远程敌人 (RangedEnemyData)
- 中等生命值
- 保持距离
- 发射子弹攻击玩家

#### 爆炸敌人 (ExplosiveEnemyData)
- 接触玩家时自爆
- 高伤害范围攻击

### 6.2 敌人基础属性
```csharp
- health: 生命值
- moveSpeed: 巡逻速度
- chaseSpeed: 追击速度
- idleTime: 空闲时间
- patrolRadius: 巡逻半径
```

### 6.3 状态机 (FSM)
| 状态 | 描述 |
|------|------|
| Idle | 静止等待 |
| Patrol | 巡逻 |
| Chase | 追击玩家 |
| MeleeApproach | 近战敌人接近 |
| MeleeAttack | 近战攻击 |
| RangedApproach | 远程敌人保持距离 |
| RangedAttack | 远程射击 |
| Wound | 受伤状态 |
| Dead | 死亡 |

---

## 7. 地牢系统 (Dungeon System)

### 7.1 房间类型 (RoomType)
| 类型 | 描述 |
|------|------|
| Start | 起点房间 |
| Normal | 普通战斗房 |
| Elite | 精英敌人房 |
| Treasure | 宝箱房 |
| Shop | 商店 |
| Boss | Boss房 |
| End | 通关 |

### 7.2 地牢生成参数
| 参数 | 数值 |
|------|------|
| 目标房间数 | 10 |
| 房间尺寸 | 20x20 单位 |
| 普通房概率 | 80% |
| 精英房概率 | 20% |

### 7.3 门系统 (Door)
- 每个房间有4个方向的门
- 门的状态: 开启/关闭
- 房间激活时自动关闭
- 清理所有敌人后开启

---

## 8. 波次系统 (Wave System)

### 8.1 波次配置
```csharp
- enemyCount: 每波敌人数
- enemyPrefabs: 可生成的敌人类型数组
- spawnInterval: 生成间隔
```

### 8.2 波次流程
1. 波次开始 → 生成敌人
2. 玩家击杀所有敌人
3. 波次结束 → 下一波或房间完成
4. 房间完成 → 出现Buff选择

---

## 9. Buff系统 (Buff System)

### 9.1 Buff类型
| 类型 | 效果 |
|------|------|
| IncreaseDamage | 增加伤害 |
| IncreaseFireRate | 增加射速 |
| IncreaseMoveSpeed | 增加移动速度 |
| IncreaseMaxHealth | 增加最大生命值 |
| IncreaseArmor | 增加护甲 |
| IncreaseCritRate | 增加暴击率 |
| IncreaseCritDamage | 增加暴击伤害 |
| LifeSteal | 生命偷取 |
| BulletBounce | 子弹弹跳 |
| BulletSplit | 子弹分裂 |
| DashCooldownReduce | 冲刺冷却减少 |
| InvincibleAfterHit | 受伤害后无敌 |

### 9.2 Buff属性
```csharp
- buffName: Buff名称
- description: 描述
- icon: 图标
- value: 效果数值
- type: Buff类型
- isStackable: 是否可叠加
- maxStack: 最大叠加层数
- effectPrefab: 特效预制体
```

---

## 10. 子弹系统 (Bullet)

### 10.2 碰撞层级
| 子弹类型 | 可攻击目标 | 碰撞层级 |
|----------|------------|----------|
| PlayerBullet | Enemy, Wall | PlayerBullet层 |
| EnemyBullet | Player, Wall | EnemyBullet层 |

### 10.3 子弹属性
```csharp
- moveSpeed: 移动速度
- damage: 伤害值
- destroyEffect: 销毁特效
```

### 10.4 碰撞检测
- 使用射线检测进行精确碰撞
- 碰到墙壁直接销毁
- 碰到敌人造成伤害

---

## 11. UI系统 (UI System)

### 11.1 UI组件
| 组件 | 功能 |
|------|------|
| UIWeaponInfo | 武器信息显示 |
| UIComboInfo | 连击信息显示 |
| UIHPController | 生命值显示 |
| UICrosshairController | 准星控制 |
| UIPanelMainMenu | 主菜单面板 |
| UIManager | UI面板管理 |

### 11.2 UI功能
- 生命值条和动画
- 当前武器信息
- 连击计数器和连击条
- 动态准星
- 伤害数字显示 (DamageNumber)

---

## 12. 全局管理器

### 12.1 RhythmManager
- 管理游戏节奏
- 计算判定等级
- 发布节拍预览事件

### 12.2 RoomManager
- 地牢程序化生成
- 房间类型选择
- 门控制

### 12.3 WaveManager
- 波次生成和控制
- 敌人计数
- 波次间Buff发放

### 12.4 ShakeManager
- 屏幕震动效果
- 伤害时触发

### 12.5 ScreenFlashManager
- 屏幕闪烁效果
- 受伤害时触发

---

## 13. 事件系统 (Event Bus)

### 13.1 核心事件
| 事件 | 用途 |
|------|------|
| PlayerHurtEvent | 玩家受伤 |
| PlayerDiedEvent | 玩家死亡 |
| PlayerHealthChangedEvent | 生命值变化 |
| PlayerAtkChange | 攻击力变化 |
| PlayerStatChangedEvent | 属性变化 |
| HarmonyChangedEvent | 和谐能量变化 |
| EnemyDiedStruct | 敌人死亡 |
| AllWavesCompletedEvent | 所有波次完成 |
| WaveRestEvent | 波次休息 |

---

## 14. 预制体资源

### 14.1 玩家相关
| 预制体 | 描述 |
|--------|------|
| Bullet.prefab | 玩家子弹 |

### 14.2 敌人相关
| 预制体 | 描述 |
|--------|------|
| Enemy.prefab | 普通敌人 |
| RangeEnemy.prefab | 远程敌人 |
| EnemyBullet.prefab | 敌人子弹 |
| chesalArea.prefab | 区域敌人 |

### 14.3 房间相关
| 预制体 | 描述 |
|--------|------|
| StatrRoom.prefab | 起点房间 |
| Room.prefab | 普通房间 |
| 90Room.prefab | 90度转角房间 |
| 180Room.prefab | 180度转角房间 |
| 270Room.prefab | 270度转角房间 |
| Corridors.prefab | 走廊 |

### 14.4 UI相关
| 预制体 | 描述 |
|--------|------|
| PanelMainMenu.prefab | 主菜单面板 |
| PanelinBattle.prefab | 战斗面板 |
| Button.prefab | 按钮 |
| UICrosshair.prefab | 准星 |
| DamageNumberCanvas.prefab | 伤害数字 |

### 14.5 特效
| 预制体 | 描述 |
|--------|------|
| Eff.prefab | 通用特效 |

---

## 15. 技术特性

### 15.1 使用的Unity包
- TextMeshPro - 高级文本渲染
- DOTween - 动画和过渡
- Unity 2D SpriteShape - 2D地形编辑

### 15.2 设计模式
- 单例模式 (Managers)
- 状态机模式 (Enemy FSM)
- 事件总线模式 (EventBus)
- 对象池模式 (BuffPool)

### 15.3 碰撞系统
- 2D物理碰撞
- 分层碰撞遮罩
- 射线检测精确碰撞

---

## 16. 场景

| 场景 | 描述 |
|------|------|
| MainMenu.unity | 主菜单场景 |
| TestScene.unity | 测试场景 |

---

## 17. 未来扩展方向

1. **更多武器类型** - 特殊效果武器
2. **更多Buff类型** - 丰富build多样性
3. **Boss设计** - 独特的Boss机制
4. **道具系统** - 可收集使用的道具
5. **成就系统** - 挑战目标
6. **音乐系统增强** - 更多BGM支持
7. **存档系统** - 进度保存

---

*文档生成日期: 2026-03-31*
*基于项目版本: 最新代码分析*
