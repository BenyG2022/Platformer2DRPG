using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blum.ScriptableObjects.Stats
{
    [CreateAssetMenu()]
    public class GameParticiperStats : ScriptableObject
    {
        public int health = 10;
        
        public float walkSpeed = 4.0f;
        public float acceleration = 2.0f;
    }

}