using UnityEngine;
using UnityEngine.SceneManagement;

public class Navbar : MonoBehaviour
{
    // Méthode pour rediriger vers la scène 1
    public void LoadScene1()
    {
        SceneManager.LoadScene("Home"); 
    }

    // Méthode pour rediriger vers la scène 2
    public void LoadScene2()
    {
        SceneManager.LoadScene("Friend"); 
    }

    // Méthode pour rediriger vers la scène 3
    public void LoadScene3()
    {
        SceneManager.LoadScene("Ranking"); 
    }

    // Méthode pour rediriger vers la scène 4
    public void LoadScene4()
    {
        SceneManager.LoadScene("Shop"); 
    }
}
