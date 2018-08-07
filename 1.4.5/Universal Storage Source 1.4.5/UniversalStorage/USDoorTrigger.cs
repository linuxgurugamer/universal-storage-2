using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniversalStorage
{
    public class USDoorTrigger : MonoBehaviour
    {
        private USAnimateGeneric _animator;

        private void OnTriggerEnter(Collider other)
        {
            USdebugMessages.USStaticLog("Obstruction Entered");
        }
        
        private void OnTriggerExit(Collider other)
        {
            USdebugMessages.USStaticLog("Obstruction Exited");
        }

        public void Init(USAnimateGeneric animator)
        {
            USdebugMessages.USStaticLog("Trigger Detector Added");
            _animator = animator;
        }


    }
}
