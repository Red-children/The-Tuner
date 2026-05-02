// 对话开始事件
public struct DialogueStartEvent
{
    public DialogueData data;
    public bool freeze;
    public DialogueStartEvent (DialogueData data, bool freeze = false)
    {
        this.data = data;
        this.freeze = freeze;
    }
}

// 对话结束事件 不是对话面板关闭事件
public struct DialogueEndEvent { }