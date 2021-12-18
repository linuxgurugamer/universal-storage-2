
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniversalStorage2
{
    public class USModuleGreeble : PartModule
    {
        [KSPField]
        public string BottomNodeName = "";
        [KSPField]
        public bool CheckBottomNode = true;
        [KSPField]
        public bool AllowGreebleToggle = true;
        [KSPField]
        public string GreebleToggleName = "Toggle Details";
        [KSPField]
        public string GreebleTransform = "";
        [KSPField(isPersistant = true)]
        public bool IsActive = true;

        private List<AttachNode> bottomNodes = new List<AttachNode>();
        private List<Transform> greebles = new List<Transform>();

        private bool editor = false;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (string.IsNullOrEmpty(GreebleTransform))
                return;

            greebles = part.FindModelTransforms(GreebleTransform).ToList();

            if (!string.IsNullOrEmpty(BottomNodeName))
            {
                string[] nodes = BottomNodeName.Split(';');

                for (int i = nodes.Length - 1; i >= 0; i--)
                {
                    AttachNode node = part.FindAttachNode(nodes[i]);

                    if (node != null)
                        bottomNodes.Add(node);
                }
            }
            else
                bottomNodes = null;

            if (IsActive)
            {
                if (CheckBottomNode && bottomNodes != null && bottomNodes.Count > 0)
                {
                    for (int i = bottomNodes.Count - 1; i >= 0; i--)
                    {
                        AttachNode node = bottomNodes[i];

                        if (node == null)
                            continue;

                        if (node.radius < 0.01f)
                            continue;

                        if (node.attachedPart == null)
                        {
                            for (int j = greebles.Count - 1; j >= 0; j--)
                                    greebles[j].gameObject.SetActive(false);
                        }
                        else
                        {
                            for (int j = greebles.Count - 1; j >= 0; j--)
                                    greebles[j].gameObject.SetActive(true);
                        }
                    }
                }
                else
                {
                    for (int i = greebles.Count - 1; i >= 0; i--)
                        greebles[i].gameObject.SetActive(true);
                }
            }
            else
            {
                for (int j = greebles.Count - 1; j >= 0; j--)
                    greebles[j].gameObject.SetActive(false);
            }

            if (state == StartState.Editor)
            {
                Events["ToggleGreeble"].active = AllowGreebleToggle;
                Events["ToggleGreeble"].guiName = GreebleToggleName;
                editor = true;
            }
            else
            {
                Events["ToggleGreeble"].active = false;
                editor = false;
            }
            
        }

        [KSPEvent(name = "ToggleGreeble", guiName = "Toggle Details", guiActive = false, guiActiveUnfocused = false, guiActiveEditor = true, active = true)]
        public void ToggleGreeble()
        {
            IsActive = !IsActive;

            for (int i = greebles.Count - 1; i >= 0; i--)
                greebles[i].gameObject.SetActive(IsActive);
        }

        private void LateUpdate()
        {
            if (!editor)
                return;

            if (!CheckBottomNode || bottomNodes == null || bottomNodes.Count <= 0)
                return;

            if (!IsActive)
                return;

            for (int i = bottomNodes.Count - 1; i >= 0; i--)
            {
                AttachNode node = bottomNodes[i];

                if (node == null)
                    continue;

                if (node.radius < 0.01f)
                    continue;
                
                if (node.attachedPart == null)
                {
                    for (int j = greebles.Count - 1; j >= 0; j--)
                    {
                        if (greebles[j].gameObject.activeSelf)
                            greebles[j].gameObject.SetActive(false);
                    }
                }
                else
                {
                    for (int j = greebles.Count - 1; j >= 0; j--)
                    {
                        if (!greebles[j].gameObject.activeSelf)
                            greebles[j].gameObject.SetActive(true);
                    }
                }
            }
        }
    }
}
