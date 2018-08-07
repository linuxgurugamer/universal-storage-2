using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniversalStorage
{
    public class USDoorTest : PartModule
    {
        private USdebugMessages debug;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            debug = new USdebugMessages(true, "USRayCheck");
        }

        [KSPEvent(name = "DoorTrigger", guiName = "Door Trigger", guiActive = true, guiActiveEditor = true, active = true)]
        public void DoorTrigger()
        {
            debug.debugMessage("Start Door Ray Check...");

            RayCheck();
        }

        private bool RayCheck()
        {
            Vector3 dir;

            dir = (part.parent.partTransform.position - part.partTransform.position).normalized;

            dir *= -1;

            debug.debugMessage("Ray Direction: " + dir.ToString());

            RaycastHit hit;

            if (Physics.Raycast(part.partTransform.position, dir, out hit, 5f, LayerUtil.DefaultEquivalent))
            {
                debug.debugMessage("Ray hit");

                if (hit.collider != null)
                {
                    debug.debugMessage("Ray hit collider - Name: " + hit.collider.gameObject.name);

                    if (hit.collider.gameObject.tag == "NoAttach")
                    {
                        debug.debugMessage("Ray hit NoAttach");

                        bool primary = false;
                        bool secondary = false;

                        if (hit.collider.gameObject.name == "PrimaryDoorCollider")
                            primary = true;
                        else if (hit.collider.gameObject.name == "SecondaryDoorCollider")
                            secondary = true;

                        if (primary || secondary)
                        {
                            debug.debugMessage("Door Detected");

                            Part p = Part.GetComponentUpwards<Part>(hit.collider.gameObject);

                            if (p != null)
                            {
                                debug.debugMessage("Part from GameObject: " + p.partInfo.name);

                                PartModule USAnimate = null;

                                for (int i = p.Modules.Count - 1; i >= 0; i--)
                                {
                                    if (p.Modules[i].moduleName == "USAnimateGeneric")
                                    {
                                        USAnimate = p.Modules[i];
                                        break;
                                    }
                                }

                                if (USAnimate != null)
                                {
                                    debug.debugMessage("US Animate Module Detected");

                                    BaseEvent doorEvent = null;

                                    if (primary)
                                        doorEvent = USAnimate.Events["toggleEventPrimary"];
                                    else if (secondary)
                                        doorEvent = USAnimate.Events["toggleEventSecondary"];

                                    if (doorEvent != null)
                                    {
                                        debug.debugMessage("Door Event Found");

                                        if (doorEvent.active && doorEvent.guiActive)
                                        {
                                            doorEvent.Invoke();

                                            debug.debugMessage("Door Invoked");

                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }        

    }
}
