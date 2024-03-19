using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CategoryLoad : MonoBehaviour
{
    public string apiUrl = APIConfig.apiURL + "categories";
    public QuestionLoader questionLoader; // Référence au script QuestionLoader
    public GameObject buttonPrefab; // Prefab de bouton à instancier
    public Transform buttonContainer; // Parent des boutons dans la hiérarchie
    public Sprite[] backgroundImages; // Tableau d'images de fond aléatoires

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
        Debug.Log("Récupération des catégories depuis l'API...");

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erreur lors de la récupération des catégories : " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            CategoriesResponse response = JsonUtility.FromJson<CategoriesResponse>(jsonResponse);

            Debug.Log("Message de la réponse : " + response.message);

            Debug.Log("Catégories récupérées avec succès :");
            Debug.Log(jsonResponse);

            foreach (CategoryData categoryData in response.categories_list)
            {
                Debug.Log("Catégorie : " + categoryData.categorie);
                CreateCategoryButton(categoryData.categorie);
            }
        }
    }

    void CreateCategoryButton(string categoryName)
    {
        Debug.Log("Création du bouton pour la catégorie : " + categoryName);

        GameObject newButton = Instantiate(buttonPrefab, buttonContainer.transform); // Assurez-vous que le conteneur de boutons est le parent du nouveau bouton

        // Assurez-vous que le texte est correctement défini sur le bouton
        Text buttonText = newButton.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = categoryName;
        }
        else
        {
            Debug.LogError("Le composant Text n'est pas trouvé dans le préfabriqué du bouton.");
            return;
        }

        // Choix aléatoire de l'image de fond
        Sprite randomBackground = backgroundImages[Random.Range(0, backgroundImages.Length)];
        Image buttonImage = newButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = randomBackground;
        }
        else
        {
            Debug.LogError("Le composant Image n'est pas trouvé dans le préfabriqué du bouton.");
        }

        // Vérifiez si le composant Button est correctement défini
        Button buttonComponent = newButton.GetComponent<Button>();
        if (buttonComponent != null)
        {
            // Ajoutez un écouteur de clic au bouton
            buttonComponent.onClick.AddListener(() => OnCategoryButtonClick(categoryName));
        }
        else
        {
            Debug.LogError("Le composant Button n'est pas trouvé dans le préfabriqué du bouton.");
        }
    }

    void OnCategoryButtonClick(string categoryName)
    {
        Debug.Log("Catégorie sélectionnée : " + categoryName);
        // Ajoutez ici le code que vous souhaitez exécuter lorsque vous cliquez sur un bouton de catégorie
        questionLoader.LoadQuestions(categoryName);
    }
}
