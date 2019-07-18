using UnityEngine;

public class EditorRenameAttribute : PropertyAttribute
{
    public string NewName { get; private set; }
    public EditorRenameAttribute(string name)
    {
        NewName = name;
    }
}