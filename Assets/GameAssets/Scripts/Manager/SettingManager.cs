using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            sound = true;
            music = true;
            vibration = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool sound = true;
    public bool music = true;
    public bool vibration = true;

    public SettingButtonUI soundButton;
    public SettingButtonUI musicButton;
    public SettingButtonUI vibrationButton;


    public void OnSoundButton()
    {
        sound = !sound;
        soundButton.SetButton(sound);
        AudioManager.Instance.SwitchSfxVolume(sound);
        AudioManager.Instance.PressButtonSfx();
    }

    public void OnMusicButton()
    {
        music = !music;
        musicButton.SetButton(music);
        AudioManager.Instance.SwitchMusicVolume(music);
        AudioManager.Instance.PressButtonSfx();
    }

    public void OnVibrationButton()
    {
        vibration = !vibration;
        vibrationButton.SetButton(vibration);
        AudioManager.Instance.PressButtonSfx();
    }

    public void UpdateButtonUI()
    {
        soundButton.GetSelectedImage().gameObject.SetActive(sound);
        musicButton.GetSelectedImage().gameObject.SetActive(music);
        vibrationButton.GetSelectedImage().gameObject.SetActive(vibration);
    }

    public void Vibration()
    {
        if (vibration)
        {
            Handheld.Vibrate();
        }
    }
}
