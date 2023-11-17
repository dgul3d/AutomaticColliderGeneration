//https://bronsonzgeb.com/index.php/2021/11/27/better-collider-generation-with-asset-processors/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GenerateColliderPostProcessor : AssetPostprocessor
{
    [MenuItem("Tools/Better Collider Generation")]
    static void ToggleColliderGeneration()
    {
        var betterColliderGenerationEnabled = EditorPrefs.GetBool("BetterColliderGeneration", false);
        EditorPrefs.SetBool("BetterColliderGeneration", !betterColliderGenerationEnabled);
    }

    [MenuItem("Tools/Better Collider Generation", true)]
    static bool ValidateToggleColliderGeneration()
    {
        var betterColliderGenerationEnabled = EditorPrefs.GetBool("BetterColliderGeneration", false);
        Menu.SetChecked("Tools/Better Collider Generation", betterColliderGenerationEnabled);
        return true;
    }

    void OnPostprocessModel(GameObject g)
    {
        if (!EditorPrefs.GetBool("BetterColliderGeneration", false))
            return;

        List<Transform> transformsToDestroy = new List<Transform>();
        //Skip the root
        foreach (Transform child in g.transform)
        {
            GenerateCollider(child, transformsToDestroy);
        }

        for (int i = transformsToDestroy.Count - 1; i >= 0; --i)
        {
            if (transformsToDestroy[i] != null)
            {
                GameObject.DestroyImmediate(transformsToDestroy[i].gameObject);
            }
        }
    }

    bool DetectNamingConvention(Transform t, string convention)
    {
        bool result = false;
        if (t.gameObject.TryGetComponent(out MeshFilter meshFilter))
        {
            var lowercaseMeshName = meshFilter.sharedMesh.name.ToLower();
            result = lowercaseMeshName.StartsWith($"{convention}_");
        }

        if (!result)
        {
            var lowercaseName = t.name.ToLower();
            result = lowercaseName.StartsWith($"{convention}_");
        }

        return result;
    }

    void GenerateCollider(Transform t, List<Transform> transformsToDestroy)
    {
        foreach (Transform child in t.transform)
        {
            GenerateCollider(child, transformsToDestroy);
        }

        if (DetectNamingConvention(t, "ubx"))
        {
            if (!HasRotation(t))
            {
                AddCollider<BoxCollider>(t);
                transformsToDestroy.Add(t);
            }
            else
            {
                // Keep the object but remove MeshFilter and Renderer
                AddCollider<BoxCollider>(t);
            }
        }

        else if (DetectNamingConvention(t, "ucp"))
        {
            if (!HasRotation(t))
            {
                AddCollider<CapsuleCollider>(t);
                transformsToDestroy.Add(t);
            }
            else
            {
                // Keep the object but remove MeshFilter and Renderer
                AddCollider<CapsuleCollider>(t);
            }
        }
        else if (DetectNamingConvention(t, "usp"))
        {
            AddCollider<SphereCollider>(t);
            transformsToDestroy.Add(t);
        }
        else if (DetectNamingConvention(t, "ucx"))
        {
            TransformSharedMesh(t.GetComponent<MeshFilter>());
            var collider = AddCollider<MeshCollider>(t);
            collider.convex = true;
            transformsToDestroy.Add(t);
        }
        //warning - convex mesh colliders are deprecated in Unreal 4 and 5. This option will work with Unity only.
        else if (DetectNamingConvention(t, "umc"))
        {
            TransformSharedMesh(t.GetComponent<MeshFilter>());
            AddCollider<MeshCollider>(t);
            transformsToDestroy.Add(t);
        }
    }

    void TransformSharedMesh(MeshFilter meshFilter)
    {
        if (meshFilter == null)
            return;

        var transform = meshFilter.transform;
        var mesh = meshFilter.sharedMesh;
        var vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; ++i)
        {
            vertices[i] = transform.TransformPoint(vertices[i]);
            vertices[i] = transform.parent.InverseTransformPoint(vertices[i]);
        }

        mesh.SetVertices(vertices);
    }

    T AddCollider<T>(Transform t) where T : Collider
    {


        if (!HasRotation(t))
        {
            T collider = t.gameObject.AddComponent<T>();
            T parentCollider = t.parent.gameObject.AddComponent<T>();
            EditorUtility.CopySerialized(collider, parentCollider);

            SerializedObject parentColliderSo = new SerializedObject(parentCollider);
            var parentCenterProperty = parentColliderSo.FindProperty("m_Center");
            if (parentCenterProperty != null)
            {
                SerializedObject colliderSo = new SerializedObject(collider);
                var colliderCenter = colliderSo.FindProperty("m_Center");
                var worldSpaceColliderCenter = t.TransformPoint(colliderCenter.vector3Value);

                parentCenterProperty.vector3Value = t.parent.InverseTransformPoint(worldSpaceColliderCenter);
                parentColliderSo.ApplyModifiedPropertiesWithoutUndo();
            }

            return parentCollider;
        }
        else
        {
            T collider = t.gameObject.AddComponent<T>();
            GameObject.DestroyImmediate(t.GetComponent<MeshFilter>());
            GameObject.DestroyImmediate(t.GetComponent<Renderer>());
            return collider;
        }
    }

    //TODO: While it's possible to use box colliders with rotation of a multiple of 90 deg without rotated parent, script considers them necessary to have one.
    //Enabling this option requires rewriting transform copying logic in case of inconsistent collider scales, eg (2m,1m,1m) rotated box will transform into (1m,2m,1m)
    bool HasRotation(Transform t, float rotationTolerance = 0.01f)
    {
        Quaternion identity = Quaternion.identity;
        Quaternion targetRotation = t.rotation;

        // Check if the rotation is close enough to the identity rotation
        return Quaternion.Angle(targetRotation, identity) > rotationTolerance;
    }
}