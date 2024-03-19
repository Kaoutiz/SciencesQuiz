using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class FriendList : MonoBehaviour
{
    public string apiURL = "http://localhost:3000/api";
    public GameObject friendPrefab;
    public Transform contentPanel;

    [System.Serializable]
    public class FriendData
    {
        public string _id;
        public string userId;
        public string friendId;
        public string dateAjout;
        public int __v;
    }

    [System.Serializable]
    public class FriendListData
    {
        public FriendData[] friends;
    }

    void Start()
    {
        DisplayFriendList();
    }

    public void DisplayFriendList()
    {
        string userId = PlayerPrefs.GetString("UserID");
        StartCoroutine(GetFriendList(userId));
    }

    IEnumerator GetFriendList(string userId)
    {
        string url = $"{apiURL}/Friend/list/{userId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseBody = request.downloadHandler.text;
                Debug.Log(responseBody);

                // Désérialisation de la réponse JSON
                FriendListData friendListData = JsonUtility.FromJson<FriendListData>(responseBody);

                if (friendListData == null || friendListData.friends == null)
                {
                    Debug.LogWarning("Failed to deserialize JSON or no friends found.");
                    yield break; // Sortir de la coroutine si la désérialisation échoue
                }

                // Utiliser les données des amis
                foreach (FriendData friend in friendListData.friends)
                {
                    string friendId = friend.friendId;
                    yield return StartCoroutine(GetPseudoFromId(friendId, (friendPseudo) =>
                    {
                        GameObject friendObject = Instantiate(friendPrefab, contentPanel);
                        friendObject.name = friendId; // Définir le nom de l'ami comme son ID
                        TextMeshProUGUI friendText = friendObject.GetComponentInChildren<TextMeshProUGUI>();
                        friendText.text = $"{friendPseudo}";

                        // Ajouter un bouton pour supprimer l'ami
                        Button deleteButton = friendObject.transform.Find("Background/ZoneBouton/DeleteButton").GetComponent<Button>();
                        deleteButton.onClick.AddListener(() => DeleteFriend(friendId));
                    }));
                }
            }
            else
            {
                Debug.LogWarning("Error getting friend list: " + request.error);
            }
        }
    }

    IEnumerator GetPseudoFromId(string friendId, Action<string> callback)
    {
        string url = $"{apiURL}/{friendId}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseBody = request.downloadHandler.text;
                string friendPseudo = ExtractPseudoFromJson(responseBody);
                callback(friendPseudo);
            }
            else
            {
                Debug.LogWarning("Error getting pseudo: " + request.error);
                callback(null);
            }
        }
    }

    string ExtractPseudoFromJson(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData))
        {
            Debug.LogWarning("JSON response is empty or null.");
            return null;
        }

        try
        {
            var data = JsonUtility.FromJson<DataWrapper>(jsonData);

            if (data == null || data.utilisateur == null)
            {
                Debug.LogWarning("Deserialization failed or user object is null.");
                return null;
            }

            string pseudo = data.utilisateur.pseudo;

            if (string.IsNullOrEmpty(pseudo))
            {
                Debug.LogWarning("Pseudo extracted from JSON response is empty or null.");
                return null;
            }

            return pseudo;
        }
        catch (Exception e)
        {
            Debug.LogError("Error deserializing JSON response: " + e);
            return null;
        }
    }

    [System.Serializable]
    public class DataWrapper
    {
        public UserData utilisateur;
    }

    [System.Serializable]
    public class UserData
    {
        public string pseudo;
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }

    void DeleteFriend(string friendId)
    {
        string userId = PlayerPrefs.GetString("UserID");
        StartCoroutine(DeleteFriendCoroutine(userId, friendId));
    }

    IEnumerator DeleteFriendCoroutine(string userId, string friendId)
    {
        string url = $"{apiURL}/Friend/{userId}/{friendId}";

        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Friend deleted successfully.");

                // Trouver l'objet ami dans la liste et le détruire avec son parent
                foreach (Transform child in contentPanel)
                {
                    if (child.name == friendId)
                    {
                        Destroy(child.gameObject.transform.parent.gameObject); // Détruire son parent (la ligne entière)
                        break;
                    }
                }
            }
            else
            {
                Debug.LogWarning("Error deleting friend: " + request.error);
            }
        }
    }
}
