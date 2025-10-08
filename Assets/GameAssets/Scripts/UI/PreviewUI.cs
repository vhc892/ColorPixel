using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PreviewUI : MonoBehaviour
{
    [SerializeField] private GameObject tickButton;
    [SerializeField] private GameObject replayButton;
    [SerializeField] private GameObject decorObject;
    [SerializeField] private GameObject suggestObject;
    [SerializeField] private GameObject downloadButton;
    [SerializeField] private GameObject shareImageButton;

    [SerializeField] private DecorButtonUI backgroundButton;
    [SerializeField] private DecorButtonUI frameButton;
    [SerializeField] private DecorButtonUI stickerButton;

    void OnEnable()
    {
        tickButton.SetActive(true);
        decorObject.SetActive(true);
        replayButton.SetActive(false);
        downloadButton.SetActive(false);
        shareImageButton.SetActive(false);
        suggestObject.SetActive(false);

        CoreGameManager.Instance.SetHighlightIndex(-1, false);
        DecorManager.Instance.UpdateDecorType(Helper.DecorType.Background);
        backgroundButton.OnSelect();
        frameButton.Unselect();
        stickerButton.Unselect();
    }

    public void OnTickButton()
    {
        //Save BG, Frame, Sticker
        //Show Button
        tickButton.SetActive(false);
        decorObject.SetActive(false);
        replayButton.SetActive(true);
        downloadButton.SetActive(true);
        shareImageButton.SetActive(true);
        suggestObject.SetActive(true);
        DecorManager.Instance.UnselectAllStickers();
        DecorManager.Instance.SaveStickerCapture();
        DecorManager.Instance.CreatePreviewTexture();
        DatabaseManager.Instance.UpdateSuggestContainer();
        AudioManager.Instance.PressButtonSfx();
    }
    public async void OnDownloadButton()
    {
        AudioManager.Instance.PressButtonSfx();
        var tex = DecorManager.Instance.PreviewTempTex;
        if (tex == null)
        {
            return;
        }

        var perm = await NativeGallery.RequestPermissionAsync(
            NativeGallery.PermissionType.Write,
            NativeGallery.MediaType.Image
        );
        if (perm != NativeGallery.Permission.Granted)
        {
            Debug.LogWarning("Không có quyền lưu ảnh vào Gallery.");
            AndroidNativeToast.Show("Please grant permission to save the image!");
            return;
        }

        string filename = $"ArtBox_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        NativeGallery.SaveImageToGallery(tex, "ArtBox", filename, (success, path) =>
        {
            if (success)
            {
                Debug.Log($"Đã lưu: {path}");
                AndroidNativeToast.Show("Image saved to gallery!");
            }
            else
            {
                Debug.Log("Failed to save image.");
                AndroidNativeToast.Show("Failed to save image!", true);
            }
        });
    }
    public void OnShareButton()
    {
        AudioManager.Instance.PressButtonSfx();
        var tex = DecorManager.Instance.PreviewTempTex;
        if (tex == null)
        {
            Debug.LogWarning("Chưa có ảnh tạm để chia sẻ. Hãy nhấn Tick trước.");
            return;
        }

        string fileName = $"ArtBox_{DateTime.Now:yyyyMMdd_HHmmss}.png";

        new NativeShare()
            .SetSubject("ArtBox Share")
            .SetText("Image from ArtBox ✨")
            .SetTitle("Share from ArtBox")
            .AddFile(tex, fileName)
            .SetCallback((result, target) =>
            {
                Debug.Log($"Share result: {result} | target: {target}");
            })
            .Share();
    }
    public void OnBackgroundButton()
    {
        AudioManager.Instance.PressButtonSfx();
        DecorManager.Instance.UpdateDecorType(Helper.DecorType.Background);
        frameButton.Unselect();
        stickerButton.Unselect();
    }

    public void OnFrameButton()
    {
        AudioManager.Instance.PressButtonSfx();
        DecorManager.Instance.UpdateDecorType(Helper.DecorType.Frame);
        backgroundButton.Unselect();
        stickerButton.Unselect();
    }

    public void OnStickerButton()
    {
        AudioManager.Instance.PressButtonSfx();
        DecorManager.Instance.UpdateDecorType(Helper.DecorType.Sticker);
        frameButton.Unselect();
        backgroundButton.Unselect();
    }

    public GameObject GetDecorObject()
    {
        return decorObject;
    }
}
