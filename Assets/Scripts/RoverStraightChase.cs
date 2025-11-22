using UnityEngine;

// Anexe ao Rover_2. Quando o jogador entra em uma zona de detecção
// de longo alcance, maior que o alcance dos inimigos, o rover
// avança em linha reta seguindo sua própria orientação (eixo local),
// capturando a direção no momento da detecção (sem perseguição).
public class RoverStraightChase : MonoBehaviour
{
    [Header("Alvo")]
    public bool autoFindTarget = true;
    public Transform target; // auto-resolved from PlayerHealth/PlayerMovement

    [Header("Alcances")]
    [Tooltip("O Rover detecta o jogador dentro dessa distância.")]
    public float roverDetectRange = 28f;
    [Tooltip("Alcance de detecção do inimigo para comparação. Se <= 0, tenta ler de EnemyLargeChase na cena, caso contrário usa este valor.")]
    public float enemyDetectRangeOverride = -1f; // e.g., 15f like EnemyLargeChase

    [Header("Movimento")]
    public float speed = 12f;
    [Tooltip("Travar o Y enquanto se move para manter o rover na altura original.")]
    public bool lockInitialY = true;
    public float rotationSpeed = 20f; // not used unless alignVisualToDirection is enabled

    [Header("Orientação")]
    public Transform forwardReference; // opcional: usar o eixo local deste transform
    public enum LocalAxis { Z, X }
    public enum ChargeDirectionMode { LocalAxis, WorldX, WorldZ }
    public ChargeDirectionMode directionMode = ChargeDirectionMode.LocalAxis;
    public LocalAxis axis = LocalAxis.Z; // quando usando modo LocalAxis
    public bool invertDirection = false; // inverte a direção escolhida
    public bool alignVisualToDirection = false; // rotaciona o objeto para olhar na direção do avanço no início

    [Header("Investida")]
    [Tooltip("Duração da investida reta antes de parar. Defina <= 0 para infinito.")]
    public float chargeDuration = 2.5f;
    public bool repeatCharges = true; // se false, avança apenas uma vez por entrada

    [Header("Animação (opcional)")]
    public Animator animator; // opcional
    public string runBoolParam = "Run"; // fica true enquanto está avançando
    public string speedFloatParam = "Speed"; // opcional: define a velocidade atual

    [Header("Áudio")]
    [Tooltip("Som em loop do movimento do rover.")]
    public AudioSource audioMotor;
    [Range(0f,1f)] public float volumeMotor = 1f;
    [Tooltip("Se verdadeiro, o som do motor começa assim que o rover for renderizado (OnBecameVisible). Se falso, começa apenas ao iniciar a investida.")]
    public bool motorIniciaAoSerRenderizado = false;

    float _enemyDetectRangeCached = 15f;
    bool _charging;
    float _chargeEndTime;
    Vector3 _chargeDir;
    float _yLock;
    bool _didChargeOnce;

    void Reset()
    {
        // Valores padrão adequados para Rover_2
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

        if (audioMotor != null)
        {
            audioMotor.loop = true;
            audioMotor.playOnAwake = false;
            audioMotor.volume = volumeMotor;
            audioMotor.Stop();
        }
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

        AtualizarAudioMotor();
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
        // Rover_2: inicia som do motor quando começa a investida
        if (!motorIniciaAoSerRenderizado && audioMotor != null && MusicManager.SfxEnabled && !audioMotor.isPlaying)
        {
            audioMotor.volume = volumeMotor;
            audioMotor.Play();
        }
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
            // Rover_2: para o som quando termina a investida
            if (!motorIniciaAoSerRenderizado && audioMotor != null && audioMotor.isPlaying)
            {
                audioMotor.Stop();
            }
        }
    }

    void OnBecameVisible()
    {
        // Rover_Round: som começa ao ser renderizado
        if (motorIniciaAoSerRenderizado && audioMotor != null && MusicManager.SfxEnabled && !audioMotor.isPlaying)
        {
            audioMotor.volume = volumeMotor;
            audioMotor.Play();
        }
    }

    void OnBecameInvisible()
    {
        if (motorIniciaAoSerRenderizado && audioMotor != null && audioMotor.isPlaying)
        {
            audioMotor.Stop();
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

    void AtualizarAudioMotor()
    {
        if (audioMotor == null) return;

        // Apenas respeita a flag global de efeitos:
        // se SomJogo estiver desligado, silencia o motor.
        audioMotor.mute = !MusicManager.SfxEnabled;
        audioMotor.volume = volumeMotor;
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
