using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Blum.ScriptableObjects.Stats;
using Blum.MonoBehaviours.Helpers;
using System.Collections.Generic;
using System.Collections;
/// <summary>
/// Hey developer!
/// If you have any questions, come chat with me on my Discord: https://discord.gg/GqeHHnhHpz
/// If you enjoy the controller, make sure you give the video a thumbs up: https://youtu.be/rJECT58CQHs
/// Have fun!
///
/// Love,
/// Tarodev
/// </summary>
public class PlayerController : GameParticiperBase
{
    [Header("PlayerController")]
    private FrameInputs _inputs;

    private void Update()
    {
        if (isDead == false)
        {

            GatherInputs();

            HandleGrounding();

            HandleWalking();

            HandleJumping();

            HandleSwordAttack();

            HandleDashing();

        }
    }



    #region inherited


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
        if (_currentAnimationClip == "HeroBeforeAfterJump" || _currentAnimationClip == "HitSparkle" || _currentAnimationClip == "HeroSwordAttack")
        {
            if (_currentAnimationClip == "HeroSwordAttack" && newAnimationState == "HitSparkle")
            {
                StartCoroutine(coroutine_waitAnimationEndAndStartNextOne(newAnimationState));
            }
            else if (_anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.96f)
            {
                return;
            }
        }
        base.changeAnimationState(newAnimationState);
    }
    protected override void respawnGameParticiper()
    {
        Debug.Log("respawned player");
        this.gameObject.layer = LayerMask.NameToLayer("Player");
        _walkSpeed = _characterStats_SO.walkSpeed;
        base.respawnGameParticiper();

    }
    public override void receiveDamage(ImpactDamageStats damageReceivedStats, Vector3 rgdbdyPosition)
    {
        base.receiveDamage(damageReceivedStats, rgdbdyPosition);
        changeAnimationState("HitSparkle");
    }

    protected override void killGameParticiper()
    {
        Debug.Log("Player dies");
        StartCoroutine(coroutine_DiePlayer());
        base.killGameParticiper();
    }
    private IEnumerator coroutine_DiePlayer()
    {
        if (_currentAnimationClip != "HeroDeath")
        {
            while (_anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.96f)
            {
                yield return new WaitForSeconds(0.08f);
            }
        }
        changeAnimationState("HeroDeath");
    }

    #endregion

    #region Inputs

    private bool _facingLeft;

    private void GatherInputs()
    {
        _inputs.RawX = (int)Input.GetAxisRaw("Horizontal");
        _inputs.RawY = (int)Input.GetAxisRaw("Vertical");
        _inputs.X = Input.GetAxis("Horizontal");
        _inputs.Y = Input.GetAxis("Vertical");
        if (_inputs.RawX == 0.0f && IsGrounded == true)
        {
            changeAnimationState("HeroIdle");
        }


        _facingLeft = _inputs.RawX != 1 && (_inputs.RawX == -1 || _facingLeft);
        SetFacingDirection(_facingLeft); // Don't turn while grabbing the wall
    }

    private void SetFacingDirection(bool left)
    {
        _anim.transform.rotation = left ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);
    }

    #endregion

    #region Detection

    [Header("Detection")]
    [SerializeField] private float _grounderOffset = -1, _grounderRadius = 0.2f;
    public bool IsGrounded;
    public static event Action OnTouchedGround;



    private void HandleGrounding()
    {
        // Grounder
        var grounded = Physics2D.OverlapCircleNonAlloc(transform.position + new Vector3(0, _grounderOffset), _grounderRadius, _ground, _groundMask) > 0;

        if (!IsGrounded && grounded)
        {
            IsGrounded = true;
            _hasDashed = false;
            _hasJumped = false;
            _currentMovementLerpSpeed = 100;
            //PlayRandomClip(_landClips);
            changeAnimationState("HeroBeforeAfterJump");
            OnTouchedGround?.Invoke();
            transform.SetParent(_ground[0].transform);

        }
        else if (IsGrounded && !grounded)
        {
            IsGrounded = false;
            _timeLeftGrounded = Time.time;
            transform.SetParent(null);
        }
        else if (IsGrounded == false && grounded == false)
        {
            if (_rbody.velocity.y < 0.0f)
            {
                changeAnimationState("HeroJumpDown");
            }
            else if (_rbody.velocity.y > 0.0f)
            {
                changeAnimationState("HeroJumpUp");

            }

        }

        // Wall detection
    }

    private void DrawGrounderGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, _grounderOffset), _grounderRadius);
    }

    protected virtual new void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        DrawGrounderGizmos();
    }

    #endregion

    #region Walking

    [Header("Walking")] [SerializeField] private float _walkSpeed = 4;
    [SerializeField] private float _acceleration = 2;
    [SerializeField] private float _currentMovementLerpSpeed = 100;

    private void HandleWalking()
    {
        // Slowly release control after wall jump

        if (_dashing) return;
        // This can be done using just X & Y input as they lerp to max values, but this gives greater control over velocity acceleration
        var acceleration = IsGrounded ? _acceleration : _acceleration * 0.5f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (_rbody.velocity.x > 0) _inputs.X = 0; // Immediate stop and turn. Just feels better
            _inputs.X = Mathf.MoveTowards(_inputs.X, -1, acceleration * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (_rbody.velocity.x < 0) _inputs.X = 0;
            _inputs.X = Mathf.MoveTowards(_inputs.X, 1, acceleration * Time.deltaTime);
        }
        else
        {
            _inputs.X = Mathf.MoveTowards(_inputs.X, 0, acceleration * 2 * Time.deltaTime);
        }

        var idealVel = new Vector3(_inputs.X * _walkSpeed, _rbody.velocity.y);
        // _currentMovementLerpSpeed should be set to something crazy high to be effectively instant. But slowed down after a wall jump and slowly released
        _rbody.velocity = Vector3.MoveTowards(_rbody.velocity, idealVel, _currentMovementLerpSpeed * Time.deltaTime);

        if (_inputs.RawX != 0 && IsGrounded)
        {
            changeAnimationState("HeroRun");
        }
    }

    #endregion

    #region Jumping

    [Header("Jumping")] [SerializeField] private float _jumpForce = 15;
    [SerializeField] private float _fallMultiplier = 7;
    [SerializeField] private float _jumpVelocityFalloff = 8;
    [SerializeField] private ParticleSystem _jumpParticles;
    [SerializeField] private Transform _jumpLaunchPoof;
    [SerializeField] private float _coyoteTime = 0.2f;
    private float _timeLeftGrounded = -10;
    private bool _hasJumped;

    private void HandleJumping()
    {
        if (_dashing) return;
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (IsGrounded || Time.time < _timeLeftGrounded + _coyoteTime)
            {
                if (!_hasJumped) ExecuteJump(new Vector2(_rbody.velocity.x, _jumpForce)); // Ground jump
            }
        }

        void ExecuteJump(Vector3 dir)
        {
            _rbody.velocity = dir;
            //_jumpLaunchPoof.up = _rb.velocity;
            //_jumpParticles.Play();
            //Debug.Log("execute Jump");
            changeAnimationState("HeroBeforeAfterJump");
            _hasJumped = true;
        }

        // Fall faster and allow small jumps. _jumpVelocityFalloff is the point at which we start adding extra gravity. Using 0 causes floating
        if (_rbody.velocity.y < _jumpVelocityFalloff || _rbody.velocity.y > 0 && !Input.GetKey(KeyCode.C))
            _rbody.velocity += _fallMultiplier * Physics.gravity.y * Vector2.up * Time.deltaTime;
    }

    #endregion

    #region Dash

    [Header("Dash")] [SerializeField] private float _dashSpeed = 15;
    [SerializeField] private float _dashLength = 1;
    [SerializeField] private ParticleSystem _dashParticles;
    [SerializeField] private Transform _dashRing;
    [SerializeField] private ParticleSystem _dashVisual;

    public static event Action OnStartDashing, OnStopDashing;

    private bool _hasDashed;
    private bool _dashing;
    private float _timeStartedDash;
    private Vector3 _dashDir;

    private void HandleDashing()
    {
        if (Input.GetKeyDown(KeyCode.X) && !_hasDashed)
        {
            _dashDir = new Vector3(_inputs.RawX, _inputs.RawY).normalized;
            if (_dashDir == Vector3.zero) _dashDir = _facingLeft ? Vector3.left : Vector3.right;
            //_dashRing.up = _dashDir;
            //_dashParticles.Play();
            _dashing = true;
            _hasDashed = true;
            _timeStartedDash = Time.time;
            // _rbody.simulated = false;
            //_dashVisual.Play();
            //PlayRandomClip(_dashClips);
            OnStartDashing?.Invoke();
        }

        if (_dashing)
        {
            _rbody.velocity = _dashDir * _dashSpeed;

            if (Time.time >= _timeStartedDash + _dashLength)
            {
                //_dashParticles.Stop();
                _dashing = false;
                // Clamp the velocity so they don't keep shooting off
                _rbody.velocity = new Vector3(_rbody.velocity.x, _rbody.velocity.y > 3 ? 3 : _rbody.velocity.y);
                //_rbody.simulated = true;
                if (IsGrounded) _hasDashed = false;
                //_dashVisual.Stop();
                OnStopDashing?.Invoke();
            }
        }
    }

    #endregion

    #region Impacts

    [Header("Collisions")]
    [SerializeField]
    private ParticleSystem _impactParticles;

    [SerializeField] private GameObject _deathExplosion;
    [SerializeField] private float _minImpactForce = 2;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > _minImpactForce && IsGrounded) _impactParticles.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Death"))
        {
            Instantiate(_deathExplosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        _hasDashed = false;
    }

    #endregion

    #region Audio

    [Header("Audio")] [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip[] _landClips;
    [SerializeField] private AudioClip[] _dashClips;

    private void PlayRandomClip(AudioClip[] clips)
    {
        //_source.PlayOneShot(clips[Random.Range(0, clips.Length)], 0.2f);
    }

    #endregion

    #region SwordFight

    /// <summary>
    /// this trigger with animation event
    /// </summary>
    private void animationTriggersSwordAttack()
    {

        var oponnent = checkWhoInAttackRange(Vector3.right * (_facingLeft == true ? -1.0f : 1.0f), _attackCentreOffset, _attackLayerMask, _collsAttacking);
        if (oponnent != null)
        {
            makeAttack(oponnent);
        }

    }
    private void HandleSwordAttack()
    {
        if (Input.GetKeyDown(KeyCode.V) == true && _currentAnimationClip != "HitSparkle")
        {
            changeAnimationState("HeroSwordAttack");
        }
    }


    #endregion

    private struct FrameInputs
    {
        public float X, Y;
        public int RawX, RawY;
    }
}