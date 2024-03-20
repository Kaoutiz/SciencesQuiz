using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ClassementList : MonoBehaviour
{
    public string apiURL = "http://localhost:3000/api";
    public GameObject classementPrefab;
    public Transform contentPanel;

    [System.Serializable]
    public class PlayerData
    {
        public string _id;
        public string pseudo;
        public int experience;
    }

    [System.Serializable]
    public class PlayerListData
    {
        public List<PlayerData> players;
    }

    void Start()
    {
        StartCoroutine(GetPlayerLeaderboard());
    }

    IEnumerator GetPlayerLeaderboard()
    {
        string url = $"{apiURL}/users";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseBody = request.downloadHandler.text;
                Debug.Log(responseBody);

                // V�rification de si la r�ponse est un tableau vide
                if (responseBody.Equals("[]"))
                {
                    Debug.Log("No players found.");
                    yield break;
                }

                // D�s�rialisation de la r�ponse JSON
                PlayerListData playerListData = JsonUtility.FromJson<PlayerListData>("{\"players\":" + responseBody + "}");

                // Tri des joueurs par exp�rience de la plus grande � la plus petite
                List<PlayerData> sortedPlayers = playerListData.players.OrderByDescending(player => player.experience).ToList();

                // Affichage des joueurs dans le classement avec leur position, pseudo et score
                for (int i = 0; i < sortedPlayers.Count; i++)
                {
                    PlayerData player = sortedPlayers[i];
                    GameObject playerObject = Instantiate(classementPrefab, contentPanel);

                    // R�cup�rer les composants TextMeshProUGUI dans l'objet joueur
                    TextMeshProUGUI positionText = playerObject.transform.Find("Background/Panel/Position").GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI pseudoText = playerObject.transform.Find("Background/Panel/Pseudo").GetComponent<TextMeshProUGUI>();
                    TextMeshProUGUI scoreText = playerObject.transform.Find("Background/Panel/Score").GetComponent<TextMeshProUGUI>();

                    // Attribution des valeurs aux composants de texte
                    positionText.text = $"{i + 1}";
                    pseudoText.text = $"{player.pseudo}";
                    scoreText.text = $"{player.experience}";
                }
            }
            else
            {
                Debug.LogWarning("Error getting player leaderboard: " + request.error);
            }
        }
    }
}
