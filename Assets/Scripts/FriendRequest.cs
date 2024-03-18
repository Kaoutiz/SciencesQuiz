using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;

public class FriendRequest : MonoBehaviour
{
    public string apiURL = "http://localhost:3000/api";
    public GameObject friendRequestPrefab;
    public Transform contentPanel;

    void Start()
    {
        DisplayFriendRequests();
    }

    public void DisplayFriendRequests()
    {
        // Récupérer l'ID de l'utilisateur connecté depuis PlayerPrefs
        string userId = PlayerPrefs.GetString("UserID");
        Debug.Log(userId);

        StartCoroutine(GetFriendRequests(userId));
    }

    IEnumerator GetFriendRequests(string userId)
    {
        // Construire l'URL de la route pour récupérer les demandes d'amis de l'utilisateur
        string url = $"{apiURL}/Friend/{userId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Récupérer la réponse JSON
                string responseBody = request.downloadHandler.text;

                Debug.Log(responseBody);

                // Désérialiser la réponse JSON en utilisant une classe représentant le tableau JSON
                FriendRequestData[] friendRequests = JsonHelper.FromJson<FriendRequestData>(responseBody);

                // Effacer les anciens éléments
                foreach (Transform child in contentPanel)
                {
                    Destroy(child.gameObject);
                }

                // Pour chaque demande d'ami
                foreach (FriendRequestData friendRequest in friendRequests)
                {
                    // Récupérer l'ID de l'ami
                    string friendId = friendRequest.friendId;

                    // Récupérer le pseudo de l'ami
                    yield return StartCoroutine(GetPseudoFromId(friendId, (friendPseudo) =>
                    {
                        // Créer un nouveau GameObject à partir du prefab
                        GameObject friendRequestObject = Instantiate(friendRequestPrefab, contentPanel);

                        // Récupérer le composant TextMeshProUGUI pour afficher la demande
                        TextMeshProUGUI friendRequestText = friendRequestObject.GetComponentInChildren<TextMeshProUGUI>();

                        // Définir le texte de la demande avec l'ID de l'ami et son pseudo
                        friendRequestText.text = $"{friendPseudo}";

                        // Ajouter des boutons pour accepter et refuser la demande
                        Button acceptButton = friendRequestObject.transform.Find("AcceptButton").GetComponent<Button>();
                        acceptButton.onClick.AddListener(() => AcceptFriendRequest(friendId));

                        Button rejectButton = friendRequestObject.transform.Find("RejectButton").GetComponent<Button>();
                        rejectButton.onClick.AddListener(() => RejectFriendRequest(friendId));
                    }));
                }

            }
            else
            {
                Debug.LogWarning("Erreur lors de la récupération des demandes d'amis de l'utilisateur : " + request.error);
            }
        }
    }

    IEnumerator GetPseudoFromId(string friendId, System.Action<string> callback)
    {
        string url = $"{apiURL}/{friendId}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Récupérer la réponse JSON
                string responseBody = request.downloadHandler.text;

                // Extraire le pseudo de la réponse JSON
                string friendPseudo = ExtractPseudoFromJson(responseBody);

                // Appeler le callback avec le pseudo
                callback(friendPseudo);
            }
            else
            {
                Debug.LogWarning("Erreur lors de la récupération du pseudo : " + request.error);
                callback(null); // Indiquer une erreur en appelant le callback avec null
            }
        }
    }

    string ExtractPseudoFromJson(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData))
        {
            Debug.LogWarning("La réponse JSON est vide ou nulle.");
            return null;
        }

        try
        {
            // Désérialiser la réponse JSON en un objet contenant "utilisateur"
            var data = JsonUtility.FromJson<DataWrapper>(jsonData);

            if (data == null || data.utilisateur == null)
            {
                Debug.LogWarning("La désérialisation de la réponse JSON a échoué ou l'objet utilisateur est nul.");
                return null;
            }

            // Extraire le pseudo de l'utilisateur
            string pseudo = data.utilisateur.pseudo;

            if (string.IsNullOrEmpty(pseudo))
            {
                Debug.LogWarning("Le pseudo extrait de la réponse JSON est vide ou nul.");
                return null;
            }

            return pseudo;
        }
        catch (Exception e)
        {
            Debug.LogError("Erreur lors de la désérialisation de la réponse JSON : " + e);
            return null;
        }
    }

    void AcceptFriendRequest(string friendId)
    {
        Debug.Log("Accepter la demande de l'ami avec l'ID : " + friendId);
        // Modifier la demande d'ami avec l'ID de l'utilisateur et l'ID de l'ami
        StartCoroutine(PatchFriendRequest(friendId, "accepted"));
    }

    void RejectFriendRequest(string friendId)
    {
        Debug.Log("Refuser la demande de l'ami avec l'ID : " + friendId);
        // Modifier la demande d'ami avec l'ID de l'utilisateur et l'ID de l'ami
        StartCoroutine(PatchFriendRequest(friendId, "rejected"));
    }

    IEnumerator PatchFriendRequest(string friendId, string newStatus)
    {
        // Construire l'URL de la route pour modifier la demande d'ami
        string url = $"{apiURL}/Friend";

        // Construire les données JSON pour la requête PATCH
        string jsonData = JsonUtility.ToJson(new FriendRequestPatchData(friendId, newStatus));

        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Modification de la demande d'ami réussie !");
                // Actualiser l'affichage des demandes d'amis
                DisplayFriendRequests();
            }
            else
            {
                Debug.LogWarning("Erreur lors de la modification de la demande d'ami : " + request.error);
            }
        }
    }

    // Classe pour envelopper la réponse JSON
    [Serializable]
    public class DataWrapper
    {
        public UserData utilisateur;
    }

    // Classe représentant les données utilisateur dans la réponse JSON
    [Serializable]
    public class UserData
    {
        public string pseudo;
    }

    [Serializable]
    public class FriendRequestData
    {
        public string _id;
        public string userId;
        public string friendId;
        public string status;
        public int __v;
    }

    // Classe utilitaire pour la désérialisation des tableaux JSON
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }

    // Classe pour les données à envoyer lors de la requête PATCH
    [Serializable]
    public class FriendRequestPatchData
    {
        public string userId;
        public string friendId;
        public string nouvelEtat;

        public FriendRequestPatchData(string friendId, string nouvelEtat)
        {
            this.userId = PlayerPrefs.GetString("UserID");
            this.friendId = friendId;
            this.nouvelEtat = nouvelEtat;
        }
    }
}
