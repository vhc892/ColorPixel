using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            PlayBGM();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public AudioMixer audioMixer;
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource sfxSource2;
    public AudioSource shortSoundSource;

    public void PopupOpenSfx()
    {
        PlaySFX(SfxSource.i.popupOpenSfx);
    }
    
    public void CompleteColorSfx()
    {
        PlaySFX(SfxSource.i.completeColorSfx);
    }

    public void PaintPixelSfx()
    {
        AudioClip cutClip = CutClip(SfxSource.i.paintPixelSfx, 0.2f);
        PlaySFX(cutClip);
    }

    public void PurchasedSfx()
    {
        PlaySFX(SfxSource.i.paintPixelSfx);
    }

    public void CollectItemSfx()
    {
        AudioClip cutClip = CutClip(SfxSource.i.collectItemSfx, 0.1f);
        PlaySFX2(cutClip);
    }

    public void FireWorkExplodeSfx()
    {
        PlaySFX(SfxSource.i.fireworkExplodeSfx);
    }

    public void FireWorkLaunchSfx()
    {
        //PlaySFX(SfxSource.i.fireworkLaunchSfx);
    }
    public void FillBoosterSfx()
    {
        PlayShortSound(SfxSource.i.fillBoosterSfx);
    }

    public void StopFillBoosterSfx()
    {
        // Tween trực tiếp giá trị volume
        DOTween.To(() => shortSoundSource.volume, 
                   x => shortSoundSource.volume = x, 
                   0f, 0.5f)
               .OnComplete(() =>
               {
                   shortSoundSource.Stop();
                   shortSoundSource.volume = 1f; // reset lại
               });
    }

    public void WinSfx()
    {
        PlayShortSound(SfxSource.i.winSfx);
        // Tween trực tiếp giá trị volume
        DOTween.To(() => shortSoundSource.volume,
                   x => shortSoundSource.volume = x,
                   1f, 1f)
                .From(0.5f)
                .OnComplete(() =>
                {
                    shortSoundSource.volume = 1f; // reset lại
                });
    }
    
    public void BoomBoosterSfx()
    {
        PlaySFX(SfxSource.i.boomBoosterSfx);
    }

    public void PressButtonSfx()
    {
        PlaySFX2(SfxSource.i.pressButtonSfx);
    }

    public void WheelSpinSfx()
    {
        PlaySFX(SfxSource.i.wheelSpinSfx);
    }

    public void CoinReceivedSfx()
    {
        AudioClip cutClip = CutClip(SfxSource.i.coinSfx, 0.1f);
        PlaySFX2(cutClip);
    }

    public void CoinScatterSfx()
    {
        //AudioClip cutClip = CutClip(SfxSource.i.waterDripSfx, 0.1f);
        //PlaySFX(cutClip);
    }

    private void PlayBGM(string name = "BGM_Main")
    {
        AudioClip bgmClip = Resources.Load<AudioClip>(name);
        if (bgmClip == null)
        {
            Debug.LogWarning("Không tìm thấy BGM trong Resources/BGM_Main");
            return;
        }

        bgmSource.clip = bgmClip;
        bgmSource.loop = true;      
        bgmSource.Play();
    }
    private void PlaySFX(AudioClip sfx)
    {
        if (sfx == null) return;
        if (!SettingManager.Instance.sound) return;
        sfxSource.PlayOneShot(sfx);
    }

    private void PlaySFX2(AudioClip sfx)
    {
        if (sfx == null) return;
        if (!SettingManager.Instance.sound) return;
        if (sfxSource2.isPlaying) sfxSource2.Stop();
        sfxSource2.PlayOneShot(sfx);
    }

    private void PlayShortSound(AudioClip shortSound)
    {
        if (shortSound == null) return;
        if (!SettingManager.Instance.sound) return;
        if (shortSoundSource.isPlaying) shortSoundSource.Stop();

        shortSoundSource.PlayOneShot(shortSound);
    }

    public void StopShortSound()
    {
        if (shortSoundSource.isPlaying) shortSoundSource.Stop();
    }

    public void SwitchMusicVolume(bool isOn) // volume = 0 -> 1
    {
        audioMixer.SetFloat("BGM", Mathf.Log10(Mathf.Max(isOn ? 1 : 0, 0.0001f)) * 20);
    }


    public void SwitchSfxVolume(bool isOn) // volume = 0 -> 1
    {
        audioMixer.SetFloat("SFX", Mathf.Log10(Mathf.Max(isOn ? 1 : 0, 0.0001f)) * 20);
    }

    AudioClip CutClip(AudioClip originalClip, float startTime)
    {
        int sampleRate = originalClip.frequency;
        int channels = originalClip.channels;
        int startSample = (int)(startTime * sampleRate * channels);
        int lengthSamples = originalClip.samples * channels - startSample;

        float[] data = new float[lengthSamples];
        originalClip.GetData(data, startSample / channels);

        AudioClip newClip = AudioClip.Create("SubClip", lengthSamples / channels, channels, sampleRate, false);
        newClip.SetData(data, 0);

        return newClip;
    }
}
