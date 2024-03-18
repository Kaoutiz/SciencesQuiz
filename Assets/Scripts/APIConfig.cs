using UnityEngine;

public static class APIConfig
{
    // Variable statique pour stocker l'URL de l'API
    public static string apiURL;

    // M�thode appel�e une seule fois lors du d�marrage de l'application
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        // Initialisez l'URL de l'API avec votre URL r�elle ici
        apiURL = "http://localhost:3000/api/";
    }
}
