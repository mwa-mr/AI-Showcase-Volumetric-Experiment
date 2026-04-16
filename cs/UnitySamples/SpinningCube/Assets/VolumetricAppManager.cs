using GLTFast.Export;
using Microsoft.MixedReality.Volumetric;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class VolumetricAppManager : MonoBehaviour
{
    public Transform SceneRoot;
    private VolumetricApp _volumetricApp;
    private ModelResource _model;
    private VisualElement _visual;
    private Dictionary<string, VisualElement> _subElements;

    private void Start()
    {
        Application.targetFrameRate = 72;
        BuildExportNodeLists();
        _volumetricApp = new VolumetricApp(
            appName: Application.productName,
            requiredExtensions: new string[] {
                Extensions.VA_EXT_gltf2_model_resource,
            });
        _volumetricApp.OnStart += InitializeVolumes;
    }

    List<GameObject> rootNodes = new List<GameObject>();
    List<GameObject> allNodes = new List<GameObject>();
    private void BuildExportNodeLists()
    {
        for (int i = 0; i < SceneRoot.childCount; i++)
        {
            var node = SceneRoot.GetChild(i);
            node.name = node.name + "_" + node.GetInstanceID().ToString();
            rootNodes.Add(node.gameObject);
            allNodes.Add(node.gameObject);
            var children = node.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                if (child != node)
                {
                    child.name = child.name + "_" + child.GetInstanceID().ToString();
                    allNodes.Add(child.gameObject);
                }
            }
        }
    }

    private void OnDisable()
    {
        if (_volumetricApp != null)
        {
            _volumetricApp.RequestExit();
            _volumetricApp = null;
        }
    }

    private void Update()
    {
        if (_volumetricApp?.PollEvents() == false)
        {
            Application.Quit();
        }
    }

    private async void InitializeVolumes(VolumetricApp app)
    {
        string filePath = await ExportSceneAsync();

        Volume volume = new Volume(app);
        volume.OnReady = (var) =>
        {
            _model = new ModelResource(volume);
            _visual = new VisualElement(volume, _model);
            _subElements = new();

            foreach (var node in allNodes)
            {
                _subElements[node.name] = new VisualElement(volume, _visual, node.name);
            }

            string uri = new Uri(filePath).AbsoluteUri;
            _model.SetModelUri(uri);

            volume.RequestUpdate(VaVolumeUpdateMode.FullFramerate);
        };
        volume.OnUpdate = OnVolumeUpdate;
    }

    private void OnVolumeUpdate(Volume volume)
    {
        if (_subElements != null)
        {
            foreach (var node in allNodes)
            {
                var element = _subElements[node.name];
                if (element != null)
                {
                    // Note conversions of position and rotation from Unity to VolumetricApp (glTF) coordinate systems
                    var p = node.transform.localPosition;
                    var pv = new VaVector3f() { x = -p.x, y = p.y, z = p.z };
                    element.SetPosition(in pv);

                    var r = node.transform.localRotation;
                    var rv = new VaQuaternionf() { x = r.x, y = -r.y, z = -r.z, w = r.w };
                    element.SetOrientation(in rv);

                    var s = node.transform.localScale;
                    var sv = new VaVector3f() { x = s.x, y = s.y, z = s.z };
                    element.SetScale(in sv);
                }
            }
        }
    }

    async Task<string> ExportSceneAsync()
    {
        var exportSettings = new ExportSettings
        {
            Format = GltfFormat.Binary,
            FileConflictResolution = FileConflictResolution.Overwrite,
            ComponentMask = GLTFast.ComponentType.All,
        };

        var export = new GameObjectExport(exportSettings);
        export.AddScene(rootNodes.ToArray(), SceneRoot.name);

        var tempFilePath = Path.GetTempFileName();
        var tempFileName = Path.GetFileNameWithoutExtension(tempFilePath);
        var tempGlbFilePath = Path.Combine(Path.GetTempPath(), $"{tempFileName}.glb");

        // Async glTF export
        bool success = await export.SaveToFileAndDispose(tempGlbFilePath);
        Debug.Log($"Exported scene to {tempGlbFilePath}\nsuccess: {success}");

        if (!success)
        {
            Debug.LogError("Something went wrong exporting a glTF");
            return null;
        }

        return tempGlbFilePath;
    }
}
