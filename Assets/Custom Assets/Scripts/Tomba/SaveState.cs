/*
    Author: Francesco Podda
    Date: 25/07/2019
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tomba
{
    /// <summary>
    /// Base class to store saved data.
    /// </summary>
    [System.Serializable]
    public class SaveState
    {
        [HideInInspector]
        public int ID = -1;

        [ShowOnly] public string name;

        [HideInInspector]
        public Texture thumbnail = null;

        [HideInInspector]
        public Vector2 thumbnailSize = new Vector2(256, 144);

        [HideInInspector]
        public string areaName = "???";

        [ShowOnly] public string SaveDate = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");


    }
}

