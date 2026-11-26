using System.Collections;
using BullyingGame.Core;
using BullyingGame.Prologue;
using BullyingGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using BullyingGame.Interaction;

namespace BullyingGame.Campaign
{
    [RequireComponent(typeof(PlayableDirector))]
    public sealed class LevelTwoClassroomIntro : MonoBehaviour
    {
        [SerializeField] Transform player, teacher, radit;
        [SerializeField] RuntimeAnimatorController raditIdle;
        [SerializeField] AnimationClip raditTalking;
        [SerializeField] Behaviour[] playerControls;
        [SerializeField] Transform library;
        [SerializeField] RuntimeAnimatorController raditWalking;
        [SerializeField] TMP_FontAsset questFont;
        [SerializeField] string arrivalScene;
        [SerializeField] PlayableDirector arrivalDirector;
        CinemachineCamera shotCamera;
        PrologueDialogueShot shot;
        RectTransform hud;
        PrologueHud map;
        TMP_Text objective;
        float announced;
        PrologueDialogueCanvas dialogue;
        TMP_Text prompt;
        bool[] previouslyEnabled;
        PlayableGraph talking;

        IEnumerator Start()
        {
            var animator = radit.GetComponentInChildren<Animator>(true);
            animator.enabled = true;
            animator.applyRootMotion = false;
            animator.runtimeAnimatorController = raditIdle;
            animator.speed = 1;
            animator.Rebind();
            animator.Update(0);
            dialogue = Instantiate(Resources.Load<PrologueDialogueCanvas>("TutorialDialogueCanvas"), transform);
            var cam = new GameObject("Kamera dialog kelas");
            cam.transform.SetParent(transform, false);
            shotCamera = cam.AddComponent<CinemachineCamera>();
            shotCamera.Priority = -1000;
            hud = StoryOverlay.Rect(dialogue.transform, "HUD misi Level02", Vector2.zero, Vector2.zero);
            hud.anchorMin = Vector2.zero;
            hud.anchorMax = Vector2.one;
            hud.sizeDelta = Vector2.zero;
            map = hud.gameObject.AddComponent<PrologueHud>();
            map.InitializeMinimap(hud, player);
            objective = StoryOverlay.Text(hud, "Objective", "", Vector2.zero, new Vector2(900, 130), 46);
            objective.font = questFont;
            objective.color = new Color32(255, 220, 124, 255);
            objective.alignment = TextAlignmentOptions.Center;
            objective.raycastTarget = false;
            var marker = new GameObject("Penanda objective Level02");
            marker.transform.SetParent(transform, false);
            marker.AddComponent<GroundMarker>();
            hud.gameObject.SetActive(false);
            prompt = StoryOverlay.Text(dialogue.transform, "Petunjuk kelas", "", Vector2.zero, new Vector2(680, 60), 28);
            prompt.rectTransform.anchorMin = prompt.rectTransform.anchorMax = new Vector2(.5f, .1f);
            prompt.alignment = TextAlignmentOptions.Center;
            prompt.raycastTarget = false;
            LockControls();
            Frame(teacher);
            var director = GetComponent<PlayableDirector>();
            foreach (var output in director.playableAsset.outputs)
                director.SetGenericBinding(output.sourceObject, teacher.GetComponentInChildren<Animator>(true));
            dialogue.ShowLine("Bu Guru", "Anak-anak, hari ini kita ke perpustakaan. Pilih satu buku, lalu rangkum isinya dengan kata-kata kalian sendiri.");
            director.Play();
            yield return null;
            while (director.state == PlayState.Playing) yield return null;
            shot.Exit();
            dialogue.SetVisible(false);
            UnlockControls();
            Announce("Ajak Radit ke perpustakaan.", radit);
            while (true)
            {
                bool near = Vector3.Distance(player.position, radit.position) <= 2.8f;
                prompt.text = near ? "[E] Bicara dengan Radit" : "";
                if (near && Pressed()) break;
                yield return null;
            }
            LockControls();
            hud.gameObject.SetActive(false);
            GroundMarker.ClearTarget();
            Frame(radit);
            Vector3 facing = Vector3.ProjectOnPlane(player.position - radit.position, Vector3.up);
            if (facing.sqrMagnitude > .001f) radit.rotation = Quaternion.LookRotation(facing);
            prompt.text = "[E] Lanjut";
            dialogue.ShowLine("Aji", "Dit, kita ke perpustakaan bareng, yuk?");
            do { yield return null; } while (!Pressed());
            if (raditTalking != null)
            {
                talking = PlayableGraph.Create("Radit berbicara");
                var clip = AnimationClipPlayable.Create(talking, raditTalking);
                AnimationPlayableOutput.Create(talking, "Radit", animator).SetSourcePlayable(clip);
                talking.Play();
            }
            dialogue.ShowLine("Radit", "Yuk, Ji. Aku mau cari buku buat tugas Bu Guru.");
            do { yield return null; } while (!Pressed());
            if (talking.IsValid()) talking.Destroy();
            animator.Rebind();
            shot.Exit();
            dialogue.SetVisible(false);
            prompt.text = "";
            UnlockControls();
            Announce("Temani Radit menuju perpustakaan.", library);
            yield return Escort(animator);
            while (Vector3.Distance(player.position, library.position) > 2.8f) yield return null;
            hud.gameObject.SetActive(false);
            GroundMarker.ClearTarget();
            if (arrivalDirector != null && arrivalDirector.playableAsset != null)
            {
                LockControls();
                arrivalDirector.time = 0;
                arrivalDirector.Play();
                yield return null;
                while (arrivalDirector.state == PlayState.Playing) yield return null;
                UnlockControls();
            }
            else if (Application.CanStreamedLevelBeLoaded(arrivalScene))
                yield return SceneManager.LoadSceneAsync(arrivalScene);
            else Debug.LogError("Cutscene perpustakaan belum terpasang: " + arrivalScene, this);
        }

        void Frame(Transform actor)
        {
            shot = actor.GetComponent<PrologueDialogueShot>();
            if (shot == null) shot = actor.gameObject.AddComponent<PrologueDialogueShot>();
            shot.EnterLevelShot(player, shotCamera);
        }

        void Announce(string message, Transform destination)
        {
            hud.gameObject.SetActive(true);
            objective.text = message;
            announced = Time.time;
            map.SetMinimapTarget(destination);
            GroundMarker.SetTarget(destination.position);
            LateUpdate();
        }

        void LateUpdate()
        {
            if (hud == null || !hud.gameObject.activeSelf || objective == null) return;
            float t = Mathf.SmoothStep(0, 1, (Time.time - announced - 10f) / .55f);
            var rect = objective.rectTransform;
            rect.anchorMin = rect.anchorMax = Vector2.Lerp(new Vector2(.5f, .53f), Vector2.one, t);
            rect.pivot = Vector2.Lerp(Vector2.one * .5f, Vector2.one, t);
            rect.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(-24, -194), t);
            rect.sizeDelta = new Vector2(Mathf.Lerp(Mathf.Min(1080, hud.rect.width * .88f), 370, t), 130);
            objective.fontSize = Mathf.Lerp(46, 30, t);
        }

        IEnumerator Escort(Animator animator)
        {
            var agent = radit.GetComponent<NavMeshAgent>();
            if (agent == null) agent = radit.gameObject.AddComponent<NavMeshAgent>();
            agent.radius = .25f;
            agent.height = 1.5f;
            agent.speed = 2.6f;
            agent.angularSpeed = 400f;
            agent.stoppingDistance = .2f;
            var path = new NavMeshPath();
            while (!NavMesh.SamplePosition(radit.position, out var from, 3, NavMesh.AllAreas) ||
                   !NavMesh.SamplePosition(library.position, out var to, 2, NavMesh.AllAreas) ||
                   !NavMesh.CalculatePath(from.position, to.position, NavMesh.AllAreas, path) ||
                   path.status != NavMeshPathStatus.PathComplete)
            {
                prompt.text = "Jalur Radit ke perpustakaan belum tersedia.";
                yield return new WaitForSeconds(1);
            }
            prompt.text = "";
            animator.runtimeAnimatorController = raditWalking;
            animator.SetBool("Grounded", true);
            agent.Warp(path.corners[0]);
            agent.SetPath(path);
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                foreach (var door in SwingDoor.ActiveDoors)
                    if ((Vector3.ProjectOnPlane(door.DoorwayPosition - radit.position, Vector3.up)).sqrMagnitude < 4f)
                        door.Open();
                agent.isStopped = Vector3.Distance(player.position, radit.position) > 8f && !agent.isOnOffMeshLink;
                animator.SetFloat("Speed", agent.velocity.sqrMagnitude > .01f ? 3f : 0);
                yield return null;
            }
            agent.ResetPath();
            animator.runtimeAnimatorController = raditIdle;
        }

        static bool Pressed() => Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        void LockControls()
        {
            previouslyEnabled = new bool[playerControls.Length];
            for (int i = 0; i < playerControls.Length; i++)
                if (playerControls[i] != null)
                {
                    previouslyEnabled[i] = playerControls[i].enabled;
                    playerControls[i].enabled = false;
                }
        }

        void UnlockControls()
        {
            if (previouslyEnabled == null) return;
            for (int i = 0; i < playerControls.Length; i++)
                if (playerControls[i] != null) playerControls[i].enabled = previouslyEnabled[i];
            previouslyEnabled = null;
        }

        void OnDisable()
        {
            StopAllCoroutines();
            if (shot != null) shot.Exit();
            if (hud != null) GroundMarker.ClearTarget();
            if (talking.IsValid()) talking.Destroy();
            if (dialogue != null) dialogue.gameObject.SetActive(false);
            UnlockControls();
        }
    }
}
