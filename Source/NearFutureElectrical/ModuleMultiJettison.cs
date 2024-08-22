using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NearFutureElectrical
{
  public class ModuleMultiJettison : PartModule, IPartMassModifier
  {

    [KSPField(isPersistant = false)]
    public string fairingTransformName = "fairing";

    [KSPField(isPersistant = false)]
    public Vector3 jettisonLocalDirection = Vector3.forward;

    [KSPField(isPersistant = false)]
    public float jettisonForce = 1.0f;

    [KSPField(isPersistant = false)]
    public float fairingTotalMass = 0.01f;

    [KSPField(isPersistant = true)]
    public bool isJettisoned = false;

    [KSPField(isPersistant = true,
      guiActiveEditor = true,
      guiName = "#LOC_NFElectrical_ModuleMultiJettison_Event_Fairing_Name"),
      UI_Toggle(affectSymCounterparts = UI_Scene.Editor, disabledText = "#LOC_NFElectrical_ModuleMultiJettison_Event_Fairing_Disabled", enabledText = "#LOC_NFElectrical_ModuleMultiJettison_Event_Fairing_Enabled")]
    public bool fairingEnabled = false;


    [KSPField(isPersistant = false)]
    public string jettisonGUIName = "#LOC_NFElectrical_ModuleMultiJettison_Event_Jettision";

    [KSPField(isPersistant = false)]
    public string fxGroupName = "jettison";


    /// KSPEvents
    [KSPEvent(guiActive = true,
      guiName = "#LOC_NFElectrical_ModuleMultiJettison_Event_Jettision")]
    public void Jettison()
    {
      if (fairingEnabled && !isJettisoned)
        DoJettison();

    }

    /// KSPActions
    [KSPAction("Deploy")]
    public void JettisonAction(KSPActionParam param)
    {
      Jettison();
    }

    private Transform[] fairingTransforms;



    public override void OnStart(StartState state)
    {
      fairingTransforms = part.FindModelTransforms(fairingTransformName);
      if (fairingTransforms.Length == 0)
      {
        Debug.LogWarning($"[ModuleMultiJettision] Could not find any transforms with name {fairingTransformName} on {part.partInfo.title}");
      }
      SetStartState();

      Fields[nameof(fairingEnabled)].uiControlEditor.onFieldChanged = OnEditorToggleJettison;

      Events[nameof(Jettison)].active = !isJettisoned;
      Events[nameof(Jettison)].guiName = jettisonGUIName;
      Actions[nameof(JettisonAction)].active = !isJettisoned;

      base.OnStart(state);
    }

    private void OnEditorToggleJettison(BaseField field, object oldValue)
    {
      if (!HighLogic.LoadedSceneIsEditor)
        return;

      SetFairingEnabled(fairingEnabled);
    }


    protected void DoJettison()
    {

      if (fairingTransforms.Length == 0)
      {
        return;
      }

      List<Collider> partColliders = base.part.FindModelComponents<Collider>();

      for (int i = 0; i < fairingTransforms.Length; i++)
      {
        Vector3 d = jettisonLocalDirection;
        if (d == Vector3.zero)
        {
          d = Vector3.Normalize(fairingTransforms[i].transform.position - part.transform.position);
        }
        else
        {
          d = fairingTransforms[i].TransformDirection(d);
        }

        fairingTransforms[i].SetParent(null);

        Debug.Log("Jettisoned object does not have a collider, creating a Convex Mesh Collider for it");
        MeshCollider meshCollider = fairingTransforms[i].gameObject.AddComponent<MeshCollider>();
        meshCollider.convex = true;

        SetIgnoreCollisionFlags(meshCollider, partColliders);

        //meshCollider.enabled;

        Rigidbody partRB = part.Rigidbody;

        if (partRB != null)
        {
          Rigidbody rb = physicalObject.ConvertToPhysicalObject(part, fairingTransforms[i].gameObject).rb;
          part.ResetCollisions();
          
          rb.maxAngularVelocity = PhysicsGlobals.MaxAngularVelocity;
          rb.angularVelocity = part.Rigidbody.angularVelocity;
          rb.velocity = part.Rigidbody.velocity + Vector3.Cross(part.Rigidbody.worldCenterOfMass - vessel.CurrentCoM, vessel.angularVelocity);
          rb.mass = fairingTotalMass / fairingTransforms.Length;

          

          rb.AddForceAtPosition(d * (jettisonForce * 0.5f), part.transform.position, ForceMode.Force);
          part.Rigidbody.AddForce(d * (-jettisonForce * 0.5f), ForceMode.Force);
        }
      }

      isJettisoned = true;


      Events[nameof(Jettison)].active = false;
      Actions[nameof(JettisonAction)].active = false;

      FXGroup effect = part.findFxGroup(fxGroupName);
      if (effect != null)
      {
        effect.Burst();
      }

      GameEvents.onVesselWasModified.Fire(vessel);
    }

    private void SetIgnoreCollisionFlags(Collider col, List<Collider> set2)
    {
      for (int i = 0; i < set2.Count; i++)
      {
        Physics.IgnoreCollision(col, set2[i]);
      }
    }

    private void SetFairingEnabled(bool on)
    {
      SetFairingMeshVisibility(on);

      Events[nameof(Jettison)].active = on;
      Actions[nameof(JettisonAction)].active = on;
    }


    private void SetStartState()
    {
      if (!fairingEnabled || isJettisoned)
      {
        SetFairingMeshVisibility(false);
        Events[nameof(Jettison)].active = false;
        Actions[nameof(JettisonAction)].active = false;
      }
      else if (fairingEnabled && isJettisoned)
      {
        SetFairingMeshVisibility(false);
        Events[nameof(Jettison)].active = false;
        Actions[nameof(JettisonAction)].active = false;
      }
      else
      {
        SetFairingMeshVisibility(true);
        Events[nameof(Jettison)].active = true;
        Actions[nameof(JettisonAction)].active = true;
      }
    }

    private void SetFairingMeshVisibility(bool on)
    {
      for (int i = 0; i < fairingTransforms.Length; i++)
      {
        fairingTransforms[i].gameObject.SetActive(on);
      }
    }


    public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
    {
      float mass = 0f;
      return mass;
    }

    public ModifierChangeWhen GetModuleMassChangeWhen()
    {
      return ModifierChangeWhen.STAGED;
    }
  }
}