using UnityEngine;

public static class APIConfig
{
    // Variable statique pour stocker l'URL de l'API
    public static string apiURL;

    // Méthode appelée une seule fois lors du démarrage de l'application
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        // Initialisez l'URL de l'API avec votre URL réelle ici
        apiURL = "http://localhost:3000/api/";
    }
}
