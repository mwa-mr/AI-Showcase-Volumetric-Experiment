using System;
using Microsoft.MixedReality.Volumetric;

namespace Volumetric.Samples.SpatialPad
{
    public class SpatialPadVolume : Volume
    {
        private readonly VolumetricExperience _volumetricExperience;
        private readonly KeypadData _keypadData;

        // Pad GLB model properties
        private ModelResource? _padModel;
        public VisualElement? _padVisual;
        private VisualElement? _padVisual_Light, _padVisual_Dark;
        private bool _lightMode = true;

        private SpaceLocator? _locator;
        private HandTracker? _handTracker;

        private DateTime _lastInteractionTime;
        private float _timeBetweenInteractions = 1f;

        // Manage volume events
        public SpatialPadVolume(VolumetricApp app, KeypadData keypadData, VolumetricExperience volumetricExperience) : base(app)
        {
            _volumetricExperience = volumetricExperience;
            _keypadData = keypadData;

            Container.AllowInteractiveMode(true);

            OnReady += onReady;
            OnUpdate += (_) => onUpdate();
            OnClose += (_) => onClose();
        }

        // Perform actions when volume is ready
        private void onReady(Volume volume)
        {
            // Set Volume display name
            Container.SetDisplayName(App.AppName);

            // Set Volume properties and behaviour
            Content.SetSizeBehavior(VaVolumeSizeBehavior.Fixed);
            VaExtent3Df _volumeSize = new VaExtent3Df();
            _volumeSize.width = _volumeSize.height = _volumeSize.depth = 8f;
            Content.SetSize(_volumeSize);

            // Load Pad GLB model
            _padModel = new ModelResource(volume, VolumetricApp.GetAssetUri("Assets/Models/Pad.glb"));
            // Create Visual Element with an associated model
            _padVisual = new VisualElement(volume, _padModel);
            // Set model position
            VaVector3f padPosition = new VaVector3f() { x = 0f, y = -0.75f, z = 0f };
            _padVisual.SetPosition(padPosition);
            // Separate model by nodes
            _padVisual_Light = new VisualElement(volume, _padVisual, "Light_Tray");
            _padVisual_Dark = new VisualElement(volume, _padVisual, "Dark_Tray");
            _padVisual_Dark.SetVisible(false);

            _padVisual_Dark.SetVisible(_keypadData.DarkMode);
            _padVisual_Light.SetVisible(!_keypadData.DarkMode);

            foreach (var slot in _keypadData.Slots)
            {
                _keypadData.Slots[slot.Index].VolumetricSlot = new VolumetricSlot(volume, slot);
            }

            // Set volume rotation lock
            Container.SetRotationLock(VaVolumeRotationLockFlags.X | VaVolumeRotationLockFlags.Z);

            _locator = new SpaceLocator(volume);
            _handTracker = new HandTracker(volume);

            // Configure volume update frequency
            RequestUpdate(VaVolumeUpdateMode.FullFramerate);
        }

        // Perform actions when Volume is updated
        private void onUpdate()
        {
            if (_locator?.IsReady == true)
            {
                _locator.Update();
            }

            if (_handTracker?.IsReady == true)
            {
                _handTracker.Update();
            }

            foreach (var slot in _keypadData.Slots)
            {

                if (slot.VolumetricSlot == null) continue;

                if (slot.VolumetricSlot.ModelHasChanged &&
                    slot.VolumetricSlot.Model?.IsReady == true &&
                    slot.VolumetricSlot.Visual?.IsReady == true &&
                    slot.VolumetricSlot.DisabledModel?.IsReady == true &&
                    slot.VolumetricSlot.DisabledVisual?.IsReady == true &&
                    (DateTime.Now - slot.VolumetricSlot.ModificationTime).TotalSeconds > 0.1f)
                {
                    // Perform actions just one time after the model is ready
                    if (!slot.VolumetricSlot.Initialized)
                    {
                        slot.VolumetricSlot.Initialized = true;
                        slot.VolumetricSlot.Visual.SetPosition(slot.VolumetricSlot.PositionInSpace);
                        var disabledPos = slot.VolumetricSlot.PositionInSpace;
                        disabledPos.y = -0.4f;
                        slot.VolumetricSlot.DisabledVisual.SetPosition(disabledPos);
                        if (slot.Index == _keypadData.Slots.Count - 1) _volumetricExperience.DesignPage.DeployButtonState("disabled");
                    }

                    slot.VolumetricSlot.Enable();
                    slot.VolumetricSlot.ModelHasChanged = false;
                }
                // Edit Mesh Buffers each loop
                slot.VolumetricSlot.PressButtonAnimation();
            }

            CheckInteractions();
            checkForegroundApp();
        }

        // Perform actions when Volume is closed
        private void onClose()
        {
            foreach (var slot in _keypadData.Slots)
            {
                slot.VolumetricSlot = null;
            }

            if (SpatialPad.App.GetCurrentKeypad().Index == _keypadData.Index)
            {
                // First transition to disabled.
                _volumetricExperience.DesignPage.DeployButtonState("disabled");

                // Now re-enable the button so the user can deploy a new volume again.
                _volumetricExperience.DesignPage.DeployButtonState("enabled");
            }

            _volumetricExperience.PadVolumes[_keypadData.Index] = null;
        }

        // Check if user interacts with the spatial pad
        public void CheckInteractions()
        {
            System.TimeSpan timeSinceLastInteraction = DateTime.Now - _lastInteractionTime;
            if (_handTracker?.IsReady == true && timeSinceLastInteraction.TotalSeconds > _timeBetweenInteractions)
            {
                // Check if each hand is being tracked and get index finger joint position
                VaVector3f outside = new VaVector3f() { x = -1f, y = -1f, z = -1f };
                var posIndexL = _handTracker.JointLocations[0].IsTracked ? _handTracker.JointLocations[0].Pose(10).position : outside;
                var posIndexR = _handTracker.JointLocations[1].IsTracked ? _handTracker.JointLocations[1].Pose(10).position : outside;

                if (posIndexL.Equals(outside) && posIndexR.Equals(outside)) return;

                // Check if index fingers are close to any slot
                foreach (var slot in _keypadData.Slots)
                {
                    if (!slot.HasShortcut
                    || slot.VolumetricSlot == null
                    || !slot.VolumetricSlot.Enabled
                    || slot.VolumetricSlot.AnimationIn
                    || slot.VolumetricSlot.AnimationOut)
                    {
                        continue;
                    }

                    var distanceL = System.Numerics.Vector3.Distance(new System.Numerics.Vector3(posIndexL.x, posIndexL.y, posIndexL.z), new System.Numerics.Vector3(slot.VolumetricSlot.PositionInSpace.x, slot.VolumetricSlot.PositionInSpace.y, slot.VolumetricSlot.PositionInSpace.z));
                    var distanceR = System.Numerics.Vector3.Distance(new System.Numerics.Vector3(posIndexR.x, posIndexR.y, posIndexR.z), new System.Numerics.Vector3(slot.VolumetricSlot.PositionInSpace.x, slot.VolumetricSlot.PositionInSpace.y, slot.VolumetricSlot.PositionInSpace.z));

                    if (distanceL != 0 && distanceL < 0.8 || distanceR != 0 && distanceR < 0.8)
                    {
                        // If index fingers are close to slot, trigger action
                        _lastInteractionTime = DateTime.Now;
                        slot.VolumetricSlot.OnButtonPressed();
                        break;
                    }
                }
            }
        }

        // Switch between light and dark pad (switch between model nodes)
        public void SwitchDarkLightPad()
        {
            _lightMode = !_lightMode;

            _padVisual_Light?.SetVisible(_lightMode);
            _padVisual_Dark?.SetVisible(!_lightMode);
        }

        // Check if the foreground app has changed
        private void checkForegroundApp()
        {
            string foregroundApp = ShortcutsManager.CheckForegroundApp();

            if (foregroundApp == ShortcutsManager.ForegroundApp) return;
            ShortcutsManager.ForegroundApp = foregroundApp;

            foreach (var slot in _keypadData.Slots)
            {
                // Enable or disable buttons when foreground app has changed
                slot.VolumetricSlot?.Enable();
            }
        }
    }
}
