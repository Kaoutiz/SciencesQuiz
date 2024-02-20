using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class Register : MonoBehaviour
{
    public InputField pseudoInput;
    public InputField passwordInput;
    public string apiURL = "http://localhost:3000/api/Register";

    [System.Serializable]
    public class UserData
    {
        public string pseudo;
        public string password;
    }
    
    public void SubmitRegistration()
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
            Debug.LogError(request.error);
        }
        else
        {
            Debug.Log("Inscription réussie !");
            print(jsonStr);
        }
    }
}
