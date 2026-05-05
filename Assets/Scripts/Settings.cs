using System;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    
    [SerializeField] Slider VolumeSlider;
    [SerializeField] GameObject settingsPanel;

    public void volume()
    {
        AudioListener.volume = VolumeSlider.value;
    }

    public void ClosePanel()
    {
        PlayerPrefs.SetFloat("MasterVolume", VolumeSlider.value);
        Debug.Log(PlayerPrefs.GetFloat("MasterVolume"));
    }

    public void OpenPanel()
    {
      VolumeSlider.value =  PlayerPrefs.GetFloat("MasterVolume" );
      Debug.Log(PlayerPrefs.GetFloat("MasterVolume"));
    }

    public void Start()
    {
        
        settingsPanel.SetActive(false);
    }
}
