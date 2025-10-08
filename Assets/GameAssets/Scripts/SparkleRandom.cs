using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SparkleRandom : MonoBehaviour
{

    public void ActivateSparkle()
    {
        Image image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError("SparkleRandomizer needs an Image component on the same GameObject.", this);
            return;
        }

        Material uniqueMaterial = new Material(image.material);
        image.material = uniqueMaterial;

        float randomOffset = Random.Range(0f, 100f);
        uniqueMaterial.SetFloat("_RandomOffset", randomOffset);
        uniqueMaterial.SetFloat("_SparkleOn", 1.0f);

        Debug.Log("Activated Sparkle for: " + transform.parent.name);
    }
}