using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Blum.ScriptableObjects.Stats;
using Blum.MonoBehaviours.Helpers;
namespace Blum.MonoBehaviours.Enemies
{

    public class Enemy : GameParticiperBase
    {

        [Header("Patrolling")]
        [SerializeField] private List<Transform> _patrollingPositions = new List<Transform>();
        private int _indexOfLastVisitedPatrollingPositions = 0;
        private bool _canPatroll = true;
        protected void stopPatrollTillBoolChanges(ref bool toggle)
        {

        }
        private void dontPatrollJustStayStopped()
        {
            _canPatroll = false;
        }
        protected void stopFromPatrollingForSeconds(float secondsDurationOfStopped)
        {
            pausePatroll(secondsDurationOfStopped);
        }
        private IEnumerator pausePatroll(float delay)
        {
            _canPatroll = false;
            yield return new WaitForSeconds(delay);
            _canPatroll = true;
        }
        protected void getBackToPatrolling()
        {
            _canPatroll = true;
        }

        protected bool gotPatrollingPoints() => _patrollingPositions.Count > 1;


        protected override void respawnGameParticiper()
        {
            Debug.Log("Respawned Enemy" + gameObject.name);
            _indexOfLastVisitedPatrollingPositions = 0;
            this.gameObject.layer = LayerMask.NameToLayer("Militant");
            if (_patrollingPositions.Count > 1)
            {
                faceWalkSide();
            }
            base.respawnGameParticiper();
        }
        protected virtual void Update()
        {
            if (_patrollingPositions.Count > 1 && _canPatroll == true)
            {
                ///_rbody.velocity = Vector3.MoveTowards(_rbody.position, _patrollingPositions[_indexOfLastVisitedPatrollingPositions].position, 50.0f * Time.deltaTime);
                _thisTransform.Translate(_thisTransform.TransformDirection((_patrollingPositions[_indexOfLastVisitedPatrollingPositions].position - _thisTransform.position).normalized) * Time.deltaTime);
                if ((_patrollingPositions[_indexOfLastVisitedPatrollingPositions].position - _thisTransform.position).magnitude < 1.0f)
                {
                    _indexOfLastVisitedPatrollingPositions++;
                    if (_indexOfLastVisitedPatrollingPositions >= _patrollingPositions.Count)
                    {
                        _indexOfLastVisitedPatrollingPositions = 0;
                    }
                    faceWalkSide();
                }
            }
        }
        private void faceWalkSide() => this._thisTransform.rotation = Quaternion.Euler(0.0f, ((_thisTransform.position - _patrollingPositions[_indexOfLastVisitedPatrollingPositions].position).x > 0.0f ? 180.0f : 0.0f), 0.0f);
    }

}