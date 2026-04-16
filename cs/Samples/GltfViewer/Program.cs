using Microsoft.MixedReality.Volumetric;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

internal sealed class Program
{
    sealed class ModelViewer : Volume, Sample.IGltfViewer
    {
        public Action RefreshUIText { get; set; }

        private Uri _currentModelUri;
        public string DisplayName =>
            (_currentModelUri != null && _currentModelUri.IsFile) ?
                System.IO.Path.GetFileName(_currentModelUri.ToString()) : "No model loaded";

        public ModelViewer(VolumetricApp app) : base(app)
        {
            OnReady += _ => OnVolumeReady();
            OnClose += _ => OnVolumeClose();
        }

        private void OnVolumeReady()
        {
            ResetAllProperties();

            Uri modelUri = null;
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                if (System.IO.File.Exists(args[1]))
                {
                    modelUri = new Uri(args[1]);
                }
            }

            // If no model specified, load a default one
            if (modelUri == null)
            {
                modelUri = new Uri(VolumetricApp.GetAssetUri("axis_xyz_rub.glb"));
            }

            SetGltfFile(modelUri);
        }

        private void OnVolumeClose()
        {
            App.RequestExit();  // Exit the volumetric application
            Application.Exit(); // Exit the winform application
        }

        public void SetGltfFile(Uri uri)
        {
            _currentModelUri = uri;
            Container.SetDisplayName(DisplayName);
            if (_model is null)
            {
                _model = new ModelResource(this, uri.AbsoluteUri);
                _model.OnAsyncStateChanged += OnAsyncStateChanged;
                _visual = new VisualElement(this, _model);
            }
            else
            {
                _model.SetModelUri(uri.AbsoluteUri);
            }
            this.RequestUpdate();   // on demand update
        }

        public string HelpString()
        {
            return $"{_loadingStatus} {DisplayName}\n\n" +
                $"AD: Move Left/Right X = {Content.ActualPosition.x:F2}\n" +
                $"EQ: Move Up/Down Y = {Content.ActualPosition.y:F2}\n" +
                $"WS: Move Forward/Backward  Z={Content.ActualPosition.z:F2}\n" +
                "IK: Pitch Up/Down\n" +
                "JL: Yaw Left/Right\n" +
                "UO: Roll Left/Right\n" +
                $"+/-: Size Up/Down ({PrintSize(Content.ActualSize)})\n" +
                $"0: Auto size (Behavior)=({PrintScale(Content.ActualScale, Content.SizeBehavior)})\n" +
                $"123: Toggle Rotation Lock (XYZ)=({PrintRotationLockState()})\n" +
                "Esc: Reset to Default";
        }

        private string PrintSize(VaExtent3Df actualSize)
        {
            return $"w={actualSize.width:F2},h={actualSize.height:F2},d={actualSize.depth:F2}";
        }

        private string PrintScale(float size, VaVolumeSizeBehavior behavior)
        {
            return behavior == VaVolumeSizeBehavior.AutoSize ? "Auto" : $"Fixed: s={size:F2}";
        }

        private string PrintRotationLockState()
        {
            var x = _rotationLock.HasFlag(VaVolumeRotationLockFlags.X) ? "1" : "0";
            var y = _rotationLock.HasFlag(VaVolumeRotationLockFlags.Y) ? "1" : "0";
            var z = _rotationLock.HasFlag(VaVolumeRotationLockFlags.Z) ? "1" : "0";
            return $"{x}|{y}|{z}";
        }

        private void OnAsyncStateChanged(VaElementAsyncState newState)
        {
            switch (newState)
            {
                case VaElementAsyncState.Pending:
                    _loadingStatus = "Loading...";
                    break;
                case VaElementAsyncState.Ready:
                    _loadingStatus = "Ready: ";
                    break;
                case VaElementAsyncState.Error:
                    _loadingStatus = "Error: ";
                    break;
                default:
                    break;
            }
            RefreshUIText?.Invoke();
        }

        public bool HandleKeyDown(Keys keyData)
        {
            if (_keyActions == null)
            {
                const float delta = 0.02f;
                _keyActions = new Dictionary<Keys, Action>
                {
                    { Keys.W, () => MoveVolume(0, 0, -delta) },     // forward
                    { Keys.S, () => MoveVolume(0, 0, +delta) },     // backward
                    { Keys.A, () => MoveVolume(-delta, 0, 0) },     // left
                    { Keys.D, () => MoveVolume(+delta, 0, 0) },     // right
                    { Keys.Q, () => MoveVolume(0, -delta, 0) },     // down
                    { Keys.E, () => MoveVolume(0, +delta, 0) },     // up
                    { Keys.I, () => RotateVolume(+delta, 0, 0) },   // pitch up
                    { Keys.K, () => RotateVolume(-delta, 0, 0) },   // pitch down
                    { Keys.J, () => RotateVolume(0, +delta, 0) },   // yaw left
                    { Keys.L, () => RotateVolume(0, -delta, 0) },   // yaw right
                    { Keys.U, () => RotateVolume(0, 0, +delta) },   // roll left
                    { Keys.O, () => RotateVolume(0, 0, -delta) },   // roll right
                    { Keys.Oemplus, () => ChangeSize(+delta) },     // size up
                    { Keys.OemMinus, () => ChangeSize(-delta) },    // size down
                    { Keys.D0, () => AutoSize() },                  // reset to auto size
                    { Keys.D1, () => ToggleRotationLock(VaVolumeRotationLockFlags.X) },  // toggle rotation lock X
                    { Keys.D2, () => ToggleRotationLock(VaVolumeRotationLockFlags.Y) },  // toggle rotation lock Y
                    { Keys.D3, () => ToggleRotationLock(VaVolumeRotationLockFlags.Z) },  // toggle rotation lock Z
                    { Keys.Escape, () => ResetAllProperties() },  // reset
                };
            }

            if (_keyActions.TryGetValue(keyData, out var action))
            {
                action();
                this.RequestUpdate();   // on demand update
                return true;
            }
            return false;
        }

        private void ResetAllProperties()
        {
            Content.SetSizeBehavior(VaVolumeSizeBehavior.AutoSize);
            Content.SetPosition(VaMath.Zero);
            Content.SetSize(VaMath.OneSize);
            Content.SetOrientation(VaMath.Identity);

            Container.SetRotationLock(VaVolumeRotationLockFlags.None);
        }

        private void MoveVolume(float dx, float dy, float dz)
        {
            var position = Content.ActualPosition;
            position.x += dx * Content.ActualSize.width;
            position.y += dy * Content.ActualSize.height;
            position.z += dz * Content.ActualSize.depth;
            Content.SetPosition(position);
        }

        private void RotateVolume(float dx, float dy, float dz)
        {
            var q = VaMath.EulerToQuaternion(dx, dy, dz);
            _orientation = VaMath.Multiply(_orientation, q);
            Content.SetOrientation(_orientation);
        }

        private void ChangeSize(float delta)
        {
            var size = Content.ActualSize;
            size.width *= (1 + delta);
            size.height *= (1 + delta);
            size.depth *= (1 + delta);
            Content.SetSizeBehavior(VaVolumeSizeBehavior.Fixed);
            Content.SetSize(size);
        }

        private void AutoSize()
        {
            Content.SetSizeBehavior(VaVolumeSizeBehavior.AutoSize);
        }

        private void ToggleRotationLock(VaVolumeRotationLockFlags flag)
        {
            if (_rotationLock.HasFlag(flag))
            {
                _rotationLock &= ~flag;
            }
            else
            {
                _rotationLock |= flag;
            }
            Container.SetRotationLock(_rotationLock);
        }

        private VisualElement _visual;
        private ModelResource _model;
        private Dictionary<Keys, Action> _keyActions;

        private string _loadingStatus;
        private VaQuaternionf _orientation = VaMath.Identity;
        private VaVolumeRotationLockFlags _rotationLock;
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.SetCompatibleTextRenderingDefault(false);

        var form = new Sample.MainForm();

        var app = new VolumetricApp("cs_gltf_viewer",
            requiredExtensions: new string[] {
                Extensions.VA_EXT_gltf2_model_resource,
                Extensions.VA_EXT_volume_content_container,
            });
        app.OnStart += _ =>
        {
            var volume = new ModelViewer(app);
            form.Viewer = volume;
        };
        app.RunAsync();

        Application.Run(form);
    }
}
