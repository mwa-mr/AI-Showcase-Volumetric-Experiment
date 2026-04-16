using Microsoft.MixedReality.Volumetric;
using System;
using UnityEngine;

[ExecuteInEditMode]
public class VolumeCamera : MonoBehaviour
{

    public string DisplayName = "";

    [Flags]
    public enum FollowAxis : byte
    {
        X = 1, Y = 2, Z = 4
    }


    public Vector3 Offset = Vector3.zero;

    public Transform FollowTarget;
    public float FollowSpeed = 1;

    public FollowAxis FollowAxes;

    public VaVolumeSizeBehavior SizeBehavior = VaVolumeSizeBehavior.Fixed;
    public VaVolumeRotationLockFlags RotationLock = VaVolumeRotationLockFlags.None;

    public bool AllowInteractive = false;
    public bool AllowOneToOne = false;
    public bool AllowSharingInTeams = false;
    public bool AllowUnbounded = false;
    public bool AllowSubpartInteraction = false;

    public Vector3 RequestedSize = Vector3.one;
    public Vector3 RequestedPosition = Vector3.zero;
    public Quaternion RequestedRotation = Quaternion.identity;

    [HideInInspector]
    public Vector3 CurrentSize = Vector3.one;
    [HideInInspector]
    public Vector3 CurrentPosition = Vector3.zero;
    [HideInInspector]
    public Quaternion CurrentRotation = Quaternion.identity;

    private float _scaler = 1;

    private void Start()
    {
        CurrentSize = RequestedSize;
        CurrentPosition = RequestedPosition + Offset;
        if (DisplayName == "")
        {
            DisplayName = gameObject.name;
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(300, 10, 200, 200));
        GUILayout.Label("Volume Camera Debug");
        GUILayout.BeginVertical();
        SizeBehavior = GUILayout.Toggle(SizeBehavior == VaVolumeSizeBehavior.AutoSize, " Auto Size") ? VaVolumeSizeBehavior.AutoSize : VaVolumeSizeBehavior.Fixed;

        AllowInteractive = GUILayout.Toggle(AllowInteractive, " Interactive");
        AllowUnbounded = GUILayout.Toggle(AllowUnbounded, " Unbounded");
        AllowOneToOne = GUILayout.Toggle(AllowOneToOne, " One to One");
        AllowSharingInTeams = GUILayout.Toggle(AllowSharingInTeams, " Sharing in Teams");
        AllowSubpartInteraction = GUILayout.Toggle(AllowSubpartInteraction, " Subpart Interaction");

        GUILayout.EndVertical();
        if (SizeBehavior == VaVolumeSizeBehavior.Fixed)
        {
            GUILayout.BeginVertical();
            GUILayout.Label($"Scale: {_scaler:0.0} {RequestedSize * _scaler}");
            _scaler = GUILayout.HorizontalSlider(_scaler, .01f, 10f);
            GUILayout.EndVertical();
        }
        GUILayout.EndArea();
    }

    void OnDrawGizmos()
    {
        // Draw a semitransparent green cube
        Gizmos.color = new Color(0, 1, 0, 0.15f);
        Gizmos.DrawCube(CurrentPosition, CurrentSize);
    }

    // Update is called once per frame
    void Update()
    {
        if (FollowTarget != null)
        {
            RequestedPosition = new Vector3(
                 FollowAxes.HasFlag(FollowAxis.X) ? FollowTarget.position.x : RequestedPosition.x,
                 FollowAxes.HasFlag(FollowAxis.Y) ? FollowTarget.position.y : RequestedPosition.y,
                 FollowAxes.HasFlag(FollowAxis.Z) ? FollowTarget.position.z : RequestedPosition.z
            );

            RequestedRotation = FollowTarget.rotation;
        }

        RequestedSize = Vector3.Max(Vector3.one * float.Epsilon, RequestedSize);

        if (!Application.isPlaying)
        {
            CurrentSize = RequestedSize;
            CurrentPosition = RequestedPosition + Offset;
            return;
        }


        CurrentPosition = Vector3.Lerp(CurrentPosition, RequestedPosition + Offset, Time.deltaTime * FollowSpeed);
        if (CurrentPosition == Vector3.zero)
        {
            // HACK: avoid autoscale kicking in on HomeApp side by nudging the camera up a bit
            CurrentPosition.y += .000001f;
        }
        CurrentSize = Vector3.Lerp(CurrentSize, RequestedSize * _scaler, Time.deltaTime * FollowSpeed);
        CurrentRotation = Quaternion.Lerp(CurrentRotation, RequestedRotation, Time.deltaTime * FollowSpeed);
    }
}
