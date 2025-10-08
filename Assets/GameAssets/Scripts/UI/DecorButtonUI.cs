using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorButtonUI : MonoBehaviour
{
    [SerializeField] private Transform selectedButton;
    [SerializeField] private Transform buttonText;

    public void OnSelect()
    {
        selectedButton.position = transform.position;
        buttonText.position = new Vector3(transform.position.x, transform.position.y - 6, transform.position.z);
        buttonText.localScale = Vector3.one * 1.1f;
    }

    public void Unselect()
    {
        buttonText.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        buttonText.localScale = Vector3.one;
    }
}
