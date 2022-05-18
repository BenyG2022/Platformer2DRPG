using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blum.MonoBehaviours.Helpers
{

    public static class PushBack
    {
        public static void getPusedhBack(this Rigidbody2D pushedBack, Vector2 pusherPosition, float pushForceMultiplier)
        {
            pushedBack.AddForce((pushedBack.position - pusherPosition).normalized * pushForceMultiplier, ForceMode2D.Force);
        }

    }

}