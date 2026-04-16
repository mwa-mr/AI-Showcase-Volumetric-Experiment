using GLTFast.Export;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneExporter : MonoBehaviour
{
    public bool EditorExport = false;
    public Transform SceneRoot;
    public List<GameObject> ExportAllNodes { get; private set; } = new List<GameObject>();

    private string _exportPath;
    private List<GameObject> _exportRootNodes = new List<GameObject>();

    public bool Exporting { get; private set; }
    void Start()
    {
        Debug.Log("SceneExporter.Start()");
        if (Application.isPlaying && EditorExport)
        {
            BuildExportNodeLists();
        }
    }

    public void ClearPath()
    {
        _exportPath = string.Empty;
    }

    private void BuildExportNodeLists()
    {
        Debug.Log("SceneExporter.BuildExportNodeLists()");
        _exportRootNodes.Clear();
        ExportAllNodes.Clear();

        for (int i = 0; i < SceneRoot.childCount; i++)
        {
            var node = SceneRoot.GetChild(i);

            if (!node.name.Contains("__"))
            {
                node.name = node.name + "__" + i;
            }

            _exportRootNodes.Add(node.gameObject);
            ExportAllNodes.Add(node.gameObject);
            var children = node.GetComponentsInChildren<Transform>(true);
            int c = 0;
            foreach (var child in children)
            {
                if (child != node && node.GetComponentsInChildren<Renderer>(true).Length > 0)
                {
                    if (!child.name.Contains("__"))
                    {
                        child.name = child.name + "__" + i + "__" + c;
                    }

                    ExportAllNodes.Add(child.gameObject);
                    c++;
                }
            }
        }
        Debug.Log($"SceneExporter.BuildExportNodeLists: {SceneRoot.childCount} root nodes and {ExportAllNodes.Count} total nodes");
    }

    private void Cleanup()
    {
        Debug.Log("SceneExporter.Cleanup()");
        foreach (var node in ExportAllNodes)
        {
            if (node.name.Contains("__"))
            {
                node.name = node.name.Split(new string[] { "__" }, System.StringSplitOptions.None)[0];
            }
        }
    }

    public string GetExportPath()
    {
        if (EditorExport)
        {

            return Path.Combine(Application.streamingAssetsPath, $"{gameObject.scene.name}_{SceneRoot.name}.glb");
        }
        return _exportPath;
    }

    public async Task<bool> ExportSceneAsync()
    {
        Exporting = true;
        Debug.Log($"Exporting scene to '{_exportPath}'");

        if (EditorExport)
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            _exportPath = Path.Combine(Application.streamingAssetsPath, $"{gameObject.scene.name}_{SceneRoot.name}.glb");
        }
        else if (string.IsNullOrEmpty(_exportPath))
        {
            // Create a temporary file to export to
            var tempFilePath = Path.GetTempFileName();
            var tempFileName = Path.GetFileNameWithoutExtension(tempFilePath);
            _exportPath = Path.Combine(Path.GetTempPath(), $"{tempFileName}.glb");
        }

        Debug.Log($"Exporting scene to to '{_exportPath}'");
        BuildExportNodeLists();
        var exportSettings = new ExportSettings
        {
            Format = GltfFormat.Binary,
            FileConflictResolution = FileConflictResolution.Overwrite,
            ComponentMask = GLTFast.ComponentType.All,
        };

        bool[] enabled = _exportRootNodes.Select(b => b.activeSelf).ToArray();
        foreach (var node in _exportRootNodes)
        {
            node.SetActive(true);
        }

        var export = new GameObjectExport(exportSettings);
        export.AddScene(_exportRootNodes.ToArray(), SceneRoot.name);

        try
        {
            // Async glTF export
            bool success = await export.SaveToFileAndDispose(_exportPath);
            Debug.Log($"Exported scene to {_exportPath}\nsuccess: {success}");

            if (!success)
            {
                Debug.LogError("Something went wrong exporting a glTF");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error exporting scene: {e.Message}");
            return false;
        }
        finally
        {

            foreach (var node in _exportRootNodes)
            {
                if (node != null)
                {
                    node.SetActive(enabled[_exportRootNodes.IndexOf(node)]);
                }
            }

            if (EditorExport)
            {
                Cleanup();
            }

            Exporting = false;
        }

        return true;
    }


#if UNITY_EDITOR

    [CustomEditor(typeof(SceneExporter))]
    public class SceneExporterEditor : Editor
    {
        public async override void OnInspectorGUI()
        {

            DrawDefaultInspector();
            SceneExporter exporter = (SceneExporter)target;
            if (exporter.EditorExport)
            {
                if (EditorGUILayout.LinkButton("Export Scene"))
                {
                    await exporter.ExportSceneAsync();
                }
            }
            else
            {
                if (!Application.isPlaying)
                {
                    exporter.ClearPath();
                }
            }
        }
    }

#endif
}

