// 对话开始事件
public struct DialogueStartEvent
{
    public DialogueData data;
    public DialogueStartEvent (DialogueData data)
    {
        this.data = data;
    }
}

// 对话结束事件
public struct DialogueEndEvent { }