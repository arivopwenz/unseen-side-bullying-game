using System;
using UnityEngine;

namespace BullyingGame.Core
{
    public enum POVType
    {
        Victim,
        Bully,
        Witness
    }

    public class POVManager : MonoBehaviour
    {
        public static POVManager Instance { get; private set; }

        public event Action<POVType> OnPOVChanged;

        public POVType CurrentPOV { get; private set; } = POVType.Victim;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetPOV(POVType newPOV)
        {
            if (CurrentPOV == newPOV) return;
            CurrentPOV = newPOV;
            OnPOVChanged?.Invoke(newPOV);
        }

        public bool IsCurrentPOV(POVType pov)
        {
            return CurrentPOV == pov;
        }
    }
}
