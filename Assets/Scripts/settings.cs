using UnityEngine;
using UnityEngine.UI;

public class settings : MonoBehaviour
{
    public static settings Instance { get; private set; }

    [Header("UI Elements")]
    public Slider volumeSlider;
    public Slider brightnessSlider;
    public Slider sensitivitySlider;
    public Image brightnessPanel; // Assign a black UI Image panel here

    // Public properties to access settings from other scripts
    public float MouseSensitivity { get; private set; }

    // PlayerPrefs keys
    private const string VolumeKey = "MasterVolume";
    private const string BrightnessKey = "Brightness";
    private const string SensitivityKey = "MouseSensitivity";

    // Default values
    private const float DefaultVolume = 0.8f;
    private const float DefaultBrightness = 1.0f;
    private const float DefaultSensitivity = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        LoadSettings();
        SetupListeners();
    }

    private void LoadSettings()
    {
        // Load values from PlayerPrefs or use defaults
        float volume = PlayerPrefs.GetFloat(VolumeKey, DefaultVolume);
        float brightness = PlayerPrefs.GetFloat(BrightnessKey, DefaultBrightness);
        MouseSensitivity = PlayerPrefs.GetFloat(SensitivityKey, DefaultSensitivity);

        // Update sliders if they are assigned
        if (volumeSlider) volumeSlider.value = volume;
        if (brightnessSlider) brightnessSlider.value = brightness;
        if (sensitivitySlider) sensitivitySlider.value = MouseSensitivity;

        // Apply the loaded settings
        ApplyVolume(volume);
        ApplyBrightness(brightness);
    }

    private void SetupListeners()
    {
        if (volumeSlider)
        {
            volumeSlider.onValueChanged.AddListener(ApplyVolume);
        }
        if (brightnessSlider)
        {
            brightnessSlider.onValueChanged.AddListener(ApplyBrightness);
        }
        if (sensitivitySlider)
        {
            sensitivitySlider.onValueChanged.AddListener(ApplySensitivity);
        }
    }

    public void ApplyVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
    }

    public void ApplyBrightness(float value)
    {
        if (brightnessPanel)
        {
            // Assuming 1.0 is full brightness (panel is transparent) and 0.0 is dark (panel is opaque)
            brightnessPanel.color = new Color(0, 0, 0, 1 - value);
        }
        PlayerPrefs.SetFloat(BrightnessKey, value);
    }

    public void ApplySensitivity(float value)
    {
        MouseSensitivity = value;
        PlayerPrefs.SetFloat(SensitivityKey, value);
    }
}