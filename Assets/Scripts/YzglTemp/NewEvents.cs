// 对话开始事件
public struct DialogueStartEvent
{
    public IDialogueTrigger trigger;
    public DialogueStartEvent (IDialogueTrigger trigger)
    {
        this.trigger = trigger;
    }
}

// 对话结束事件
public struct DialogueEndEvent { }