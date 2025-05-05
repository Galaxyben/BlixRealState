using System.Collections;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;

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
        public CinemachineCamera cmCamera;
        public Material availableMat;
        public Material soldMat;
        public Material checkedMat;

        private GameObject filtersPanel, amenitiesPanel;
        private Vector3 ogFilterPos, ogAmenitiesPos;
        private bool isAmenitiesUp = false, isFiltersUp = false;
        private CanvasGroup loadingScreenCG;
        private MeshRenderer[] departmentRenderers;
    
        private IEnumerator Start()
        {
            departmentBoxes = FindObjectsByType<DepartmentIdentifier>(FindObjectsSortMode.None);
            departmentRenderers = departmentBoxes
                .Select(b => b.GetComponent<MeshRenderer>())
                .ToArray();
            
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
            {
                amenitiesPanel.transform.DOMoveY(ogAmenitiesPos.y, 1f).SetEase(Ease.InOutSine);
            }
            else
            {
                amenitiesPanel.transform.DOMoveY(-200f, 1f).SetEase(Ease.InOutSine);
                ChangeCameraTarget(0);
            }

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

        public void ChangeCameraTarget(int value)
        {
            cmCamera.Target.TrackingTarget = cameraTargets[value];
            cmCamera.GetComponent<CinemachineOrbitalFollow>().Radius = value == 0 ? 60f : 10f;
        }

        public void SpawnDepartment()
        {
            Instantiate(departmentCard, viewCardPlayground);
        }

        public void RandomDepartments()
        {
            foreach (var box in departmentBoxes)
            {
                box.gameObject.SetActive(Random.Range(0, 5) < 2);
            }
        }

        public void TurnOnSide(bool north)
        {
            foreach (var box in departmentBoxes)
            {
                if (north)
                    box.gameObject.SetActive(box.transform.parent.name == "NORTH");
                else
                    box.gameObject.SetActive(box.transform.parent.name == "SOUTH");
            }
        }

        public void TurnAvailability(int value)
        {
            for (int i = 0; i < departmentBoxes.Length; i++)
            {
                var box       = departmentBoxes[i];
                var rend  = departmentRenderers[i];
                var mat       = rend.sharedMaterial;    // use sharedMaterial for direct asset comparison
                bool isActive = value switch
                {
                    0 => mat == availableMat,
                    1 => mat == checkedMat,
                    2 => mat == soldMat,
                    _ => true
                };
                box.gameObject.SetActive(isActive);
            }
        }

        private void CleanCardPlayground()
        {
            for (int i = 0; i < viewCardPlayground.childCount; i++) Destroy(viewCardPlayground.GetChild(i));
        }
    }
}
