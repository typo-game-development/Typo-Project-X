using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RailingSystem
{
    public class SwitchColliderTrigger : MonoBehaviour
    {
        public bool enter;
        public bool exit;
        public bool stay;
        public string checkTag = "";

        private void OnTriggerEnter(Collider other)
        {

            if (other.tag == checkTag)
            {
                enter = true;
                exit = false;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == checkTag)
            {
                enter = false;
                exit = true;
                stay = false;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.tag == checkTag)
            {
                stay = true;
            }
        }
    }
}

