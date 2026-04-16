using Microsoft.MixedReality.Volumetric;
using UnityEngine;

public static class UtilityExtensions
{
    public static UnityEngine.GameObject FindChildRecursive(this UnityEngine.GameObject root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (var i = 0; i < root.transform.childCount; i++)
        {
            UnityEngine.Transform child = root.transform.GetChild(i);
            var result = FindChildRecursive(child.gameObject, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    // The following helper functions convert coordinate system between Volumetric and Unity.
    // Volumetric uses a right-handed coordinate system with Y up, while Unity uses a left-handed coordinate system with Y up.
    // Because the volume content involves a 180 degree rotation around the Y axis to compensate for GLTF model loading.
    // In this way, the GLTF front face (+Z) is facing the camera, that is aligned with unity's +Z direction.
    // This rotation might be changed in the future.
    //
    // Therefore, the conversion involves negating the X coordinate while keeping the Z and Y coordinates the same.
    //  Position p : (X, Y, Z) -> (-X, Y, Z)
    //  Quaternion q : (X, Y, Z, W) -> (-X, Y, Z, -W)

    public static VaExtent3Df ToVolumetricSize(this UnityEngine.Vector3 value)
    {
        return new VaExtent3Df() { width = value.x, height = value.y, depth = value.z };
    }

    public static VaVector3f ToVolumetricScale(this UnityEngine.Vector3 value)
    {
        return new VaVector3f() { x = value.x, y = value.y, z = value.z };
    }

    public static VaVector3f ToVolumetricPos(this UnityEngine.Vector3 value)
    {
        return new VaVector3f() { x = -value.x, y = value.y, z = value.z };
    }

    public static VaVector2f ToVolumetric(this UnityEngine.Vector2 value)
    {
        return new VaVector2f() { x = value.x, y = value.y };
    }

    public static VaQuaternionf ToVolumetricRot(this UnityEngine.Quaternion value)
    {
        return new VaQuaternionf() { x = -value.x, y = value.y, z = value.z, w = -value.w };
    }

    public static UnityEngine.Vector3 ToUnityPos(this VaVector3f value)
    {
        return new UnityEngine.Vector3(-value.x, value.y, value.z);
    }

    public static UnityEngine.Vector2 ToUnity(this VaVector2f value)
    {
        return new UnityEngine.Vector2(value.x, value.y);
    }

    public static UnityEngine.Quaternion ToUnityRot(this VaQuaternionf value)
    {
        return new UnityEngine.Quaternion(-value.x, value.y, value.z, -value.w);
    }
}

