using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoutScript : MonoBehaviour
{
    public void Logout()
    {
        // Effacer la variable PlayerPref contenant l'ID de l'utilisateur
        PlayerPrefs.DeleteKey("UserID");

        // Revenir à la scène "Login"
        SceneManager.LoadScene("Login");
    }
}