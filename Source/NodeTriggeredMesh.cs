using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;

namespace NearFutureElectrical
{
    public class NodeTriggeredMesh:PartModule
    {
        // Whether to use a staging icon or not
        [KSPField(isPersistant = false)]
        public string MeshName="";
        
        [KSPField(isPersistant = false)]
        public string NodeName = "";

        // Whether to use a staging icon or not
        [KSPField(isPersistant = true)]
        public bool Shown;

        private GameObject meshObject;
        private AttachNode node;

        public override void  OnStart(PartModule.StartState state)

        {
            if (part != null)
            {
                if (MeshName != "")
                {
                    meshObject = part.FindModelTransform(MeshName).gameObject;
                   
                }
                if (NodeName != "")
                {
                    foreach (AttachNode nodes in part.attachNodes)
                    {
                       
                        if (nodes.id == NodeName)
                            node = nodes;
                    }
                }
                if (meshObject == null)
                {
                    Utils.LogError("Couldn't find gameObject " + MeshName);
                }
                else if (node == null)
                {
                    Utils.LogError("Couldn't find stack node " + NodeName);
                }
                else
                {
                    SetVisibility();
                }
            }
            
        }

        public void SetVisibility()
        {
            Utils.Log("Setting Mesh visiblity to" + Shown.ToString());
            if (meshObject)
                meshObject.SetActive(Shown);
        }

        public void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (this.part != null & meshObject != null & node != null)
                {
                    if (node.attachedPart != null)
                    {
                        Shown = true;
                        if (meshObject.activeSelf == false)
                        {
                            
                            SetVisibility();
                        }

                    }
                    if (node.id == NodeName && node.attachedPart == null)
                    {
                        Shown = false;
                        if (meshObject.activeSelf == true)
                        {
                            SetVisibility();
                        }
                    }

                }
            }
        }
    }
}
