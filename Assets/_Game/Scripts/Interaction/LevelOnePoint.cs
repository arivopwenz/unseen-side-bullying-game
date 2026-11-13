using UnityEngine;
using BullyingGame.Interaction;

namespace BullyingGame.LevelOne
{
    [RequireComponent(typeof(SphereCollider))]
    public class LevelOnePoint : MonoBehaviour, IInteractable
    {
        [Header("Move this GameObject to move its interaction")]
        [SerializeField] private string pointId, label;
        [SerializeField] private LevelOneDirector director;
        [SerializeField, Min(1)] private float radius = 3;
        [SerializeField] private Color gizmoColor = new Color(.2f, .85f, .75f);
        [SerializeField] private GameObject markerVisual;
        public string Id => pointId;
        public string Label => label;
        public float Radius => radius;
        public LevelOneAction Action => director != null ? director.ActionFor(this) : LevelOneAction.Observe;
        public string GetPromptText() => LevelOneDirector.ActionName(Action) + " • " + label;
        public bool CanInteract() => isActiveAndEnabled && director != null && director.CanUse(this);
        public void Interact(GameObject interactor)
        {
            if (director != null && interactor == director.Player && director.IsReachable(this)) director.Use(this);
        }
        void Start()
        {
            var editorLabel = transform.Find("Label penanda");
            if (editorLabel != null) editorLabel.gameObject.SetActive(false);
        }
        void Update()
        {
            if (markerVisual != null) markerVisual.SetActive(director != null && director.IsRelevant(this));
        }
        void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor; Gizmos.DrawWireSphere(transform.position + Vector3.up * .2f, .4f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2);
        }
        void OnDrawGizmosSelected() { Gizmos.color = gizmoColor; Gizmos.DrawWireSphere(transform.position, radius); }
    }
}
