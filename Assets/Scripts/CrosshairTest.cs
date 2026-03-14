using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairTest : MonoBehaviour
{
    public UICrosshairController parent;
    void endAnim()
    {
        double dspStart = parent._dspStartTime;
        Debug.Log($"Animation End at{AudioSettings.dspTime - dspStart}");
    }
    void beginAnim()
    {
        double dspStart = parent._dspStartTime;
        Debug.Log($"Animation Start at{AudioSettings.dspTime - dspStart}");
    }
}
