using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuSimple : MonoBehaviour
{
    public GameObject settingsPanel;
    public Slider brightnessSlider;
    public Slider sensitivitySlider;
    public Slider volumeSlider;



    public void ChangeGlobalVolume(float value)
    {
        AudioListener.volume = value;
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
        PlayerPrefs.SetFloat("Brightness", brightnessSlider.value);
    }

    private void OnEnable()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        brightnessSlider.value = PlayerPrefs.GetFloat("Brightness");
    }

    private void Start()
    {
        volumeSlider.onValueChanged.AddListener(delegate { ChangeGlobalVolume(volumeSlider.value); });
        brightnessSlider.onValueChanged.AddListener(delegate { SetBrightness(brightnessSlider.value); });
    }

    public void SetBrightness(float value)
    {
        brightnessSlider.value = value;
    }
    public void SetSensitivity(float value)
    {
        sensitivitySlider.value = value;
    }
}