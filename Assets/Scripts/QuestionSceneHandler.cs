using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using System.Text;

public class QuestionSceneHandler : MonoBehaviour
{
    public Text questionText;
    public Text chronoText;
    public Text questionCounterText;
    public Button nextButton;
    public Button homeButton;
    public Transform buttonPanel;
    public GameObject answerButtonPrefab; // Prefab du bouton de réponse
    public Sprite[] backgroundImages; // Tableau d'images de fond aléatoires pour les reponses
    public Sprite[] backgroundImagesQuestion; // Tableau d'images de fond aléatoires pour les questions

    public float countdownTime = 60f;
    public string apiURL = APIConfig.apiURL + "chemin";

    public Image questionPanelBackground;

    private List<QuestionData> questions;
    private int currentQuestionIndex = 0;
    private int correctAnswersCount = 0;
    private bool isAnswered = false;
    private string completionMessage;

    private Coroutine countdownCoroutine;

    void Start()
    {
        LoadQuestions();
    }

    void LoadQuestions()
    {
        if (PlayerPrefs.HasKey("Questions"))
        {
            string questionsJson = PlayerPrefs.GetString("Questions");
            QuestionsWrapper wrapper = JsonUtility.FromJson<QuestionsWrapper>("{\"questions\":" + questionsJson + "}");
            questions = new List<QuestionData>(wrapper.questions);
            DisplayQuestion();
        }
        else
        {
            Debug.LogWarning("Aucune question trouvée dans PlayerPrefs.");
        }
    }

    void DisplayQuestion()
    {
        // Réinitialiser le texte de la question
        questionText.text = "";
        chronoText.text = "";

        // Afficher la question actuelle
        QuestionData currentQuestion = questions[currentQuestionIndex];
        questionText.text = "" + currentQuestion.question;

        // Mettre à jour le compteur de questions
        questionCounterText.text = "Question " + (currentQuestionIndex + 1) + " / " + questions.Count;

        // Choisir aléatoirement une image de fond pour le panneau de la question
        Sprite randomBackground = backgroundImagesQuestion[Random.Range(0, backgroundImagesQuestion.Length)];
        questionPanelBackground.sprite = randomBackground;

        // Afficher les réponses
        DisplayAnswers(currentQuestion);

        // Démarrer le compte à rebours
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);
        countdownCoroutine = StartCoroutine(Countdown());
    }

    void DisplayAnswers(QuestionData question)
    {
        // Effacer les anciens boutons de réponse
        foreach (Transform child in buttonPanel)
        {
            Destroy(child.gameObject);
        }

        // Afficher les réponses dynamiquement
        DisplayAnswer(question.reponse1.texte);
        DisplayAnswer(question.reponse2.texte);
        DisplayAnswer(question.reponse3.texte);
        DisplayAnswer(question.reponse4.texte);
    }

    void DisplayAnswer(string answerText)
    {
        // Instancier un nouveau bouton de réponse dans le panneau des réponses
        GameObject answerButtonGO = Instantiate(answerButtonPrefab, buttonPanel);
        Button answerButton = answerButtonGO.GetComponent<Button>();
        answerButton.GetComponentInChildren<Text>().text = answerText;
        // Choix aléatoire de l'image de fond
        Sprite randomBackground = backgroundImages[Random.Range(0, backgroundImages.Length)];
        Image buttonImage = answerButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = randomBackground;
        }
        else
        {
            Debug.LogError("Le composant Image n'est pas trouvé dans le préfabriqué du bouton de réponse.");
        }

        answerButton.onClick.AddListener(() => OnAnswerSelected(answerText));
    }

    IEnumerator Countdown()
    {
        float timer = countdownTime;
        while (timer > 0f && !isAnswered)
        {
            timer -= Time.deltaTime;
            chronoText.text = Mathf.Round(timer).ToString(); // Afficher uniquement le temps restant
            yield return null;
        }

        if (!isAnswered)
        {
            // Si le temps est écoulé et que l'utilisateur n'a pas répondu, afficher la réponse correcte
            DisplayCorrectAnswer();
        }
    }

    void DisplayCorrectAnswer()
    {
        QuestionData currentQuestion = questions[currentQuestionIndex];
        string correctAnswer = "";

        // Trouver la bonne réponse
        if (currentQuestion.bonne_reponse.Equals("reponse1"))
            correctAnswer = currentQuestion.reponse1.texte;
        else if (currentQuestion.bonne_reponse.Equals("reponse2"))
            correctAnswer = currentQuestion.reponse2.texte;
        else if (currentQuestion.bonne_reponse.Equals("reponse3"))
            correctAnswer = currentQuestion.reponse3.texte;
        else if (currentQuestion.bonne_reponse.Equals("reponse4"))
            correctAnswer = currentQuestion.reponse4.texte;

        // Afficher la réponse correcte
        Debug.Log("La réponse correcte est : " + correctAnswer);

        // Afficher les bonnes et mauvaises réponses
        for (int i = 0; i < buttonPanel.childCount; i++)
        {
            Button answerButton = buttonPanel.GetChild(i).GetComponent<Button>();
            if (answerButton.GetComponentInChildren<Text>().text.Equals(correctAnswer))
            {
                answerButton.image.color = Color.green; // Afficher en vert la bonne réponse
            }
            else
            {
                answerButton.image.color = Color.red; // Afficher en rouge la mauvaise réponse
            }
        }

        // Afficher le bouton pour passer à la question suivante
        nextButton.gameObject.SetActive(true);
    }

    public void OnAnswerSelected(string answerText)
    {
        isAnswered = true;
        int answerIndex = GetAnswerIndex(answerText);
        bool isCorrect = questions[currentQuestionIndex].bonne_reponse.Equals("reponse" + (answerIndex + 1));

        // Incrémenter correctAnswersCount si la réponse est correcte
        if (isCorrect)
        {
            correctAnswersCount++;
            string userId = PlayerPrefs.GetString("UserID");
            StartCoroutine(UpdateUserExperience(userId, 100));
        }
        StartCoroutine(IncrementerChoixReponse(currentQuestionIndex, answerIndex + 1));
        // Mettre en vert ou rouge la réponse sélectionnée
        Color answerColor = isCorrect ? Color.green : Color.red;
        buttonPanel.GetChild(answerIndex).GetComponent<Image>().color = answerColor;

        DisplayCorrectAnswer();
        // Désactiver les autres boutons
        for (int i = 0; i < buttonPanel.childCount; i++)
        {
            buttonPanel.GetChild(i).GetComponent<Button>().interactable = false;
        }

        nextButton.gameObject.SetActive(true);
    }

    IEnumerator IncrementerChoixReponse(int questionIndex, int reponseIndex)
    {
        // Construire l'URL pour l'API avec les paramètres nécessaires
        string url = APIConfig.apiURL + "Questions/" + questions[questionIndex]._id + "/" + reponseIndex;
        Debug.Log(url);

        // Créez les données du formulaire ici, puis encodez-les sous forme d'octets []
        string formData = ""; // Vous devez remplir formData avec les données nécessaires
        byte[] formDataBytes = System.Text.Encoding.UTF8.GetBytes(formData);

        // Créez une requête UnityWebRequest avec les données du formulaire
        UnityWebRequest request = UnityWebRequest.Put(url, formDataBytes);

        // Spécifiez la méthode PATCH
        request.method = "PATCH";

        // Ajoutez les en-têtes nécessaires
        request.SetRequestHeader("Content-Type", "application/json");

        // Envoyez la requête
        yield return request.SendWebRequest();

        // Vérifiez le résultat de la requête
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Echec de la requête : " + request.error);
        }
        else
        {
            Debug.Log("Choix de réponse incrémenté avec succès pour la question " + questions[questionIndex]._id + ", réponse " + reponseIndex);
        }
    }

    IEnumerator UpdateUserExperience(string userId, int experience)
    {
        // Construire l'URL pour l'API avec les paramètres nécessaires
        string url = APIConfig.apiURL + userId + "/experience";

        // Créer un objet JSON contenant les données à envoyer
        string json = "{\"experience\": " + experience + "}";

        // Convertir le JSON en tableau d'octets
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // Créer une requête UnityWebRequest avec les données JSON
        UnityWebRequest request = new UnityWebRequest(url, "PATCH");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Envoyer la requête
        yield return request.SendWebRequest();

        // Vérifier le résultat de la requête
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Echec de la requête : " + request.error);
        }
        else
        {
            Debug.Log("Expérience de l'utilisateur mise à jour avec succès");
        }
    }

    int GetAnswerIndex(string answerText)
    {
        // Trouver l'index de la réponse dans les réponses de la question actuelle
        QuestionData currentQuestion = questions[currentQuestionIndex];
        if (currentQuestion.reponse1.texte.Equals(answerText))
            return 0;
        else if (currentQuestion.reponse2.texte.Equals(answerText))
            return 1;
        else if (currentQuestion.reponse3.texte.Equals(answerText))
            return 2;
        else if (currentQuestion.reponse4.texte.Equals(answerText))
            return 3;
        else
            return -1;
    }

    public void OnNextButtonClicked()
    {
        Debug.Log("Bouton 'Next' cliqué."); // Débogage pour vérifier si le bouton Next est cliqué
        currentQuestionIndex++;
        isAnswered = false;
        nextButton.gameObject.SetActive(false); // Cacher le bouton "Next" avant de passer à la prochaine question
        if (currentQuestionIndex < questions.Count)
        {
            DisplayQuestion();
        }
        else
        {
            Debug.Log("Fin des questions.");
            completionMessage = "Bien joué, vous avez " + correctAnswersCount + " bonnes réponses";
            questionText.text = completionMessage;
            chronoText.gameObject.SetActive(false);
            questionCounterText.gameObject.SetActive(false);
            buttonPanel.gameObject.SetActive(false);
            homeButton.gameObject.SetActive(true);
        }
    }
}

[System.Serializable]
public class QuestionsWrapper
{
    public QuestionData[] questions;
}

[System.Serializable]
public class QuestionData
{
    public string _id;
    public string categorie;
    public string question;
    public ReponseData reponse1;
    public ReponseData reponse2;
    public ReponseData reponse3;
    public ReponseData reponse4;
    public string bonne_reponse;
}

[System.Serializable]
public class ReponseData
{
    public string texte;
    public int choix;
}
