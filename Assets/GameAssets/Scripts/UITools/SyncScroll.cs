using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncScroll : MonoBehaviour
{
    public HorizontalCarouselLoop interactCarousel;   // cái item full width
    public HorizontalCarouselLoop syncCarousel; // cái item nhỏ hơn

    private float ratio; // tỉ lệ giữa 2 chiều rộng
    private bool syncing; // tránh vòng lặp gọi nhau vô hạn
    private int lastDragged = -1;

    void Start()
    {
        float bigWidth = interactCarousel.items[0].rect.width;
        float smallWidth = syncCarousel.items[0].rect.width;

        ratio = smallWidth / bigWidth;
    }

    void Update()
    {
        if (syncing) return; // tránh vòng lặp

        // 🎯 Nếu đang kéo interactCarousel (big/small tùy bạn set)
        if (interactCarousel.isDragging)
        {
            lastDragged = 0;
            float delta = interactCarousel.DragDelta;
            if (Mathf.Abs(delta) > 0.01f)
            {
                syncing = true;
                syncCarousel.Scroll(delta * ratio);
                syncing = false;
            }
            interactCarousel.DragDelta = 0;
        }
        // 🎯 Nếu đang kéo syncCarousel
        else if (syncCarousel.isDragging)
        {
            lastDragged = 1;
            float delta = syncCarousel.DragDelta;
            if (Mathf.Abs(delta) > 0.01f)
            {
                syncing = true;
                interactCarousel.Scroll(delta / ratio);
                syncing = false;
            }
            syncCarousel.DragDelta = 0;
        }
        // 🎯 Khi thả ra → cả 2 snap theo cái cuối cùng được kéo
        else
        {
            if (interactCarousel.isDragging == false && syncCarousel.isDragging == false)
            {
                // đồng bộ 2 bên: ưu tiên cái nào vừa được kéo gần nhất
                syncing = true;
                if (lastDragged == 0) // 0 = interactCarousel
                {
                    syncCarousel.SnapTo(DatabaseManager.Instance.currentConceptIndex);
                }
                else if (lastDragged == 1) // 1 = syncCarousel
                {
                    interactCarousel.SnapTo(DatabaseManager.Instance.currentConceptIndex);
                }
                syncing = false;
            }
            lastDragged = -1;
        }
    }

}
