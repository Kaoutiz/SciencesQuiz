using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Register : MonoBehaviour
{
    public InputField pseudoInput;
    public InputField passwordInput;
    public string apiURL = "http://localhost:3000/api/Register";
    public Color errorColor = Color.red;

    [System.Serializable]
    public class UserData
    {
        public string pseudo;
        public string password;
    }

    private void Start()
    {
        // Réinitialiser les champs d'entrée au démarrage
        ResetInputFields();
    }

    public void SubmitRegistration()
    {
        string pseudo = pseudoInput.text;
        string password = passwordInput.text;

        // Créer une instance de UserData et définir le pseudo et le mot de passe
        UserData userData = new UserData();
        userData.pseudo = pseudo;
        userData.password = password;

        // Convertir l'objet UserData en une chaîne JSON
        string jsonStr = JsonUtility.ToJson(userData);

        StartCoroutine(SendRequest(jsonStr));
    }

    IEnumerator SendRequest(string jsonStr)
    {
        var request = new UnityWebRequest(apiURL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonStr);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            // Appliquer la couleur d'erreur aux champs d'entrée de texte
            ChangeInputFieldColor(pseudoInput);
            ChangeInputFieldColor(passwordInput);
            // Réinitialiser les champs d'entrée de texte après un délai
            StartCoroutine(ResetInputFieldsDelayed());
        }
        else
        {
            Debug.Log("Inscription réussie !");
            print(jsonStr);
            SceneManager.LoadScene("Login");
        }
    }

    // Méthode pour appliquer la couleur d'erreur aux champs d'entrée de texte
    void ChangeInputFieldColor(InputField inputField)
    {
        inputField.image.color = errorColor;
    }

    // Méthode pour réinitialiser la couleur des champs d'entrée de texte
    void ResetInputFieldColor(InputField inputField)
    {
        inputField.image.color = Color.white;
    }

    // Méthode pour réinitialiser les champs d'entrée de texte après un délai
    IEnumerator ResetInputFieldsDelayed()
    {
        yield return new WaitForSeconds(2f);
        ResetInputFields();
    }

    // Méthode pour réinitialiser les champs d'entrée de texte
    void ResetInputFields()
    {
        pseudoInput.text = "";
        passwordInput.text = "";
        ResetInputFieldColor(pseudoInput);
        ResetInputFieldColor(passwordInput);
    }
}
