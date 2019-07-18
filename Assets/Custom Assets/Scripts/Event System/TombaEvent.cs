

namespace Tomba
{
    [System.Serializable]
    public class TombaEvent
    {
        [ShowOnly] public int eventType = -1;
        [ShowOnly] public string name = "";
        [ShowOnly] public string description = "";
        [ShowOnly] public bool cleared = false;
        [ShowOnly] public bool occurred = false;
        [ShowOnly] public int occurrAPGain = 0;
        [ShowOnly] public int clearAPGain = 0;
    }

}
