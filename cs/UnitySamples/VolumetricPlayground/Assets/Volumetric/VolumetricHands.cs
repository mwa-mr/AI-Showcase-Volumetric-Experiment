using Microsoft.MixedReality.Volumetric;
using System;
using System.Collections.Generic;
using UnityEngine;

public class VolumetricHands : MonoBehaviour
{
    public Transform HandsContainer;
    public bool DrawDebugJoints = false;
    private List<GameObject> debugJoints = new List<GameObject>();

    [Serializable]
    public class Joint
    {
        public string Name;
        public Transform JointTransform;
    }

    [Serializable]
    public class Hand
    {
        public string Name;
        public List<Joint> joints = new List<Joint>();
    }

    public GameObject[] HandObjects = { null, null };

    [SerializeField]
    private Hand[] hands = { new Hand { Name = "Left" }, new Hand { Name = "Right" } };
    private string[] hand_prefixes = { "L_", "R_" };
    private GameObject[] handMeshes = { null, null };


    void Start()
    {
        if (HandsContainer == null)
        {
            HandsContainer = transform;
        }

        HandObjects[0]?.SetActive(false);
        HandObjects[1]?.SetActive(false);

        handMeshes[0] = HandObjects[0].GetComponentInChildren<SkinnedMeshRenderer>()?.gameObject;
        handMeshes[1] = HandObjects[1].GetComponentInChildren<SkinnedMeshRenderer>()?.gameObject;

        // Only used for initialization - should be saved in prefab
        if (hands[0].joints.Count == 0 || hands[0].joints[0].JointTransform == null)
        {

            var mat = new PhysicMaterial();
            mat.bounciness = 0.1f;
            mat.dynamicFriction = 0.5f;
            mat.staticFriction = 0.5f;
            mat.frictionCombine = PhysicMaterialCombine.Maximum;
            mat.bounceCombine = PhysicMaterialCombine.Average;
            mat.name = "HandColliderMaterial";
            foreach (int side in new int[] { 0, 1 })
            {
                hands[side].joints.Clear();
                for (int i = 0; i < HandTracker.JointCount; i++)
                {
                    var name = $"{hand_prefixes[side]}{Enum.GetNames(typeof(VaHandJointExt))[i]}";
                    var joint = HandObjects[side] == null ? null : HandObjects[side].FindChildRecursive(name)?.transform;
                    if (joint == null)
                    {
                        Debug.LogError($"Joint {name} not found in {HandObjects[side].name}");
                        continue;
                    }
                    if (joint.GetComponent<CapsuleCollider>() == null)
                    {
                        CapsuleCollider collider = joint.gameObject.AddComponent<CapsuleCollider>();
                        var rb = collider.gameObject.AddComponent<Rigidbody>();
                        rb.useGravity = false;
                        rb.isKinematic = true;
                        rb.excludeLayers = LayerMask.GetMask("Hand");
                        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                        collider.sharedMaterial = mat;
                        collider.height = 0.03f;
                        collider.center = Vector3.back * 0.01f;
                        collider.radius = 0.01f;
                        collider.direction = 2;
                        if ((VaHandJointExt)i == VaHandJointExt.Palm)
                        {
                            collider.radius = 0.02f;
                            collider.center = Vector3.down * 0.005f;
                            // collider.size = new Vector3(0.03f, 0.03f, 0.04f);
                        }
                        if ((VaHandJointExt)i == VaHandJointExt.Wrist)
                        {
                            collider.radius = 0.022f;
                            collider.height = 0.05f;
                            collider.center = Vector3.zero;
                            // collider.size = new Vector3(0.03f, 0.03f, 0.04f);
                        }
                        else if (((VaHandJointExt)i).ToString().Contains("Metacarpal"))
                        {
                            collider.height = 0.06f;
                            collider.radius = 0.015f;
                            //collider.size = new Vector3(0.025f, 0.015f, 0.03f);
                        }
                        else if (((VaHandJointExt)i).ToString().Contains("Proximal"))
                        {
                            collider.height = 0.06f;
                            collider.radius = 0.015f;
                            //collider.size = new Vector3(0.02f, 0.02f, 0.035f);
                        }
                    }
                    hands[side].joints.Add(new Joint { Name = joint.name, JointTransform = joint });
                }
            }
        }
    }

    float[] activeTime = { 0, 0 };
    public void UpdateHands(IReadOnlyList<JointLocations> jointLocations, float volumeScale)
    {
        // When using Unity's hand mesh to display hands in the 2D window, we need to negate the rotation of 180 degrees around the Y axis
        // This is because the orientation of the hand joints are provided in the Volume content space, which has a 180 degree rotation
        // to compensate for the GLTF model loading in the current design. This rotation might be changed in the future.
        var rotation180 = Quaternion.AngleAxis(-180, Vector3.up); // Negated rotation for correct orientation

        foreach (int side in new int[] { 0, 1 })
        {
            var joints = jointLocations[side];
            HandObjects[side].SetActive(joints.IsTracked);
            if (joints.IsTracked)
            {
                activeTime[side] += Time.deltaTime;
                HandObjects[side].transform.localScale = Vector3.one / volumeScale;
                for (int i = 0; i < HandTracker.JointCount; i++)
                {
                    var jointPose = joints.Pose(i);
                    var jointTransform = hands[side].joints[i].JointTransform;
                    var worldPos = HandsContainer.TransformPoint(jointPose.position.ToUnityPos());
                    if (activeTime[side] < .1f)
                    {
                        Debug.Log($"Setting position for {jointTransform.name} to {worldPos}");
                        jointTransform.GetComponent<Rigidbody>().position = (worldPos);
                        jointTransform.GetComponent<Rigidbody>().rotation = (jointPose.orientation.ToUnityRot());
                    }
                    else
                    {
                        jointTransform.GetComponent<Rigidbody>().MovePosition(worldPos);
                        jointTransform.GetComponent<Rigidbody>().MoveRotation(jointPose.orientation.ToUnityRot() * rotation180);
                    }
                }
            }
            else
            {
                activeTime[side] = 0;
            }

            if (handMeshes[side] != null)
            {
                handMeshes[side].SetActive(!DrawDebugJoints);
            }
        }

        if (DrawDebugJoints && debugJoints.Count == 0)
        {
            foreach (int side in new int[] { 0, 1 })
            {
                foreach (var joint in hands[side].joints)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.localScale = Vector3.one * 0.01f / volumeScale;
                    go.transform.SetParent(joint.JointTransform);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    debugJoints.Add(go);
                }
            }
        }
        else if (!DrawDebugJoints && debugJoints.Count > 0)
        {
            foreach (var go in debugJoints)
            {
                Destroy(go);
            }
            debugJoints.Clear();
        }
    }
}
