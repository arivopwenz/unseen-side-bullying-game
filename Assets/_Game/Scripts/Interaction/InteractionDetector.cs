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
            if (nearbyInteractables.Count > 0)
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
                    if (candidate.CanInteract())
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
    }
}