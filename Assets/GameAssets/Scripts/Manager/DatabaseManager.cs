using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int currentConceptIndex = 0;
    public ConceptDatabaseSO[] concepts;
    public ConceptDatabaseSO artBoxSODatabase;
    public List<int> myWork = new List<int>();
    public EventGame[] eventDatabase;

    public HorizontalCarouselLoop categoryBarContainer;

    void Start()
    {
        UpdateHomeScreenArtBox();
    }

    public ConceptDatabaseSO GetConceptDatabaseSOByIndex(int index)
    {
        int cur = (index % concepts.Length + concepts.Length) % concepts.Length;
        return concepts[cur];
    }

    public void UpdateArtBoxContainerByIndex(int index)
    {
        ConceptDatabaseSO curConceptDatabaseSO = GetConceptDatabaseSOByIndex(index);
        int curUI = (index % ArtBoxPool.Instance.artBoxContainer.Length
                    + ArtBoxPool.Instance.artBoxContainer.Length)
                    % ArtBoxPool.Instance.artBoxContainer.Length;
        // Debug.Log(curUI + " " + index);

        ArtBoxPool.Instance.ReturnAllArtBoxesFromContainer(curUI);
        foreach (ArtBoxSO artBoxSO in curConceptDatabaseSO.artBoxSOList)
        {
            ArtBox artBox = ArtBoxPool.Instance.GetArtBoxFromContainer(curUI);
            artBox.SetArtBox(artBoxSO);
        }
    }

    public void UpdateSuggestContainer()
    {
        if (CoreGameManager.Instance.GetCurrentArtBoxSO() == null) return;

        ArtBoxPool.Instance.ReturnAllArtBoxes();

        StartCoroutine(UpdateSuggestCoroutine());
    }

    private IEnumerator UpdateSuggestCoroutine()
    {
        foreach (ArtBoxSO artBoxSO in artBoxSODatabase.artBoxSOList)
        {
            if (!artBoxSO.isDone && !artBoxSO.name.Contains("EventArt_"))
            {
                ArtBox artBox = ArtBoxPool.Instance.GetArtBoxFromContainer(ArtBoxPool.Instance.suggestContainer);
                artBox.SetArtBox(artBoxSO);
            }

            // chia nhỏ, tránh lag frame
            if (Time.frameCount % 2 == 0)  
                yield return null;
        }
    }


    public void UpdateCategoryContainerByIndex(int index)
    {
        ConceptDatabaseSO curConceptDatabaseSO = GetConceptDatabaseSOByIndex(index);
        int categoryIndexUI = (index % categoryBarContainer.items.Length + categoryBarContainer.items.Length)
                                % categoryBarContainer.items.Length;

        CategoryBarUI categoryBarUI = categoryBarContainer.items[categoryIndexUI].GetComponent<CategoryBarUI>();
        categoryBarUI.TMP.SetText(curConceptDatabaseSO.GetName());
        categoryBarUI.shadowTMP.SetText(curConceptDatabaseSO.GetName());
        categoryBarUI.selectedTMP.SetText(curConceptDatabaseSO.GetName());
        categoryBarUI.selectedShadowTMP.SetText(curConceptDatabaseSO.GetName());
    }

    public void UpdateHomeScreenArtBox()
    {
        ArtBoxPool.Instance.ReturnAllArtBoxes();
        UpdateArtBoxContainerByIndex(currentConceptIndex);
        UpdateArtBoxContainerByIndex(currentConceptIndex + 1);
        UpdateArtBoxContainerByIndex(currentConceptIndex - 1);

        UpdateCategoryContainerByIndex(currentConceptIndex);
        UpdateCategoryContainerByIndex(currentConceptIndex + 1);
        UpdateCategoryContainerByIndex(currentConceptIndex - 1);
        UpdateCategoryContainerByIndex(currentConceptIndex + 2);
        UpdateCategoryContainerByIndex(currentConceptIndex - 2);
    }

    public void UpdateBothSideArtBoxContainer()
    {
        UpdateArtBoxContainerByIndex(currentConceptIndex - 1);
        UpdateArtBoxContainerByIndex(currentConceptIndex + 1);
    }

    public void UpdateBothSideCategoryContainer()
    {
        UpdateCategoryContainerByIndex(currentConceptIndex - 1);
        UpdateCategoryContainerByIndex(currentConceptIndex + 1);
        UpdateCategoryContainerByIndex(currentConceptIndex - 2);
        UpdateCategoryContainerByIndex(currentConceptIndex + 2);
    }

    public int GetArtBoxSoIndex(ArtBoxSO artBoxSO)
    {
        for (int i = 0; i < artBoxSODatabase.artBoxSOList.Count; i++)
        {
            if (artBoxSODatabase.artBoxSOList[i] == artBoxSO) return i;
        }
        return -1;
    }

    public void AddToMyWork(ArtBoxSO artBoxSO)
    {
        int i = GetArtBoxSoIndex(artBoxSO); 
        if (i >= 0 && !myWork.Contains(i)) myWork.Add(i);
    }

    public void UpdateMyWorkArtBoxs()
    {
        ArtBoxPool.Instance.ReturnAllArtBoxesFromContainer(ArtBoxPool.Instance.myWorkContainer);
        foreach (int index in myWork)
        {
            ArtBox artBox = ArtBoxPool.Instance.GetArtBoxFromContainer(ArtBoxPool.Instance.myWorkContainer);
            artBox.SetArtBox(artBoxSODatabase.artBoxSOList[index]);
        }
    }
}
