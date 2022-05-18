using Blum.ScriptableObjects.Stats;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blum.MonoBehaviours.Enemies
{

    public class Mushroom : Enemy
    {
        private bool _canCrush = true;
        private bool _canGetHit = true;
        public override void receiveDamage(ImpactDamageStats damageReceivedStats, Vector3 rgdbdyPosition)
        {
            base.receiveDamage(damageReceivedStats, rgdbdyPosition);
            _canGetHit = false;
            changeAnimationState("MushroomHited");
        }
        protected override void killGameParticiper()
        {
            Debug.Log("Mushroom Dies");
            StartCoroutine(coroutine_waitAnimationEndAndStartNextOne("MushroomDeath"));
            base.killGameParticiper();
        }

        private IEnumerator coroutine_waitAnimationEndAndStartNextOne(string newAnimationState)
        {
            if (_currentAnimationClip != newAnimationState)
            {
                while (_anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.96)
                {
                    yield return new WaitForSeconds(0.08f);
                }
            }
            base.changeAnimationState(newAnimationState);
        }

        protected override void changeAnimationState(string newAnimationState)
        {
            if (_currentAnimationClip == "MushroomHited")
            {
                if (_anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.96f)
                {
                    base.changeAnimationState(newAnimationState);
                }
            }
            else if (_currentAnimationClip == "MushroomCrushed")
            {
                if (newAnimationState == "MushroomHited")
                {
                    StartCoroutine(coroutine_waitAnimationEndAndStartNextOne(newAnimationState));
                }
                else
                {
                    base.changeAnimationState(newAnimationState);
                }
            }
            else
            {
                base.changeAnimationState(newAnimationState);
            }
        }
        protected override void Update()
        {
            if (isDead == false)
            {

                base.Update();
                if (_canCrush == true && _canGetHit == true)
                {
                    var opponent = checkWhoInAttackRange(Vector3.zero, _attackCentreOffset, _attackLayerMask, _collsAttacking);
                    if (opponent != null)
                    {
                        _canCrush = false;
                        _canGetHit = false;
                        changeAnimationState("MushroomCrushed");
                        makeAttack(opponent);

                    }
                    else if (_canGetHit == true)
                    {
                        if (gotPatrollingPoints() == true)
                        {
                            changeAnimationState("MushroomWalk");
                        }
                        else
                        {
                            changeAnimationState("MushroomIdle");
                        }
                    }
                }
                else
                {
                    if (_currentAnimationClip == "MushroomCrushed" || _currentAnimationClip == "MushroomHited")
                    {
                        if (_anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
                        {
                            _canCrush = true;
                            _canGetHit = true;
                        }
                    }
                }
            }
        }
        private void canCrushAgain() => _canCrush = true;
        private void canHitedAgain() => _canGetHit = true;
    }

}