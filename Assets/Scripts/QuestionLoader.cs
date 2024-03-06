using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class QuestionLoader : MonoBehaviour
{
    public string apiUrl = "https://votre-api.com/Questions";

    // M�thode pour charger les questions depuis l'API en fonction de la cat�gorie sp�cifi�e
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
            Debug.LogError("Erreur lors de la r�cup�ration des questions : " + request.error);
        }
        else
        {
            Debug.Log("reussi else");
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("R�ponse JSON des questions : " + jsonResponse);

            
        }
    }
    // M�thode appel�e lors du clic sur le bouton
    public void OnClick()
    {
        LoadQuestions();
    }
}
