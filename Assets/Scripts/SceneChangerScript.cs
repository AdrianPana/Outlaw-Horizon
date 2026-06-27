using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangerScript : MonoBehaviour
{
    public string sceneToLoad;
    private void OnTriggerEnter(Collider other)
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
