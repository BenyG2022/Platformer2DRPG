using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Blum.ScriptableObjects.Stats;

namespace Blum.MonoBehaviours.Helpers
{

    public abstract class GameParticiperBase : MonoBehaviour
    {
        [SerializeField] protected LayerMask _groundMask;
        protected readonly Collider2D[] _ground = new Collider2D[1];
        protected readonly Collider2D[] _leftWall = new Collider2D[1];
        protected readonly Collider2D[] _rightWall = new Collider2D[1];

        protected Transform _thisTransform = null;
        protected readonly Collider2D[] _collsAttacking = new Collider2D[1];
        protected string _currentAnimationClip = "";
        public bool isDead { private set; get; } = true;

        [Header("GameParticiper")]
        [SerializeField] protected Animator _anim;
        [SerializeField] protected Vector3 _attackCentreOffset = default;
        [SerializeField] protected Rigidbody2D _rbody = null;
        [SerializeField] private MeleeStats _myWeaponAttackingStats_SO = null;
        [SerializeField] protected GameParticiperStats _characterStats_SO = null;
        [SerializeField] private Transform _respawnPosition = default;
        [SerializeField] protected LayerMask _attackLayerMask = default;
        [SerializeField] private int _healthAmount = 3;

        protected virtual void changeAnimationState(string newAnimationState)
        {
            if (_currentAnimationClip == newAnimationState) return;
            _anim.Play(newAnimationState);
            _currentAnimationClip = newAnimationState;
        }
        public virtual void receiveDamage(ImpactDamageStats damageReceivedStats, Vector3 pusherPosition)
        {
            _rbody.getPusedhBack(pusherPosition, damageReceivedStats.pushBackMultiplier);
            _healthAmount -= damageReceivedStats.damage;
            
        }

        private void animationTriggersGetHit()
        {
            Debug.Log("AnimationTriggers-gettingHit: " + this.gameObject.name);
            if (_healthAmount <= 0)
            {
                killGameParticiper();
            }
        }

        private void animationTriggersDeath()
        {
            Debug.Log(gameObject.name + " died");
            this.gameObject.SetActive(false);
        }

        [ContextMenu("Kill")]
        private void kill() => killGameParticiper();
        protected virtual void killGameParticiper()
        {
            this.gameObject.layer = LayerMask.NameToLayer("Dead");
            this._rbody.simulated = false;
            isDead = true;

        }
        [ContextMenu("Respawn")]
        private void respawn() => respawnGameParticiper();
        protected virtual void respawnGameParticiper()
        {
            _healthAmount = _characterStats_SO.health;
            this._thisTransform.position = _respawnPosition.position;
            this.gameObject.SetActive(true);
            isDead = false;
            this._rbody.simulated = true;
        }
        protected GameParticiperBase checkWhoInAttackRange(Vector3 dirAttack, Vector3 centreOfOverlap, LayerMask mask, Collider2D[] colls)
        {
            if (Physics2D.OverlapBoxNonAlloc(_thisTransform.position + (dirAttack + centreOfOverlap) * _myWeaponAttackingStats_SO.attackRange * 0.5f, new Vector2(_myWeaponAttackingStats_SO.attackRange, 0.64f), 0.0f, colls, mask) > 0)
            {
                if (colls[0].TryGetComponent(out GameParticiperBase opponnent) == true)
                {

                    return opponnent;
                }
            }
            return null;
        }
        protected void makeAttack(GameParticiperBase oponnent)
        {
            oponnent.receiveDamage(_myWeaponAttackingStats_SO, this._thisTransform.position);

        }
        private void Awake()
        {
            _thisTransform = this.transform;
            if (this is PlayerController == true)
            {
                (this as PlayerController).respawnGameParticiper();
            }
            else if (this is Enemies.Enemy == true)
            {
                (this as Enemies.Enemy).respawnGameParticiper();
            }
        }
        protected virtual void OnDrawGizmos()
        {
            drawSwordAttackArea();
        }
        private void drawSwordAttackArea()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(this.transform.position + ((this is PlayerController == true ? Vector3.right : Vector3.zero) + _attackCentreOffset) * (this.transform.rotation.eulerAngles.y != 0.0f ? -1.0f : 1.0f) * _myWeaponAttackingStats_SO.attackRange * 0.5f, new Vector3(_myWeaponAttackingStats_SO.attackRange, 0.64f, 0.0f));
        }
    }

}