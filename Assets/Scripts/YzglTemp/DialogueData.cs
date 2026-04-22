using System.Collections.Generic;
using UnityEngine;
//  可序列化标记
[System.Serializable]
public struct DialogueLines
{
    public List<LinesEntry> entries;
}
//  可序列化标记
[System.Serializable]
public struct LinesEntry
{
    public int id;
    public string line;
    public LinesEntry(int id, string line)
    {
        this.id = id;
        this.line = line;
    }
}

[CreateAssetMenu(fileName = "NewDialogueData", menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    [SerializeField] private DialogueLines dialogue = new();
    [SerializeField] private string[] speakers = {"", ""};

#region 接口
    public DialogueLines GetDialogueLines() => dialogue;
    public string[] GetSpeakers() => speakers;
#endregion
}
