#region 连击数据结构
public struct ComboData
{
    public int CurrentCombo { get; private set; }
    public ComboEffect[] Effects { get; private set; }
    public bool HasEffects => Effects != null && Effects.Length > 0;

    public ComboData(int currentCombo, ComboEffect[] effects)
    {
        CurrentCombo = currentCombo;
        Effects = effects;
    }
}
#endregion

#region 连击事件
public struct ComboChangedEvent
{
    public ComboData ComboData { get; private set; }

    public ComboChangedEvent(ComboData comboData)
    {
        ComboData = comboData;
    }
}

public struct ComboBreakEvent
{
    public int FinalCombo { get; private set; }

    public ComboBreakEvent(int finalCombo)
    {
        FinalCombo = finalCombo;
    }
}
#endregion