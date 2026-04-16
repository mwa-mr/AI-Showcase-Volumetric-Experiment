
using Microsoft.MixedReality.Volumetric;
using System;
using UnityEngine;

public class VolumetricAppManager : MonoBehaviour
{
    public static VolumetricAppManager Instance;
    public event System.Action<VolumetricApp> OnAppConnected;
    public event System.Action<VolumetricApp> OnAppDisconnected;
    public event System.Action<string> OnAppError;

    private VolumetricApp _volumetricApp;
    public VolumetricApp VolumetricApp => _volumetricApp;

    private UnityVolume _volume;
    public UnityVolume Volume => _volume;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("VolumetricAppManager already exists.  Disabling this one.");
            gameObject.SetActive(false);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Debug.Log("VolumetricAppManager.Awake()");
        Instance = this;
    }

    void Start()
    {
        Debug.Log("VolumetricAppManager.Start()");

        try
        {
            _volumetricApp = new VolumetricApp(
                appName: Application.productName,
                requiredExtensions: new string[]
                {
                    Extensions.VA_EXT_gltf2_model_resource,
                    Extensions.VA_EXT_mesh_edit,
                    Extensions.VA_EXT_locate_joints,
                    Extensions.VA_EXT_locate_spaces,
                    Extensions.VA_EXT_volume_container_modes
                });
            VolumetricApp.OnStart += AppConnected;
            VolumetricApp.OnReconnect += AppConnected;
            VolumetricApp.OnDisconnect += AppDisconnected;
            VolumetricApp.OnStop += (app) => Debug.Log("VolumetricAppManager.OnStop()");
        }
        catch (System.Exception e)
        {
            Debug.LogError("VolumetricAppManager.Start() Exception: " + e.Message);
            OnAppError?.Invoke(e.Message);
        }
    }

    private void AppConnected(VolumetricApp app)
    {
        Debug.Log("VolumetricAppManager.AppConnected()");
        OnAppConnected?.Invoke(app);
    }

    private void AppDisconnected(VolumetricApp app)
    {
        Debug.Log("VolumetricAppManager.AppDisconnected()");
        OnAppDisconnected?.Invoke(app);

        // The library will manage volume close and life cycle
        // Here we just need to disconnect it from this class.
        _volume = null;
    }

    private void OnApplicationQuit()
    {
        Debug.Log("VolumetricAppManager.OnApplicationQuit()");
        if (VolumetricApp != null)
        {
            foreach (var volume in VolumetricApp.Volumes)
            {
                volume.RequestClose();
            }
            VolumetricApp.RequestExit();
        }
    }

    private void Update()
    {
        if (VolumetricApp?.PollEvents() == false)
        {
            Application.Quit();
        }
    }

    internal void ConnectSceneToVolume(Action onVolumeReady, Action onVolumeUpdate, Action onVolumeClose)
    {
        if (_volume == null && _volumetricApp.IsConnected)
        {
            _volume = new UnityVolume(_volumetricApp);
        }

        _volume.OnReady = _ => onVolumeReady();
        _volume.OnUpdate = _ => onVolumeUpdate();
        _volume.OnClose = _ => onVolumeClose();
    }
}
