using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoutScript : MonoBehaviour
{
    public void Logout()
    {
        // Effacer la variable PlayerPref contenant l'ID de l'utilisateur
        PlayerPrefs.DeleteKey("UserID");

        // Revenir � la sc�ne "Login"
        SceneManager.LoadScene("Login");
    }
}