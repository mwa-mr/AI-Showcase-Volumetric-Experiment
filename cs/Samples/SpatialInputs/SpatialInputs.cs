using Microsoft.MixedReality.Volumetric;

namespace CsBoids
{
    public class SpatialInputs
    {
        private readonly Volume _volume;
        private readonly SpaceVisuals _axis;
        private HandTracker? _handTracker;
        private SpaceLocator? _locator;

        private sealed class SpaceVisuals
        {
            public ModelResource? model { get; set; }
            public VisualElement? volumeContainer { get; set; }
            public VisualElement? volumeContent { get; set; }
            public VisualElement? viewerSpace { get; set; }
            public VisualElement? localSpace { get; set; }
            public VisualElement? localFloorSpace { get; set; }
            public VisualElement?[,] joints { get; set; } = new VisualElement?[2, HandTracker.JointCount];
        }

        public SpatialInputs(Volume volume)
        {
            _volume = volume;
            _axis = new SpaceVisuals();
        }

        public void OnReady()
        {
            _handTracker = new HandTracker(_volume);
            _locator = new SpaceLocator(_volume);

            _axis.model = new ModelResource(_volume, VolumetricApp.GetAssetUri("axis_xyz_rub.glb"));
            _axis.viewerSpace = new VisualElement(_volume, _axis.model);
            _axis.localSpace = new VisualElement(_volume, _axis.model);
            _axis.localFloorSpace = new VisualElement(_volume, _axis.model);
            _axis.volumeContainer = new VisualElement(_volume, _axis.model);
            _axis.volumeContent = new VisualElement(_volume, _axis.model);

            foreach (int side in new int[] { 0, 1 })
            {
                for (uint i = 0; i < HandTracker.JointCount; i++)
                {
                    _axis.joints[side, i] = new VisualElement(_volume, _axis.model);
                }
            }

            OnUpdate();
        }

        public void OnUpdate()
        {
            float scale = _volume.Content.ActualScale;
            var sizeInMeter = (float size) => { return size / scale; };

            if (_locator?.IsReady == true)
            {
                _locator.Update();

                UpdateSpaceVisual(_axis.volumeContainer!, _locator.Locations.volumeContainer, 0.3f);
                UpdateSpaceVisual(_axis.volumeContent!, _locator.Locations.volumeContent, 0.2f);
                UpdateSpaceVisual(_axis.viewerSpace!, _locator.Locations.viewer, 0.1f);
                UpdateSpaceVisual(_axis.localSpace!, _locator.Locations.local, 0.15f);

                // Position the viewer space axis at a fixed position and only keep the rotation factor.
                _axis.viewerSpace!.SetPosition(new VaVector3f { x = 0.2f, y = 0.2f, z = 0.2f });
            }

            if (_handTracker?.IsReady == true)
            {
                _handTracker.Update();

                foreach (int side in new int[] { 0, 1 })
                {
                    var joints = _handTracker.JointLocations[side];
                    for (int i = 0; i < HandTracker.JointCount; i++)
                    {
                        UpdateSpaceVisual(_axis.joints[side, i]!, joints.IsTracked, joints.Pose(i), joints.Radius(i));
                    }
                }
            }
        }

        private void UpdateSpaceVisual(VisualElement visual, SpaceLocation location, float size)
        {
            UpdateSpaceVisual(visual, location.isTracked, location.pose, size);
        }

        private void UpdateSpaceVisual(VisualElement visual, bool isTracked, VaPosef location, float size, VaVector3f? offset = null)
        {
            if (isTracked)
            {
                visual.SetVisible(true);
                visual.SetPosition(location.position);
                visual.SetOrientation(location.orientation);
                visual.SetScale(size);
            }
            else
            {
                visual.SetVisible(false);
            }
        }
    }
}
