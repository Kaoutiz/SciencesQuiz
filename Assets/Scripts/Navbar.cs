using UnityEngine;
using UnityEngine.SceneManagement;

public class Navbar : MonoBehaviour
{
    // M�thode pour rediriger vers la sc�ne 1
    public void LoadScene1()
    {
        SceneManager.LoadScene("Home"); 
    }

    // M�thode pour rediriger vers la sc�ne 2
    public void LoadScene2()
    {
        SceneManager.LoadScene("Friend"); 
    }

    // M�thode pour rediriger vers la sc�ne 3
    public void LoadScene3()
    {
        SceneManager.LoadScene("Ranking"); 
    }

    // M�thode pour rediriger vers la sc�ne 4
    public void LoadScene4()
    {
        SceneManager.LoadScene("Shop"); 
    }
}
