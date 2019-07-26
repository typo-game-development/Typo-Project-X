/*
    Author: Francesco Podda
    Date: 25/07/2019
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tomba.Items;

namespace Tomba
{
    [System.Serializable]
    public class CharacterData
    {
        public Weapon equippedWeapon;
        public List<Weapon> availableWeapons;

    }
}

