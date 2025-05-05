using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Main.Scripts
{
    public class ManagerDepartment : MonoBehaviour
    {
        public void GetBackToProject() => SceneManager.LoadScene(0);
    }
}
