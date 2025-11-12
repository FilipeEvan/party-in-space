using UnityEngine;

// Attach to Rover_2. When the player enters a long-range detection
// zone that is greater than the enemies' detection range, the rover
// charges in a straight line following its own orientation (local axis),
// capturing the direction at the moment of detection (no homing).
public class RoverStraightChase : MonoBehaviour
{
    [Header("Targeting")]
    public bool autoFindTarget = true;
    public Transform target; // auto-resolved from PlayerHealth/PlayerMovement

    [Header("Ranges")]
    [Tooltip("Rover detects the player within this distance.")]
    public float roverDetectRange = 28f;
    [Tooltip("Enemy detection to compare against. If <= 0, tries to read from EnemyLargeChase in the scene, otherwise uses this value.")]
    public float enemyDetectRangeOverride = -1f; // e.g., 15f like EnemyLargeChase

    [Header("Movement")]
    public float speed = 12f;
    [Tooltip("Lock Y while moving so the rover stays on its original height.")]
    public bool lockInitialY = true;
    public float rotationSpeed = 20f; // not used unless alignVisualToDirection is enabled

    [Header("Orientation")]
    public Transform forwardReference; // optional: use this transform's local axis
    public enum LocalAxis { Z, X }
    public enum ChargeDirectionMode { LocalAxis, WorldX, WorldZ }
    public ChargeDirectionMode directionMode = ChargeDirectionMode.LocalAxis;
    public LocalAxis axis = LocalAxis.Z; // when using LocalAxis mode
    public bool invertDirection = false; // flips the chosen direction
    public bool alignVisualToDirection = false; // rotate object to face charge direction on start

    [Header("Charge")]
    [Tooltip("Duration of the straight charge before stopping. Set <= 0 for infinite.")]
    public float chargeDuration = 2.5f;
    public bool repeatCharges = true; // if false, only charges once per entry

    [Header("Animation (optional)")]
    public Animator animator; // optional
    public string runBoolParam = "Run"; // set true while charging
    public string speedFloatParam = "Speed"; // optional: sets current speed

    float _enemyDetectRangeCached = 15f;
    bool _charging;
    float _chargeEndTime;
    Vector3 _chargeDir;
    float _yLock;
    bool _didChargeOnce;

    void Reset()
    {
        // Defaults suitable for Rover_2
        animator = GetComponentInChildren<Animator>();
        forwardReference = null; // use own transform
        directionMode = ChargeDirectionMode.LocalAxis;
        axis = LocalAxis.Z;
        invertDirection = false;
        alignVisualToDirection = false;
        lockInitialY = true;
        speed = 12f;
        roverDetectRange = 28f;
        chargeDuration = 2.5f;
        repeatCharges = true;
    }

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        _yLock = transform.position.y;
        CacheEnemyDetectRange();
    }

    void CacheEnemyDetectRange()
    {
        if (enemyDetectRangeOverride > 0f)
        {
            _enemyDetectRangeCached = enemyDetectRangeOverride;
            return;
        }
        var enemy = FindObjectOfType<EnemyLargeChase>();
        if (enemy != null) _enemyDetectRangeCached = Mathf.Max(0.1f, enemy.detectionRange);
    }

    void Update()
    {
        if (autoFindTarget && target == null)
        {
            var ph = FindObjectOfType<PlayerHealth>();
            if (ph != null) target = ph.transform;
            if (target == null)
            {
                var pm = FindObjectOfType<PlayerMovement>();
                if (pm != null) target = pm.transform;
            }
        }

        if (target == null)
        {
            SetAnim(false, 0f);
            return;
        }

        if (!_charging)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            bool withinRover = dist <= roverDetectRange;
            bool beyondEnemy = dist > _enemyDetectRangeCached;
            if (withinRover && beyondEnemy && (!
                _didChargeOnce || repeatCharges))
            {
                BeginCharge();
            }
            else
            {
                SetAnim(false, 0f);
            }
        }
        else
        {
            ChargeStep();
        }
    }

    void BeginCharge()
    {
        _didChargeOnce = true;
        _charging = true;
        // Decide charge direction based on selected mode
        if (directionMode == ChargeDirectionMode.LocalAxis)
        {
            var t = forwardReference != null ? forwardReference : transform;
            Vector3 basis = axis == LocalAxis.Z ? t.forward : t.right;
            _chargeDir = new Vector3(basis.x, 0f, basis.z).normalized;
            if (_chargeDir.sqrMagnitude < 0.0001f) _chargeDir = Vector3.forward;
        }
        else if (directionMode == ChargeDirectionMode.WorldX)
        {
            _chargeDir = Vector3.right;
        }
        else // WorldZ
        {
            _chargeDir = Vector3.forward;
        }
        if (_chargeDir.sqrMagnitude < 0.0001f) _chargeDir = Vector3.forward;
        if (invertDirection) _chargeDir = -_chargeDir;

        if (alignVisualToDirection)
        {
            var targetRot = Quaternion.LookRotation(_chargeDir, Vector3.up);
            transform.rotation = targetRot;
        }

        if (chargeDuration > 0f)
            _chargeEndTime = Time.time + chargeDuration;
        else
            _chargeEndTime = float.PositiveInfinity;

        SetAnim(true, speed);
    }

    void ChargeStep()
    {
        Vector3 step = _chargeDir * (speed * Time.deltaTime);
        transform.position += step;
        if (lockInitialY)
        {
            var p = transform.position; p.y = _yLock; transform.position = p;
        }

        SetAnim(true, speed);
        if (Time.time >= _chargeEndTime)
        {
            _charging = false;
            SetAnim(false, 0f);
        }
    }

    void SetAnim(bool running, float curSpeed)
    {
        if (animator == null) return;
        if (!string.IsNullOrEmpty(runBoolParam) && HasBoolParam(runBoolParam))
            animator.SetBool(runBoolParam, running);
        if (!string.IsNullOrEmpty(speedFloatParam) && HasFloatParam(speedFloatParam))
            animator.SetFloat(speedFloatParam, curSpeed);
    }

    bool HasBoolParam(string name)
    {
        if (animator == null || string.IsNullOrEmpty(name)) return false;
        var pars = animator.parameters;
        for (int i = 0; i < pars.Length; i++)
            if (pars[i].type == AnimatorControllerParameterType.Bool && pars[i].name == name)
                return true;
        return false;
    }

    bool HasFloatParam(string name)
    {
        if (animator == null || string.IsNullOrEmpty(name)) return false;
        var pars = animator.parameters;
        for (int i = 0; i < pars.Length; i++)
            if (pars[i].type == AnimatorControllerParameterType.Float && pars[i].name == name)
                return true;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.1f, 0.7f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, roverDetectRange);
    }
}
