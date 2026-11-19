using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using BullyingGame.LevelOne;

namespace BullyingGame.Campaign
{
    public sealed class CampaignCinematic : MonoBehaviour
    {
        public IEnumerator Play(string path, string[] lines, LevelOneView view, Action<bool> finished)
        {
            if (!Application.CanStreamedLevelBeLoaded(path))
            {
                Debug.LogError("Cutscene belum tersedia dalam Build Settings: " + path);
                finished(false);
                yield break;
            }
            var hub = gameObject.scene;
            var roots = hub.GetRootGameObjects();
            var renderers = roots.SelectMany(root => root.GetComponentsInChildren<Renderer>())
                .Where(renderer => !renderer.forceRenderingOff).ToArray();
            var behaviours = roots.SelectMany(root => root.GetComponentsInChildren<Behaviour>())
                .Where(component => component.enabled && (component is UnityEngine.Camera || component is AudioListener ||
                    component is Light || component is UnityEngine.Rendering.Volume ||
                    component is ReflectionProbe || component is Unity.Cinemachine.CinemachineBrain)).ToArray();
            foreach (var renderer in renderers) renderer.forceRenderingOff = true;
            foreach (var behaviour in behaviours) behaviour.enabled = false;
            AsyncOperation loading = null;
            try { loading = SceneManager.LoadSceneAsync(path, LoadSceneMode.Additive); }
            catch (Exception error) { Debug.LogException(error); }
            if (loading != null) yield return loading;
            var scene = SceneManager.GetSceneByPath(path);
            var timeline = scene.IsValid() && scene.isLoaded ? scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PlayableDirector>()).FirstOrDefault() : null;
            bool success = timeline != null && timeline.playableAsset != null &&
                timeline.playableAsset.outputs.All(output => timeline.GetGenericBinding(output.sourceObject) != null);
            if (success)
            {
                SceneManager.SetActiveScene(scene);
                foreach (var graphics in scene.GetRootGameObjects().Select(root => root.GetComponent<AreaSekolahGraphicsController>()))
                    if (graphics != null) graphics.ApplyGraphicsMode();
                timeline.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
                timeline.extrapolationMode = DirectorWrapMode.Hold;
                timeline.Play();
                yield return new WaitForSecondsRealtime(.3f);
                bool done = false;
                view.ShowCinematicLines(lines, () => done = true);
                while (!done) yield return null;
                timeline.Stop();
            }
            if (hub.IsValid() && hub.isLoaded) SceneManager.SetActiveScene(hub);
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            foreach (var renderer in renderers) if (renderer != null) renderer.forceRenderingOff = false;
            foreach (var behaviour in behaviours) if (behaviour != null) behaviour.enabled = true;
            foreach (var graphics in roots.Select(root => root.GetComponent<AreaSekolahGraphicsController>()))
                if (graphics != null && graphics.isActiveAndEnabled) graphics.ApplyGraphicsMode();
            finished(success);
        }
    }
}
