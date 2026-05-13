using UnityEngine;

namespace TroflineMod
{
    public class Loader
    {
        [System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
        public static void Init()
        {
            // Alte Injektionen killen
            GameObject oldObj = GameObject.Find("Trofline_Manager");
            if (oldObj != null)
            {
                UnityEngine.Object.DestroyImmediate(oldObj);
            }

            // Neuen Manager erstellen und unsere Skripte anhängen
            GameObject loadObj = new GameObject("Trofline_Manager");
            loadObj.AddComponent<MainHandling>();
            loadObj.AddComponent<VisualsEngine>();
            UnityEngine.Object.DontDestroyOnLoad(loadObj);
        }
    }
}