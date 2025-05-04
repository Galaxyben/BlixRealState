using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace _Main.Scripts
{
    public class ManagerUI : Singleton<ManagerUI>
    {
        [Header("Views")] 
        public GameObject viewNavbar;
        public GameObject viewLoadingScreen;
        public GameObject viewFilter;
        public GameObject viewAmenities;
        public Transform viewCardPlayground;

        [Header("Prefabs")]
        public GameObject departmentCard;

        [Header("References")] 
        public Transform[] cameraTargets;
        public DepartmentIdentifier[] departmentBoxes;

        private GameObject filtersPanel, amenitiesPanel;
        private Vector3 ogFilterPos, ogAmenitiesPos;
        private bool isAmenitiesUp = false, isFiltersUp = false;
        private CanvasGroup loadingScreenCG;
    
        private IEnumerator Start()
        {
            departmentBoxes = FindObjectsByType<DepartmentIdentifier>(FindObjectsSortMode.None);
            amenitiesPanel = viewAmenities.transform.GetChild(0).gameObject;
            filtersPanel = viewFilter.transform.GetChild(0).gameObject;

            loadingScreenCG = viewLoadingScreen.GetComponent<CanvasGroup>();

            ogFilterPos = filtersPanel.transform.position;
            ogAmenitiesPos = amenitiesPanel.transform.position;

            filtersPanel.transform.DOMoveY(-200f, 0.5f).SetEase(Ease.InOutSine);
            amenitiesPanel.transform.DOMoveY(-200f, 0.5f).SetEase(Ease.InOutSine);
            CleanCardPlayground();
            foreach (var box in departmentBoxes) box.gameObject.SetActive(false);

            yield return new WaitForSeconds(3f);
            
            loadingScreenCG.DOFade(0f, 1f).SetEase(Ease.InOutSine);
            loadingScreenCG.interactable = false;
            loadingScreenCG.blocksRaycasts = false;
        }

        public void ToggleAmenities()
        {
            if(!isAmenitiesUp)
                amenitiesPanel.transform.DOMoveY(ogAmenitiesPos.y, 1f).SetEase(Ease.InOutSine);
            else
                amenitiesPanel.transform.DOMoveY(-200f, 1f).SetEase(Ease.InOutSine);

            isAmenitiesUp = !isAmenitiesUp;
        }

        public void ToggleFilters()
        {
            if(!isFiltersUp)
            {
                filtersPanel.transform.DOMoveY(ogFilterPos.y, 0.5f).SetEase(Ease.InOutSine);
                foreach (var box in departmentBoxes) box.gameObject.SetActive(true);
            }
            else
            {
                filtersPanel.transform.DOMoveY(-200f, 0.5f).SetEase(Ease.InOutSine);
                foreach (var box in departmentBoxes) box.gameObject.SetActive(false);
            }

            isFiltersUp = !isFiltersUp;
        }

        public void SpawnDepartment()
        {
            Instantiate(departmentCard, viewCardPlayground);
        }

        private void CleanCardPlayground()
        {
            for (int i = 0; i < viewCardPlayground.childCount; i++) Destroy(viewCardPlayground.GetChild(i));
        }
    }
}
