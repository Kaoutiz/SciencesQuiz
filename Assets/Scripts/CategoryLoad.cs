using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CategoryLoad : MonoBehaviour
{
    public string apiUrl = APIConfig.apiURL + "categories";
    public QuestionLoader questionLoader; // R�f�rence au script QuestionLoader
    public GameObject buttonPrefab; // Prefab de bouton � instancier
    public Transform buttonContainer; // Parent des boutons dans la hi�rarchie
    public Sprite[] backgroundImages; // Tableau d'images de fond al�atoires

    [System.Serializable]
    public class CategoriesResponse
    {
        public string message;
        public List<CategoryData> categories_list;
    }

    [System.Serializable]
    public class CategoryData
    {
        public string _id;
        public string categorie;
    }

    private void Start()
    {
        StartCoroutine(GetCategories());
    }

    IEnumerator GetCategories()
    {
        Debug.Log("R�cup�ration des cat�gories depuis l'API...");

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erreur lors de la r�cup�ration des cat�gories : " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            CategoriesResponse response = JsonUtility.FromJson<CategoriesResponse>(jsonResponse);

            Debug.Log("Message de la r�ponse : " + response.message);

            Debug.Log("Cat�gories r�cup�r�es avec succ�s :");
            Debug.Log(jsonResponse);

            foreach (CategoryData categoryData in response.categories_list)
            {
                Debug.Log("Cat�gorie : " + categoryData.categorie);
                CreateCategoryButton(categoryData.categorie);
            }
        }
    }

    void CreateCategoryButton(string categoryName)
    {
        Debug.Log("Cr�ation du bouton pour la cat�gorie : " + categoryName);

        GameObject newButton = Instantiate(buttonPrefab, buttonContainer.transform); // Assurez-vous que le conteneur de boutons est le parent du nouveau bouton

        // D�finir le texte avec le nom de la cat�gorie
        Text buttonText = newButton.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = categoryName;
        }
        else
        {
            Debug.LogError("Le composant Text n'est pas trouv� dans le pr�fabriqu� du bouton.");
            return;
        }

        // Choix al�atoire de l'image de fond
        Sprite randomBackground = backgroundImages[Random.Range(0, backgroundImages.Length)];
        Image buttonImage = newButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = randomBackground;
        }
        else
        {
            Debug.LogError("Le composant Image n'est pas trouv� dans le pr�fabriqu� du bouton.");
        }

        // V�rifiez si le composant Button est correctement d�fini
        Button buttonComponent = newButton.GetComponent<Button>();
        if (buttonComponent != null)
        {
            // Ajoutez un �couteur de clic au bouton
            buttonComponent.onClick.AddListener(() => OnCategoryButtonClick(categoryName));
        }
        else
        {
            Debug.LogError("Le composant Button n'est pas trouv� dans le pr�fabriqu� du bouton.");
        }
    }

    void OnCategoryButtonClick(string categoryName)
    {
        Debug.Log("Cat�gorie s�lectionn�e : " + categoryName);
        // Ajoutez ici le code que vous souhaitez ex�cuter lorsque vous cliquez sur un bouton de cat�gorie
        questionLoader.LoadQuestions(categoryName);
    }
}
