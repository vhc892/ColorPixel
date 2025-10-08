using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxConfettiVfxUI : MonoBehaviour
{
    public ParticleSystem confettiVfx;

    // Hàm này sẽ hiện trong Animation Event (phải là public, void, không có tham số)
    public void PlayVFX()
    {
        if (confettiVfx != null)
            confettiVfx.Play();
    }
}
