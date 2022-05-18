using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Blum.ScriptableObjects.Stats;


namespace Blum.ScriptableObjects.Stats
{
    [CreateAssetMenu()]
    public class MeleeStats : ImpactDamageStats
    {
        public float attackRange = 2.0f;
        public float attackReuseDelay = 0.5f;
    }

}