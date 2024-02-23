using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    public InputField pseudoInput;
    public InputField passwordInput;
    public string apiURL = "http://localhost:3000/api/Login";
    public Color errorColor = Color.red;

    [System.Serializable]
    public class UserData
    {
        public string pseudo;
        public string password;
        public string id;
    }

    public void SubmitLogin()
    {
        string pseudo = pseudoInput.text;
        string password = passwordInput.text;

        // Créez une instance de UserData et définissez le nom d'utilisateur
        UserData userData = new UserData();
        userData.pseudo = pseudo;
        userData.password = password;

        // Convertissez l'objet UserData en une chaîne JSON à l'aide de JsonUtility
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
            if (request.responseCode == 404) // Utilisateur non trouvé
            {
                Debug.LogWarning("Identifiants incorrects !");
                // Appliquer la couleur d'erreur aux champs d'entrée de texte
                ChangeInputFieldColor(pseudoInput);
                
            }
            else if (request.responseCode == 401) // Mot de passe incorrect
            {
                Debug.LogWarning("Mot de passe incorrects !");
                // Appliquer la couleur d'erreur aux champs d'entrée de texte
                
                ChangeInputFieldColor(passwordInput);
            }
            else
            {
                Debug.LogWarning("Erreur lors de la connexion : " + request.responseCode);
            }
        }
        else
        {
            if (request.responseCode == 200) // Connexion réussie
            {
                string jsonData = request.downloadHandler.text;
                Debug.Log("Données de l'API reçues: " + jsonData);

                // Récupérer l'ID de l'utilisateur depuis les données JSON et le stocker
                UserData userData = JsonUtility.FromJson<UserData>(jsonData);
                string userId = ExtractUserIdFromJson(jsonData);
                // Enregistrez l'ID de l'utilisateur dans les PlayerPrefs
                PlayerPrefs.SetString("UserID", userId);
            
               SceneManager.LoadScene("Main"); // Charger la scène principale après la connexion réussie
            }
        }
    }

    string ExtractUserIdFromJson(string jsonData)
    {
        // Utilisez une méthode de désérialisation JSON appropriée pour extraire l'ID de l'utilisateur
        // Dans cet exemple, je vais utiliser simplement le découpage de la chaîne JSON
        int startIndex = jsonData.IndexOf("_id") + 6; // Ajoutez 6 pour passer à l'ID réel
        int endIndex = jsonData.IndexOf("\"", startIndex);
        string userId = jsonData.Substring(startIndex, endIndex - startIndex);
        return userId;
    }

    void ChangeInputFieldColor(InputField inputField)
    {
        inputField.image.color = errorColor;
        StartCoroutine(ResetInputFieldColor(inputField));
    }

    IEnumerator ResetInputFieldColor(InputField inputField)
    {
        yield return new WaitForSeconds(2f);
        inputField.image.color = Color.white;
    }
}
