using UnityEngine;

[System.Serializable]
public class PlayerMovementRailConnection /*: MonoBehaviour*/
{
    public enum eConnectionDirection
    {
        P1_TO_P2,
        P2_TO_P1,
        BIDIRECTIONAL
    }

    public string name = "Connection";

    public GameObject point1;
    public GameObject point2;

    public eConnectionDirection direction;
    public bool autoTransition;

    [HideInInspector]
    public string[] point1Options = new string[] { "Test1"};

    [HideInInspector]
    public string[] point2Options = new string[] { "Test1" };

    [HideInInspector]
    public int point1SelectedOptionIndex;

    [HideInInspector]
    public int point2SelectedOptionIndex;

    [HideInInspector]
    public int directionSelectedOptionIndex;
}
