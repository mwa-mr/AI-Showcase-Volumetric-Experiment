
using Microsoft.MixedReality.Volumetric;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SceneExporter))]
public class VolumeManager : MonoBehaviour
{
    const int FPS = 72;
    // Temporary test hook: MrShell V2 needs primitive-local mesh-edit data for
    // shared-accessor assets, while the default sample path remains legacy-compatible.
    const string ForceDecoupleAccessorsArgument = "--force-decouple-accessors";

    public VolumeCamera VolumeCamera;
    public VolumetricHands VolumetricHands;
    public TMPro.TextMeshProUGUI MessageText;

    [SerializeField]
    UnityEvent _interactiveModeStart = new UnityEvent();
    [SerializeField]
    UnityEvent _interactiveModeStop = new UnityEvent();

    private class Elements
    {
        public ModelResource Model;
        public VisualElement Visual;
        public string ModelPath;
        public Dictionary<string, CachedNodeElement> SubNodes;
        public Dictionary<string, CachedMeshElement> MeshElements;
        public HandTracker HandTracker;
        public SpaceLocator Locator;
    }

    private Elements _elements;

    private SceneExporter _sceneExporter;
    private bool _forceDecoupleAccessors;

    private void Awake()
    {
        Debug.Log("VolumeManager.Awake()");
        Application.targetFrameRate = FPS;
        QualitySettings.vSyncCount = 0;
        _sceneExporter = GetComponent<SceneExporter>();
        _forceDecoupleAccessors = HasCommandLineArgument(ForceDecoupleAccessorsArgument);
        if (_forceDecoupleAccessors)
        {
            Debug.Log($"VolumeManager: enabling decoupled mesh-edit mode via {ForceDecoupleAccessorsArgument}");
        }
        SetMessage("", 0);
    }

    private static bool HasCommandLineArgument(string argument)
    {
        foreach (string currentArgument in Environment.GetCommandLineArgs())
        {
            if (string.Equals(currentArgument, argument, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    IEnumerator Start()
    {
        Debug.Log("VolumeManager.Start()");
        // Wait for the scene to be fully loaded
        yield return new WaitForSeconds(0.1f);
        while (VolumetricAppManager.Instance == null || VolumetricAppManager.Instance.VolumetricApp == null || !VolumetricAppManager.Instance.VolumetricApp.IsConnected)
        {
            Debug.LogWarning(VolumetricAppManager.Instance == null ? "No AppManager" : "AppManager Not Connected");
            yield return new WaitForSecondsRealtime(1);
        }
        InitializeVolume(VolumetricAppManager.Instance.VolumetricApp);
    }

    private UnityVolume Volume => VolumetricAppManager.Instance.Volume;

    private void OnDestroy()
    {
        if (_elements != null)
        {
            Debug.Log("VolumeManager.OnDestroy()");

            Volume?.RemoveAllElements();
        }
    }

    Coroutine _messageCoroutine;
    private void SetMessage(string message, float duration = 3.0f)
    {
        if (MessageText != null)
        {
            MessageText.text = message;
            if (duration > 0.0f)
            {
                if (_messageCoroutine != null)
                {
                    StopCoroutine(_messageCoroutine);
                }

                _messageCoroutine = StartCoroutine(WaitAndClearMessage(duration));
            }
        }
    }

    private IEnumerator WaitAndClearMessage(float duration)
    {
        yield return new WaitForSeconds(duration);
        MessageText.text = "";
        _messageCoroutine = null;
    }

    public async void InitializeVolume(VolumetricApp app)
    {
        if (!app.IsConnected)
        {
            Debug.Log("VolumeManager.InitializeVolumes() - App not connected");
            return;
        }

        Debug.Log($"VolumeManager: verify volume exists: {Volume}");

        // Create a new volume if one doesn't exist
        VolumetricAppManager.Instance.ConnectSceneToVolume(OnVolumeReady, OnVolumeUpdate, OnVolumeClose);

        if (_elements == null)
        {
            Volume.Container.SetDisplayName(SceneManager.GetActiveScene().name);
            Volume.Container.SetRotationLock(VolumeCamera != null ? VolumeCamera.RotationLock : VaVolumeRotationLockFlags.None);
            Volume.Content.SetSizeBehavior(VolumeCamera != null ? VolumeCamera.SizeBehavior : VaVolumeSizeBehavior.AutoSize);
            Volume.SetContentPosition(VolumeCamera != null ? VolumeCamera.CurrentPosition : Vector3.zero);
            Volume.SetContentSize(VolumeCamera != null ? VolumeCamera.CurrentSize : Vector3.one * 10.0f);
            Volume.SetContentRotation(VolumeCamera != null ? VolumeCamera.CurrentRotation : Quaternion.identity);
            Volume.Container.AllowInteractiveMode(VolumeCamera != null ? VolumeCamera.AllowInteractive : true);
            Volume.Container.AllowOneToOneMode(VolumeCamera != null ? VolumeCamera.AllowOneToOne : true);
            Volume.Container.AllowSharingInTeams(VolumeCamera != null ? VolumeCamera.AllowSharingInTeams : true);
            Volume.Container.AllowUnboundedMode(VolumeCamera != null ? VolumeCamera.AllowUnbounded : true);
            Volume.Container.AllowSubpartMode(VolumeCamera != null ? VolumeCamera.AllowSubpartInteraction : true);

            Volume.Container.onInteractiveModeChanged += (newValue) =>
            {
                SetMessage($"Volume.Container Mode changed. \nInteractive: {newValue}");
                if (newValue)
                {
                    _interactiveModeStart?.Invoke();
                }
                else
                {
                    _interactiveModeStop?.Invoke();
                }
            };
            Volume.Container.onOneToOneModeChanged += (newValue) => SetMessage($"Volume.Container Mode changed. \nOneToOne: {newValue}");
            Volume.Container.onSharingInTeamsChanged += (newValue) => SetMessage($"Volume.Container Mode changed. \nSharingInTeams: {newValue}");
            Volume.Container.onUnboundedModeChanged += (newValue) => SetMessage($"Volume.Container Mode changed. \nUnbounded: {newValue}");
            Volume.Container.onSubpartModeChanged += (newValue) => SetMessage($"Volume.Container Mode changed. \nSubpart: {newValue}");

            _elements = new Elements();
            _elements.HandTracker = new HandTracker(Volume);
            _elements.Locator = new SpaceLocator(Volume);
        }

        if (!_sceneExporter.EditorExport)
        {
            await _sceneExporter.ExportSceneAsync();
        }
    }

    private void OnVolumeReady()
    {
        Debug.Log("VolumeManager.OnVolumeReady() - Volume.OnReady()");
        Debug.Log("VolumeManager.InitializeVolumes() - Volume.OnReady()");
        Volume.RequestUpdate(VaVolumeUpdateMode.FullFramerate);
    }

    private void OnVolumeClose()
    {
        Debug.Log("VolumeManager.OnVolumeClose() - Volume.OnClose()");
        _elements = null;
    }

    private void SetupElements()
    {
        Debug.Log("VolumeManager.SetupElements()");

        if (_elements.Model != null)
        {
            _elements.SubNodes = null;
            _elements.MeshElements = null;
            _elements.Model = null;
        }

        _elements.Model = new ModelResource(Volume);
        _elements.Visual = new VisualElement(Volume, _elements.Model);
        if (_elements.SubNodes == null || _elements.SubNodes.Count == 0)
        {
            _elements.SubNodes = new Dictionary<string, CachedNodeElement>();
            _elements.MeshElements = new Dictionary<string, CachedMeshElement>();
            int rendererCount = 0;
            foreach (var nodeGameObject in _sceneExporter.ExportAllNodes)
            {
                if (nodeGameObject.GetComponentInChildren<Renderer>() == null)
                {
                    continue;
                }
                Debug.Log($"VolumeManager.SetupElements() - Adding node: {nodeGameObject.name}");
                var visualReference = new VisualElement(Volume, _elements.Visual, nodeGameObject.name);
                var nodeElement = new CachedNodeElement(nodeGameObject.transform, nodeGameObject.name, visualReference);
                _elements.SubNodes.Add(nodeGameObject.name, nodeElement);
                var renderer = nodeGameObject.GetComponent<Renderer>();

                if (renderer != null && renderer is SkinnedMeshRenderer && !renderer.isPartOfStaticBatch)
                {
                    Debug.Log($"VolumeManager.SetupElements() - Adding mesh: {nodeGameObject.name}");

                    var meshElement = new CachedMeshElement(_elements.Model, nodeGameObject.name, rendererCount, renderer, _forceDecoupleAccessors);
                    _elements.MeshElements.Add(nodeGameObject.name, meshElement);
                }

                if (renderer != null)
                {
                    rendererCount++;
                }
            }
        }
    }

    private void OnVolumeUpdate()
    {
        if (_elements == null || _sceneExporter == null || string.IsNullOrEmpty(_sceneExporter.GetExportPath()) || _sceneExporter.Exporting)
        {
            return;
        }

        var modelPath = new Uri(_sceneExporter.GetExportPath()).AbsoluteUri;
        if (_elements.ModelPath != modelPath)
        {
            Debug.Log($"VolumeManager.OnVolumeUpdate() - Loading model: {modelPath}");
            SetupElements();
            _elements.ModelPath = modelPath;
            _elements.Model.SetModelUri(_elements.ModelPath);
        }

        if (_elements.Model?.IsReady == true)
        {
            foreach (var nodeGameObject in _sceneExporter.ExportAllNodes)
            {
                if (_elements.SubNodes != null)
                {
                    var nodeElement = _elements.SubNodes.ContainsKey(nodeGameObject.name) ? _elements.SubNodes[nodeGameObject.name] : null;
                    nodeElement?.Update();
                }

                if (_elements.MeshElements != null)
                {
                    var meshElement = _elements.MeshElements.ContainsKey(nodeGameObject.name) ? _elements.MeshElements[nodeGameObject.name] : null;
                    meshElement?.Update();
                }
            }
        }

        if (_elements.Locator?.IsReady == true)
        {
            _elements.Locator.Update();
        }
        if (_elements.HandTracker?.IsReady == true)
        {
            _elements.HandTracker.Update();
            VolumetricHands.UpdateHands(_elements.HandTracker.JointLocations, Volume.Content.ActualScale);
        }

        if (VolumeCamera != null)
        {
            Volume.Container.SetDisplayName(VolumeCamera.DisplayName);
            Volume.Container.SetRotationLock(VolumeCamera.RotationLock);
            Volume.Content.SetSizeBehavior(VolumeCamera.SizeBehavior);
            Volume.SetContentPosition(-VolumeCamera.CurrentPosition);
            Volume.SetContentSize(VolumeCamera.CurrentSize);
            Volume.Container.AllowInteractiveMode(VolumeCamera.AllowInteractive);
            Volume.Container.AllowOneToOneMode(VolumeCamera.AllowOneToOne);
            Volume.Container.AllowSharingInTeams(VolumeCamera.AllowSharingInTeams);
            Volume.Container.AllowUnboundedMode(VolumeCamera.AllowUnbounded);
            Volume.Container.AllowSubpartMode(VolumeCamera.AllowSubpartInteraction);
        }
    }

    class CachedNodeElement
    {
        public VisualElement NodeElement { get; private set; }
        public Transform Transform { get; private set; }
        public string Name { get; private set; }
        private Vector3 _lastPosition;
        private Quaternion _lastOrientation;
        private Vector3 _lastScale;
        private bool _lastVisible;

        public CachedNodeElement(Transform transform, string name, VisualElement nodeElement)
        {
            NodeElement = nodeElement;
            Transform = transform;
            Name = name;
        }

        int _delayStart = 0;
        bool _isFirstUpdate = true;
        public bool Update()
        {
            if (!NodeElement.IsReady)
            {
                return false;
            }

            if (Transform == null)
            {
                return false;
            }

            // FIX: skip the first frames, otherwise the transform is not set correctly
            if (_delayStart++ < 25)
            {
                return false;
            }

            if (Transform.gameObject.activeSelf != _lastVisible || _isFirstUpdate)
            {
                //Debug.Log($"CachedNodeElement.Update() '{Name}' - activeSelf: {Transform.gameObject.activeSelf}");
                NodeElement.SetVisible(Transform.gameObject.activeSelf);
                _lastVisible = Transform.gameObject.activeSelf;
            }

            // Note conversions of position and rotation from Unity to VolumetricApp (glTF) coordinate systems
            if (Transform.localPosition != _lastPosition || _isFirstUpdate)
            {
                //Debug.Log($"CachedNodeElement.Update() '{Name}' - localPosition: {Transform.localPosition.x}, {Transform.localPosition.y}, {Transform.localPosition.z}");
                var pv = Transform.localPosition.ToVolumetricPos();
                NodeElement.SetPosition(in pv);
                _lastPosition = Transform.localPosition;
            }

            if (Quaternion.Dot(Transform.localRotation, _lastOrientation) < 0.9999999f)
            {
                //Debug.Log($"CachedNodeElement.Update() '{Name}' - localRotation: {Transform.localRotation.x}, {Transform.localRotation.y}, {Transform.localRotation.z}, {Transform.localRotation.w}");
                var rv = Transform.localRotation.ToVolumetricRot();
                NodeElement.SetOrientation(in rv);
                _lastOrientation = Transform.localRotation;
            }

            if (Transform.localScale != _lastScale || _isFirstUpdate)
            {
                //Debug.Log($"CachedNodeElement.Update() '{Name}' - localScale: {Transform.localScale.x}, {Transform.localScale.y}, {Transform.localScale.z}");
                var sv = Transform.localScale.ToVolumetricScale();
                NodeElement.SetScale(in sv);
                _lastScale = Transform.localScale;
            }

            _isFirstUpdate = false;

            return true;
        }
    }

    class CachedMeshElement
    {
        public List<MeshResource> SubMeshElements { get; private set; }
        public string Name;

        private readonly bool _forceDecoupleAccessors;
        private Mesh unityMesh = null;
        private Renderer renderer = null;
        private SkinnedMeshRenderer skinnedMeshRenderer = null;
        private List<List<UInt32>> indices = new List<List<UInt32>>();
        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<Vector4> tangents = new List<Vector4>();
        private Vector3 lastCenter;

        public CachedMeshElement(ModelResource model, string name, int index, Renderer renderer, bool forceDecoupleAccessors)
        {
            Name = name;
            SubMeshElements = new List<MeshResource>();
            _forceDecoupleAccessors = forceDecoupleAccessors;
            this.renderer = renderer;

            if (renderer is MeshRenderer)
            {
                unityMesh = renderer.gameObject.GetComponent<MeshFilter>().sharedMesh;
                lastCenter = unityMesh.bounds.center;
            }
            else if (renderer is SkinnedMeshRenderer)
            {
                skinnedMeshRenderer = renderer.gameObject.GetComponent<SkinnedMeshRenderer>();

                unityMesh = new Mesh();
                skinnedMeshRenderer.BakeMesh(unityMesh, true);
                unityMesh.RecalculateBounds();
                lastCenter = unityMesh.bounds.center;
            }
            else
            {
                Debug.LogWarning($"CachedMeshElement() - Unsupported renderer type: {renderer.GetType().Name}");
                return;
            }

            // The forced decoupled path rewrites primitive-local indices as well as positions,
            // so it needs an explicit writable index buffer. Keep the default path unchanged.
            var bufferDescriptors = _forceDecoupleAccessors
                ? new VaMeshBufferDescriptorExt[]
                {
                    new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.Index,  bufferFormat = VaMeshBufferFormatExt.Uint32 },
                    new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexPosition, bufferFormat = VaMeshBufferFormatExt.Float3 },
                    new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexNormal, bufferFormat = VaMeshBufferFormatExt.Float3 },
                    new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexTexcoord0, bufferFormat = VaMeshBufferFormatExt.Float2}
                }
                : new VaMeshBufferDescriptorExt[]
                {
                    new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexPosition, bufferFormat = VaMeshBufferFormatExt.Float3 },
                    new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexNormal, bufferFormat = VaMeshBufferFormatExt.Float3 },
                    new VaMeshBufferDescriptorExt { bufferType = VaMeshBufferTypeExt.VertexTexcoord0, bufferFormat = VaMeshBufferFormatExt.Float2}
                };

            for (var i = 0; i < unityMesh.subMeshCount; i++)
            {
                Debug.Log($"CachedMeshElement() - Adding submesh: {Name} - {i}");
                SubMeshElements.Add(new MeshResource(model, Name, (uint)i, bufferDescriptors, decoupleAccessors: _forceDecoupleAccessors, initializeData: false));
            }
        }

        private void AppendTransformedVertex(List<float> target, int sourceVertexIndex)
        {
            var sourceVertex = vertices[sourceVertexIndex];
            target.Add(sourceVertex.x / -renderer.transform.localScale.x);
            target.Add(sourceVertex.y / renderer.transform.localScale.y);
            target.Add(sourceVertex.z / renderer.transform.localScale.z);
        }

        // Rebuild a submesh into its own local vertex/index space so the written payload
        // matches the primitive-local buffers that MrShell V2 binds for this asset.
        private void BuildDecoupledSubmeshData(int subMeshIndex, out uint[] remappedIndices, out float[] primitiveLocalPositions)
        {
            int[] subMeshIndices = unityMesh.GetIndices(subMeshIndex);
            var indexRemap = new Dictionary<int, uint>(subMeshIndices.Length);
            var positions = new List<float>(subMeshIndices.Length * 3);
            remappedIndices = new uint[subMeshIndices.Length];

            for (int i = 0; i < subMeshIndices.Length; i++)
            {
                int sourceVertexIndex = subMeshIndices[i];
                if (!indexRemap.TryGetValue(sourceVertexIndex, out uint remappedIndex))
                {
                    remappedIndex = (uint)indexRemap.Count;
                    indexRemap.Add(sourceVertexIndex, remappedIndex);
                    AppendTransformedVertex(positions, sourceVertexIndex);
                }

                remappedIndices[i] = remappedIndex;
            }

            primitiveLocalPositions = positions.ToArray();
        }

        public bool Update()
        {
            for (var i = 0; i < unityMesh.subMeshCount; i++)
            {
                if (!SubMeshElements[i].IsReady)
                {
                    Debug.Log($"CachedMeshElement.Update() - MeshElement not ready: {Name} - {i}");
                    return false;
                }
            }

            bool isDirty = false;

            if (renderer is SkinnedMeshRenderer)
            {
                skinnedMeshRenderer.BakeMesh(unityMesh, false);
                unityMesh.RecalculateBounds();
            }

            // TODO: Check if the mesh has changed by checking vertex positions, normals, tangents, etc.
            // this is a simple check that only checks the bounds center which will shift slightly as mesh changes
            var diff = (unityMesh.bounds.center - lastCenter).magnitude;
            isDirty = diff > .00001f;
            if (isDirty)
            {
                //Debug.Log($"CachedMeshElement.Update() - Mesh bounds changed ({diff}): {Name} - {unityMesh.bounds.center.x},{unityMesh.bounds.center.y},{unityMesh.bounds.center.z}");
                lastCenter = unityMesh.bounds.center;

                if (indices.Count != unityMesh.subMeshCount)
                {
                    indices.Clear();
                    for (var i = 0; i < unityMesh.subMeshCount; i++)
                    {
                        indices.Add(new List<UInt32>((int)unityMesh.GetIndexCount(i)));
                    }

                    vertices = new List<Vector3>(unityMesh.vertices);
                    normals = new List<Vector3>(unityMesh.normals);
                    uvs = new List<Vector2>(unityMesh.uv);
                    tangents = new List<Vector4>(unityMesh.tangents);
                }

                //UInt32[] transformedIndices = new UInt32[mesh.triangles.Length];
                //for (int i = 0; i < mesh.triangles.Length; i += 3)
                //{
                //    transformedIndices[i] = (UInt32)mesh.triangles[i];
                //    transformedIndices[i + 1] = (UInt32)mesh.triangles[i + 2];
                //    transformedIndices[i + 2] = (UInt32)mesh.triangles[i + 1];
                //}
                unityMesh.GetVertices(vertices);
                if (_forceDecoupleAccessors)
                {
                    // In forced mode, do not broadcast the full baked vertex array to every
                    // primitive. Instead, resize each mesh resource to the primitive-local
                    // topology and write matching indices/positions for that primitive only.
                    for (var i = 0; i < unityMesh.subMeshCount; i++)
                    {
                        BuildDecoupledSubmeshData(i, out uint[] remappedIndices, out float[] primitiveLocalPositions);
                        uint targetIndexCount = checked((uint)remappedIndices.Length);
                        uint targetVertexCount = checked((uint)(primitiveLocalPositions.Length / 3));

                        SubMeshElements[i].WriteMeshBuffers(
                            new VaMeshBufferTypeExt[] { VaMeshBufferTypeExt.Index, VaMeshBufferTypeExt.VertexPosition },
                            targetIndexCount,
                            targetVertexCount,
                            (IReadOnlyList<MeshBufferData> meshBuffers) =>
                            {
                                Marshal.Copy(MemoryMarshal.Cast<uint, byte>(remappedIndices.AsSpan()).ToArray(), 0, meshBuffers[0].Buffer, remappedIndices.Length * sizeof(uint));
                                Marshal.Copy(MemoryMarshal.Cast<float, byte>(primitiveLocalPositions.AsSpan()).ToArray(), 0, meshBuffers[1].Buffer, primitiveLocalPositions.Length * sizeof(float));
                            });
                    }

                    return isDirty;
                }

                float[] transformedVertices = new float[vertices.Count * 3];
                int pos = 0;
                for (int i = 0; i < vertices.Count - 3; i += 3)
                {
                    var v0 = vertices[i];
                    var v1 = vertices[i + 1];
                    var v2 = vertices[i + 2];
                    // shouldn't have to divide by the scale, but bakemesh doesn't seem to work correctly
                    transformedVertices[pos] = v0.x / -renderer.transform.localScale.x;
                    transformedVertices[pos + 1] = v0.y / renderer.transform.localScale.y;
                    transformedVertices[pos + 2] = v0.z / renderer.transform.localScale.z;
                    pos += 3;

                    transformedVertices[pos] = v1.x / -renderer.transform.localScale.x;
                    transformedVertices[pos + 1] = v1.y / renderer.transform.localScale.y;
                    transformedVertices[pos + 2] = v1.z / renderer.transform.localScale.z;
                    pos += 3;

                    transformedVertices[pos] = v2.x / -renderer.transform.localScale.x;
                    transformedVertices[pos + 1] = v2.y / renderer.transform.localScale.y;
                    transformedVertices[pos + 2] = v2.z / renderer.transform.localScale.z;
                    pos += 3;
                }

                for (var i = 0; i < unityMesh.subMeshCount; i++)
                {
                    SubMeshElements[i].WriteMeshBuffers(new VaMeshBufferTypeExt[] { VaMeshBufferTypeExt.VertexPosition }, (IReadOnlyList<MeshBufferData> meshBuffers) =>
                    {
                        Marshal.Copy(MemoryMarshal.Cast<float, byte>(transformedVertices).ToArray(), 0, meshBuffers[0].Buffer, transformedVertices.Length * sizeof(float));
                    });
                }
            }
            return isDirty;
        }
    }
}
