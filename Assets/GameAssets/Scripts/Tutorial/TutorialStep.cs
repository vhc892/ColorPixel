using UnityEngine;
using DG.Tweening;

public abstract class TutorialStep : MonoBehaviour
{
    protected Sequence animationSequence;

    public void StartStep()
    {
        gameObject.SetActive(true);
        animationSequence?.Kill();
        CreateAnimation();
    }

    public virtual void EndStep()
    {
        animationSequence?.Kill();
        gameObject.SetActive(false);
    }

    protected abstract void CreateAnimation();
}