using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoUI : MonoBehaviour
{
    public Colors colors;
}

[System.Serializable]
public struct Colors
{
    public Color root;
    public Color dark; 
    public Color light;
    public Color border;
    public Color secondary;
}
