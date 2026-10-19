using UnityEngine;
using System.Collections.Generic;
using System;

namespace BullyingGame.Interaction
{
    public class InteractionDetector : MonoBehaviour
    {
       public event Action<IInteractable> OnInteractableChanged;
       private readonly List<IInteractable> nearbyInteractables = new List<IInteractable>();
       public IInteractable CurrentInteractable { get; private set; }
       
       private void OnTriggerEnter(Collider other)
       {
        var interactable = other.GetComponentInParent<IInteractable>();
        if (interactable != null && !nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Add(interactable);
            UpdateCurrentInteractable();
        }
       }

        private void OnTriggerExit(Collider other)
        {
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable != null && nearbyInteractables.Contains(interactable))
            {
                nearbyInteractables.Remove(interactable);
                UpdateCurrentInteractable();
            }
        }

        private void Update()
        {
            if (nearbyInteractables.Count > 0 || CurrentInteractable != null)
            {
                UpdateCurrentInteractable();
            }
        }

        private void UpdateCurrentInteractable()
        {
            IInteractable closest = null;
            float minDistance = float.MaxValue;

            for (int i = nearbyInteractables.Count - 1; i >= 0; i--)
            {
                var candidate = nearbyInteractables[i];
                if (candidate is Component comp && comp != null)
                {
                    if (candidate.CanInteract() && HasLineOfSight(comp.transform))
                    {
                        float dist = Vector3.Distance(transform.position, comp.transform.position);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            closest = candidate;
                        }
                    }
                }
                else
                {
                    nearbyInteractables.RemoveAt(i);
                }
            }

            if (CurrentInteractable != closest)
            {
                CurrentInteractable = closest;
                OnInteractableChanged?.Invoke(CurrentInteractable);
            }
        }
        private bool HasLineOfSight(Transform target)
        {
            Vector3 origin=transform.position+Vector3.up*1.25f;
            Vector3 destination=target.position+Vector3.up*1.1f;
            Vector3 direction=destination-origin;
            foreach(var hit in Physics.RaycastAll(origin,direction.normalized,direction.magnitude,~0,QueryTriggerInteraction.Ignore))
            {
                if(hit.transform==transform || hit.transform.IsChildOf(transform) || hit.transform==target || hit.transform.IsChildOf(target))continue;
                return false;
            }
            return true;
        }
    }
}
