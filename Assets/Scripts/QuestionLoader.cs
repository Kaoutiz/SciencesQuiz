using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class QuestionLoader : MonoBehaviour
{
    public string apiUrl = "https://votre-api.com/Questions";

    // Méthode pour charger les questions depuis l'API en fonction de la catégorie spécifiée
    public void LoadQuestions(string categorie = "")
    {
        Debug.Log("reussi");
        string urlWithParams = apiUrl;
        if (!string.IsNullOrEmpty(categorie))
        {
            urlWithParams += "?categorie=" + categorie;
        }
        Debug.Log(urlWithParams);
        StartCoroutine(GetQuestions(urlWithParams));
    }

    IEnumerator GetQuestions(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("reussi if");
            Debug.LogError("Erreur lors de la récupération des questions : " + request.error);
        }
        else
        {
            Debug.Log("reussi else");
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Réponse JSON des questions : " + jsonResponse);

            
        }
    }
    // Méthode appelée lors du clic sur le bouton
    public void OnClick()
    {
        LoadQuestions();
    }
}
