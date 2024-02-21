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
        // R�initialiser les champs d'entr�e au d�marrage
        ResetInputFields();
    }

    public void SubmitRegistration()
    {
        string pseudo = pseudoInput.text;
        string password = passwordInput.text;

        // Cr�er une instance de UserData et d�finir le pseudo et le mot de passe
        UserData userData = new UserData();
        userData.pseudo = pseudo;
        userData.password = password;

        // Convertir l'objet UserData en une cha�ne JSON
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
            // Appliquer la couleur d'erreur aux champs d'entr�e de texte
            ChangeInputFieldColor(pseudoInput);
            ChangeInputFieldColor(passwordInput);
            // R�initialiser les champs d'entr�e de texte apr�s un d�lai
            StartCoroutine(ResetInputFieldsDelayed());
        }
        else
        {
            Debug.Log("Inscription r�ussie !");
            print(jsonStr);
            SceneManager.LoadScene("Login");
        }
    }

    // M�thode pour appliquer la couleur d'erreur aux champs d'entr�e de texte
    void ChangeInputFieldColor(InputField inputField)
    {
        inputField.image.color = errorColor;
    }

    // M�thode pour r�initialiser la couleur des champs d'entr�e de texte
    void ResetInputFieldColor(InputField inputField)
    {
        inputField.image.color = Color.white;
    }

    // M�thode pour r�initialiser les champs d'entr�e de texte apr�s un d�lai
    IEnumerator ResetInputFieldsDelayed()
    {
        yield return new WaitForSeconds(2f);
        ResetInputFields();
    }

    // M�thode pour r�initialiser les champs d'entr�e de texte
    void ResetInputFields()
    {
        pseudoInput.text = "";
        passwordInput.text = "";
        ResetInputFieldColor(pseudoInput);
        ResetInputFieldColor(passwordInput);
    }
}
