using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuestionLoader : MonoBehaviour
{
    public string apiUrl = APIConfig.apiURL + "Questions";
    public Button loadButton;

    void Start()
    {
        loadButton.onClick.AddListener(OnClickLoadQuestions);
    }

    void OnClickLoadQuestions()
    {
        LoadQuestions();
    }

    public void LoadQuestions(string categorie = "")
    {
        string urlWithParams = apiUrl;
        if (!string.IsNullOrEmpty(categorie))
        {
            urlWithParams += "?categorie=" + categorie;
        }
        StartCoroutine(GetQuestions(urlWithParams));
    }

    IEnumerator GetQuestions(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erreur lors de la récupération des questions : " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            // Traitement de la réponse JSON
            Debug.Log("JSON avant désérialisation : " + jsonResponse);

            // Enregistrer les questions dans PlayerPrefs
            SaveQuestions(jsonResponse);
        }
    }

    void SaveQuestions(string jsonResponse)
    {
        PlayerPrefs.SetString("Questions", jsonResponse);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Question");
    }
}
