using System.Collections;
using UnityEngine;

namespace SummonerCreator
{
    public class SCBinder : MonoBehaviour
    {
        private static SCBinder instance;

        public static void UnitGlad()
        {
            if (!instance)
            {
                // Create a "manager" object that doesn't interfere with the rest of the game objects.
                instance = new GameObject
                {
                    hideFlags = HideFlags.HideAndDontSave
                }.AddComponent<SCBinder>();
            }
            instance.StartCoroutine(StartUnitgradLate());
        }

        // A coroutine that waits for the service locator to be initialized before starting the mod.
        private static IEnumerator StartUnitgradLate()
        {
            // Pause this coroutine until an instance of ServiceLocator is found in the scene. - ChatGPT
            yield return new WaitUntil(() => FindObjectOfType<ServiceLocator>() != null);

            // Wait until the ISaveLoaderService is available from the ServiceLocator. - ChatGPT
            yield return new WaitUntil(() => ServiceLocator.GetService<ISaveLoaderService>() != null);

            // Adds a small delay to ensure everything is properly initialized before the mod starts. - ChatGPT
            yield return new WaitForSeconds(0.2f);
            new SCMain();
            yield break;
        }

    }
}