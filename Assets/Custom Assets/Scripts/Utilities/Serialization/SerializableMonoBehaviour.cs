using UnityEngine;
using Tomba.Utilities;

public abstract class SerializableMonoBehaviour : MonoBehaviour
{

    [HideInInspector] public virtual string FileExtension { get; protected set;}
    [HideInInspector] public virtual string FileExtensionName { get; protected set; }

    public SerializableMonoBehaviour()
    {
        FileExtension = ".smbd";
        FileExtensionName = "SMBD";
    }

    public abstract void Save(string path);

    public abstract void Load();

}
