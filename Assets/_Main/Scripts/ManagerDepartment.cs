using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Main.Scripts
{
    public class ManagerDepartment : MonoBehaviour
    {
        public void GetBackToProject() => SceneManager.LoadScene(0);
        public void EnterDepartment() => SceneManager.LoadScene(1);
    }
}
