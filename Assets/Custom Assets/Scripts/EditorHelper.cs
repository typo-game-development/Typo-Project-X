using UnityEngine;

public static class EditorHelper
{
    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#endif
    }

    public static void Start()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = true;

#endif

    }  
}