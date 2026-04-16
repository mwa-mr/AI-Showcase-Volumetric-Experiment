using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{
    const string SceneArgument = "--scene";

    [Serializable]
    public class SceneInfo
    {
        public string Label;
        public string Name;
    }
    public List<SceneInfo> Scenes = new List<SceneInfo>();
    public GameObject ButtonPrefab;
    public GameObject ButtonPanel;
    public TMPro.TextMeshProUGUI MessageText;
    public TMPro.TextMeshProUGUI FpsText;


    private string currentScene = "";
    private string startupScene = "";
    private string startupSceneError = "";

    void Awake()
    {
        foreach (var sceneInfo in Scenes)
        {
            var button = Instantiate(ButtonPrefab, ButtonPanel.transform);
            button.GetComponentInChildren<TMPro.TMP_Text>().text = sceneInfo.Label;
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                SwitchScene(sceneInfo.Name);
            });
        }

        startupScene = ResolveStartupSceneFromCommandLine();
        if (string.IsNullOrEmpty(startupSceneError))
        {
            SwitchScene(startupScene);
        }
    }

    private void Start()
    {
        SetMessage(string.IsNullOrEmpty(startupSceneError) ? "Not connected" : startupSceneError);
        SetEnabled(false);
        VolumetricAppManager.Instance.OnAppConnected += OnVolumetricAppConnected;
        VolumetricAppManager.Instance.OnAppDisconnected += OnVolumetricAppDisconnected;
        VolumetricAppManager.Instance.OnAppError += Instance_OnAppError;
    }

    private string ResolveStartupSceneFromCommandLine()
    {
        if (!TryGetCommandLineArgumentValue(SceneArgument, out string requestedScene))
        {
            return "";
        }

        if (string.IsNullOrWhiteSpace(requestedScene))
        {
            startupSceneError = $"Missing value for {SceneArgument}.";
            Debug.LogError($"SceneSwitcher: {startupSceneError}");
            return "";
        }

        foreach (var sceneInfo in Scenes)
        {
            if (string.Equals(sceneInfo.Name, requestedScene, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(sceneInfo.Label, requestedScene, StringComparison.OrdinalIgnoreCase))
            {
                return sceneInfo.Name;
            }
        }

        startupSceneError = $"Unknown startup scene '{requestedScene}'.";
        Debug.LogError($"SceneSwitcher: {startupSceneError}");
        return "";
    }

    private static bool TryGetCommandLineArgumentValue(string argumentName, out string value)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (string.Equals(arg, argumentName, StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length)
                {
                    value = args[i + 1];
                    return true;
                }

                value = string.Empty;
                return true;
            }

            string prefix = argumentName + "=";
            if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                value = arg.Substring(prefix.Length);
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    private void Instance_OnAppError(string message)
    {
        SetMessage($"Error: {message}");
    }

    private void OnVolumetricAppConnected(Microsoft.MixedReality.Volumetric.VolumetricApp obj)
    {
        SetMessage(string.IsNullOrEmpty(startupSceneError) ? "Connected" : startupSceneError);
        if (string.IsNullOrEmpty(startupSceneError))
        {
            SwitchScene(startupScene);
        }
        SetEnabled(true);
    }

    private void OnVolumetricAppDisconnected(Microsoft.MixedReality.Volumetric.VolumetricApp obj)
    {
        SetMessage("Disconnected");
        ClearScene();
        SetEnabled(false);
    }

    public void SetEnabled(bool enabled)
    {
        foreach (var button in ButtonPanel.GetComponentsInChildren<Button>())
        {
            button.interactable = enabled;
        }
    }

    public void SetMessage(string message)
    {
        MessageText.text = message;
    }

    public void ClearScene()
    {
        if (currentScene != "")
        {
            SceneManager.UnloadSceneAsync(currentScene);
            Resources.UnloadUnusedAssets();
        }
        currentScene = "";
    }

    public void SwitchScene(string name = "")
    {
        ClearScene();
        if (name == "")
        {
            name = Scenes[0].Name;
        }
        currentScene = name;
        SceneManager.LoadScene(name, LoadSceneMode.Additive);
    }

    float deltaTime = .016f;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchScene();
        }

        deltaTime += (Time.deltaTime - deltaTime) * 0.01f;
        float fps = 1.0f / deltaTime;
        FpsText.text = $"{Mathf.Ceil(fps).ToString()} fps";
    }
}
