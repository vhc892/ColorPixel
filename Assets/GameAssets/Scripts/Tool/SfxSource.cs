
using UnityEngine;

public class SfxSource : MonoBehaviour
{
    private static SfxSource _i;

    public static SfxSource i
    {
        get
        {
            if (_i == null) _i = Instantiate(Resources.Load("SfxSource") as GameObject).GetComponent<SfxSource>();
            return _i;
        }
    }

    public AudioClip popupOpenSfx;
    public AudioClip openGiftSfx;
    public AudioClip fireworkExplodeSfx;
    public AudioClip fireworkLaunchSfx;
    public AudioClip paintPixelSfx;
    public AudioClip collectItemSfx;
    public AudioClip coinSfx;
    public AudioClip boomBoosterSfx;
    public AudioClip fillBoosterSfx;
    public AudioClip wheelSpinSfx;
    public AudioClip pressButtonSfx;
    public AudioClip winSfx;
    public AudioClip completeColorSfx;
}