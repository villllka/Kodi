using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Kodi/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [SerializeField] private string speakerName;

    [TextArea(2, 6)]
    [SerializeField] private List<string> lines = new List<string>();

    public string SpeakerName => speakerName;
    public List<string> Lines => lines;
}