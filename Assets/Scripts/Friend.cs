using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using UnityEditor.PackageManager.Requests;
using UnityEngine.SceneManagement;

public class FriendManager : MonoBehaviour
{
    // URL de l'API
    public string apiURL = "http://localhost:3000/api";
    public InputField pseudoInput;
    public Color errorColor = Color.red;

    // Fonction pour ajouter un ami
    public void AddFriend()
    {
        string friendPseudo = pseudoInput.text;

        StartCoroutine(GetFriendIdAndSendRequest(friendPseudo));
    }

    IEnumerator GetFriendIdAndSendRequest(string friendPseudo)
    {
        // Récupérer l'ID de l'utilisateur connecté depuis PlayerPrefs
        string userId = PlayerPrefs.GetString("UserID");

        // Former l'URL avec le pseudo de l'ami
        string url = apiURL + "/Search/" + friendPseudo;

        // Créer une nouvelle requête GET
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Envoyer la requête
            yield return request.SendWebRequest();

            // Vérifier si la requête a réussi
            if (request.result == UnityWebRequest.Result.Success)
            {
                // Extraire l'ID de l'ami de la réponse JSON
                string jsonData = request.downloadHandler.text;
                string friendId = ExtractFriendIdFromJson(jsonData);

                // Vous pouvez appeler votre fonction SendFriendRequest ici en utilisant les données récupérées.
                StartCoroutine(SendFriendRequest(userId, friendId));
            }
            else
            {
                Debug.LogWarning("Erreur lors de la récupération de l'ID de l'ami : " + request.error);
                ChangeInputFieldColor(pseudoInput);
            }
        }
    }


    string ExtractFriendIdFromJson(string jsonData)
    {
        // Utilisez une méthode de désérialisation JSON appropriée pour extraire l'ID de l'ami
        // Dans cet exemple, je vais utiliser simplement le découpage de la chaîne JSON
        int startIndex = jsonData.IndexOf("_id") + 6; // Ajoutez 6 pour passer à l'ID réel
        int endIndex = jsonData.IndexOf("\"", startIndex);
        string friendId = jsonData.Substring(startIndex, endIndex - startIndex);
        return friendId;
    }

    [System.Serializable]
    public class FriendData
    {
        public string userId;
        public string friendId;
    }

    IEnumerator SendFriendRequest(string userId, string friendId)
    {
        // Créer une instance de FriendData et définir les IDs
        FriendData friendData = new FriendData();
        friendData.userId = userId;
        friendData.friendId = friendId;

        // Convertir l'objet FriendData en une chaîne JSON
        string jsonData = JsonUtility.ToJson(friendData);

        Debug.Log(jsonData);

        // Définir l'URL de la requête
        string url = "http://localhost:3000/api/Friend";

        // Créer une requête POST avec UnityWebRequest
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Envoyer la requête
        yield return request.SendWebRequest();

        // Vérifier si la requête a réussi
        if (request.result != UnityWebRequest.Result.Success)
        {
            if (request.responseCode == 401) // Utilisateur déjà en amis
            {
                Debug.LogWarning("Vous êtes déjà amis avec cet utilisateur");

            }
            else if (request.responseCode == 402) // Déjà une demande qui existe
            {
                Debug.LogWarning("Vous avez déjà une demande d'ami en attente pour cet utilisateur");
            }
            else if (request.responseCode == 403) // Demande d'amis déjà envoyé
            {
                Debug.LogWarning("Vous avez déjà envoyé une demande d'ami à cet utilisateur");
            }
            else
            {
                Debug.LogWarning("Erreur lors de la connexion : " + request.responseCode);
            }
        }
        else
        {
            if (request.responseCode == 200) // Demande d'amis réussie
            {
                Debug.Log("Demande d'amis envoyé avec succès !");
            }
        }
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
