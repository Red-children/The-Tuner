using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueData", menuName = "BGM/BGMDetail")]
public class BGMData : ScriptableObject
{
    [SerializeField] private double firstOffset;
    [SerializeField] private int BPM;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private bool loop = true;

    public double GetFirstOffset() => firstOffset;
    public int GetBPM() => BPM;
    public AudioClip GetAudioClip() => audioClip;
    public bool GetLoop() => loop;
}
