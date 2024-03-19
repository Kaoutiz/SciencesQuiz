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
    public GameObject answerButtonPrefab; // Prefab du bouton de r�ponse
    public Sprite[] backgroundImages; // Tableau d'images de fond al�atoires pour les reponses
    public Sprite[] backgroundImagesQuestion; // Tableau d'images de fond al�atoires pour les questions

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
            Debug.LogWarning("Aucune question trouv�e dans PlayerPrefs.");
        }
    }

    void DisplayQuestion()
    {
        // R�initialiser le texte de la question
        questionText.text = "";
        chronoText.text = "";

        // Afficher la question actuelle
        QuestionData currentQuestion = questions[currentQuestionIndex];
        questionText.text = "" + currentQuestion.question;

        // Mettre � jour le compteur de questions
        questionCounterText.text = "Question " + (currentQuestionIndex + 1) + " / " + questions.Count;

        // Choisir al�atoirement une image de fond pour le panneau de la question
        Sprite randomBackground = backgroundImagesQuestion[Random.Range(0, backgroundImagesQuestion.Length)];
        questionPanelBackground.sprite = randomBackground;

        // Afficher les r�ponses
        DisplayAnswers(currentQuestion);

        // D�marrer le compte � rebours
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);
        countdownCoroutine = StartCoroutine(Countdown());
    }

    void DisplayAnswers(QuestionData question)
    {
        // Effacer les anciens boutons de r�ponse
        foreach (Transform child in buttonPanel)
        {
            Destroy(child.gameObject);
        }

        // Afficher les r�ponses dynamiquement
        DisplayAnswer(question.reponse1.texte);
        DisplayAnswer(question.reponse2.texte);
        DisplayAnswer(question.reponse3.texte);
        DisplayAnswer(question.reponse4.texte);
    }

    void DisplayAnswer(string answerText)
    {
        // Instancier un nouveau bouton de r�ponse dans le panneau des r�ponses
        GameObject answerButtonGO = Instantiate(answerButtonPrefab, buttonPanel);
        Button answerButton = answerButtonGO.GetComponent<Button>();
        answerButton.GetComponentInChildren<Text>().text = answerText;
        // Choix al�atoire de l'image de fond
        Sprite randomBackground = backgroundImages[Random.Range(0, backgroundImages.Length)];
        Image buttonImage = answerButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = randomBackground;
        }
        else
        {
            Debug.LogError("Le composant Image n'est pas trouv� dans le pr�fabriqu� du bouton de r�ponse.");
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
            // Si le temps est �coul� et que l'utilisateur n'a pas r�pondu, afficher la r�ponse correcte
            DisplayCorrectAnswer();
        }
    }

    void DisplayCorrectAnswer()
    {
        QuestionData currentQuestion = questions[currentQuestionIndex];
        string correctAnswer = "";

        // Trouver la bonne r�ponse
        if (currentQuestion.bonne_reponse.Equals("reponse1"))
            correctAnswer = currentQuestion.reponse1.texte;
        else if (currentQuestion.bonne_reponse.Equals("reponse2"))
            correctAnswer = currentQuestion.reponse2.texte;
        else if (currentQuestion.bonne_reponse.Equals("reponse3"))
            correctAnswer = currentQuestion.reponse3.texte;
        else if (currentQuestion.bonne_reponse.Equals("reponse4"))
            correctAnswer = currentQuestion.reponse4.texte;

        // Afficher la r�ponse correcte
        Debug.Log("La r�ponse correcte est : " + correctAnswer);

        // Afficher les bonnes et mauvaises r�ponses
        for (int i = 0; i < buttonPanel.childCount; i++)
        {
            Button answerButton = buttonPanel.GetChild(i).GetComponent<Button>();
            if (answerButton.GetComponentInChildren<Text>().text.Equals(correctAnswer))
            {
                answerButton.image.color = Color.green; // Afficher en vert la bonne r�ponse
            }
            else
            {
                answerButton.image.color = Color.red; // Afficher en rouge la mauvaise r�ponse
            }
        }

        // Afficher le bouton pour passer � la question suivante
        nextButton.gameObject.SetActive(true);
    }

    public void OnAnswerSelected(string answerText)
    {
        isAnswered = true;
        int answerIndex = GetAnswerIndex(answerText);
        bool isCorrect = questions[currentQuestionIndex].bonne_reponse.Equals("reponse" + (answerIndex + 1));

        // Incr�menter correctAnswersCount si la r�ponse est correcte
        if (isCorrect)
        {
            correctAnswersCount++;
            string userId = PlayerPrefs.GetString("UserID");
            StartCoroutine(UpdateUserExperience(userId, 100));
        }
        StartCoroutine(IncrementerChoixReponse(currentQuestionIndex, answerIndex + 1));
        // Mettre en vert ou rouge la r�ponse s�lectionn�e
        Color answerColor = isCorrect ? Color.green : Color.red;
        buttonPanel.GetChild(answerIndex).GetComponent<Image>().color = answerColor;

        DisplayCorrectAnswer();
        // D�sactiver les autres boutons
        for (int i = 0; i < buttonPanel.childCount; i++)
        {
            buttonPanel.GetChild(i).GetComponent<Button>().interactable = false;
        }

        nextButton.gameObject.SetActive(true);
    }

    IEnumerator IncrementerChoixReponse(int questionIndex, int reponseIndex)
    {
        // Construire l'URL pour l'API avec les param�tres n�cessaires
        string url = APIConfig.apiURL + "Questions/" + questions[questionIndex]._id + "/" + reponseIndex;
        Debug.Log(url);

        // Cr�ez les donn�es du formulaire ici, puis encodez-les sous forme d'octets []
        string formData = ""; // Vous devez remplir formData avec les donn�es n�cessaires
        byte[] formDataBytes = System.Text.Encoding.UTF8.GetBytes(formData);

        // Cr�ez une requ�te UnityWebRequest avec les donn�es du formulaire
        UnityWebRequest request = UnityWebRequest.Put(url, formDataBytes);

        // Sp�cifiez la m�thode PATCH
        request.method = "PATCH";

        // Ajoutez les en-t�tes n�cessaires
        request.SetRequestHeader("Content-Type", "application/json");

        // Envoyez la requ�te
        yield return request.SendWebRequest();

        // V�rifiez le r�sultat de la requ�te
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Echec de la requ�te : " + request.error);
        }
        else
        {
            Debug.Log("Choix de r�ponse incr�ment� avec succ�s pour la question " + questions[questionIndex]._id + ", r�ponse " + reponseIndex);
        }
    }

    IEnumerator UpdateUserExperience(string userId, int experience)
    {
        // Construire l'URL pour l'API avec les param�tres n�cessaires
        string url = APIConfig.apiURL + userId + "/experience";

        // Cr�er un objet JSON contenant les donn�es � envoyer
        string json = "{\"experience\": " + experience + "}";

        // Convertir le JSON en tableau d'octets
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // Cr�er une requ�te UnityWebRequest avec les donn�es JSON
        UnityWebRequest request = new UnityWebRequest(url, "PATCH");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Envoyer la requ�te
        yield return request.SendWebRequest();

        // V�rifier le r�sultat de la requ�te
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Echec de la requ�te : " + request.error);
        }
        else
        {
            Debug.Log("Exp�rience de l'utilisateur mise � jour avec succ�s");
        }
    }

    int GetAnswerIndex(string answerText)
    {
        // Trouver l'index de la r�ponse dans les r�ponses de la question actuelle
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
        Debug.Log("Bouton 'Next' cliqu�."); // D�bogage pour v�rifier si le bouton Next est cliqu�
        currentQuestionIndex++;
        isAnswered = false;
        nextButton.gameObject.SetActive(false); // Cacher le bouton "Next" avant de passer � la prochaine question
        if (currentQuestionIndex < questions.Count)
        {
            DisplayQuestion();
        }
        else
        {
            Debug.Log("Fin des questions.");
            completionMessage = "Bien jou�, vous avez " + correctAnswersCount + " bonnes r�ponses";
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
