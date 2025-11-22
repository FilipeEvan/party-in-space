using UnityEngine;
using UnityEngine.AI;

// Attach to Enemy_Large. When the player is within detectionRange,
// the enemy starts chasing using NavMeshAgent if available; otherwise
// it moves directly towards the player on the XZ plane.
public class EnemyLargeChase : MonoBehaviour
{
    [Header("Alvo")]
    public bool autoFindTarget = true;
    public Transform target; // auto filled with PlayerHealth/PlayerMovement if null
    public float detectionRange = 15f;
    public float lostRange = 22f;   // stop chasing when target farther than this

    [Header("Movimento")]
    public bool useNavMeshIfAvailable = true;
    public float chaseSpeed = 4.5f;
    public float rotationSpeed = 10f; // turning speed towards target
    public float stopDistance = 1.5f;

    [Header("Animação (opcional)")]
    public Animator animator; // optional
    public string walkBoolParam = "Walk";         // popular no USK
    public string runBoolParam = "Run";           // fallback
    public string speedFloatParam = "Speed";      // set to current planar speed (blend trees)
    public string chaseStartTriggerParam = "";    // optional trigger on start chase
    [Header("Estados de Animação (nomes do controlador)")]
    public string idleStateName = "CharacterArmature|Idle";
    public string runStateName = "CharacterArmature|Run";
    public string punchStateName = "CharacterArmature|Punch";
    public string runTriggerParam = "";   // se usar trigger para iniciar corrida
    public string punchTriggerParam = "Punch"; // se usar trigger para soco

    [Header("Combate")]
    public bool enableAttack = false; // se falso, não toca animação de ataque
    public int damage = 1; // dano ao encostar no jogador
    public float attackRange = 2.0f; // quando estiver perto o suficiente, tocar Punch
    public float attackCooldown = 1.2f;
    float lastAttackTime = -999f;

    [Header("Áudio")]
    [Tooltip("Som em loop da corrida (tocado enquanto está perseguindo).")]
    public AudioSource audioCorrida;
    [Tooltip("Som curto ao detectar o jogador (grunhido/roar).")]
    public AudioSource audioAviso;
    [Range(0f,1f)] public float volumeCorrida = 1f;
    [Range(0f,1f)] public float volumeAviso = 1f;

    NavMeshAgent agent;
    bool isChasing;
    float startY; // keep original Y when moving without NavMesh

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        startY = transform.position.y;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        ValidateRanges();
        SetupAgent();
        AutoMapAnimatorParams();
        AutoMapAnimatorStates();
        // Garante que, ao ser renderizado e antes de detectar o player,
        // o inimigo comece na animação de Idle (se existir).
        PlayIdle();

        if (audioCorrida != null)
        {
            audioCorrida.loop = true;
            audioCorrida.volume = volumeCorrida;
            audioCorrida.playOnAwake = false;
            audioCorrida.Stop();
        }
        if (audioAviso != null)
        {
            audioAviso.loop = false;
            audioAviso.volume = volumeAviso;
            audioAviso.playOnAwake = false;
        }
    }

    void OnValidate()
    {
        ValidateRanges();
    }

    void ValidateRanges()
    {
        if (lostRange < detectionRange) lostRange = detectionRange + 1f;
        if (stopDistance < 0.1f) stopDistance = 0.1f;
    }

    void SetupAgent()
    {
        if (useNavMeshIfAvailable)
        {
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = chaseSpeed;
                agent.stoppingDistance = stopDistance;
                agent.updateRotation = false; // we rotate manually for nicer turning
                agent.autoBraking = true;
            }
        }
    }

    void Update()
    {
        if (autoFindTarget && target == null)
        {
            var ph = FindObjectOfType<PlayerHealth>();
            if (ph != null) target = ph.transform;
            else
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

        float dist = Vector3.Distance(transform.position, target.position);
        if (!isChasing)
        {
            if (dist <= detectionRange) StartChase();
        }
        else
        {
            if (dist > lostRange) StopChase();
        }

        if (isChasing)
        {
            ChaseStep();
        }
        else
        {
            SetAnim(false, 0f);
        }

        AtualizarAudioCorrida();
    }

    void ChaseStep()
    {
        if (target == null)
        {
            StopChase();
            return;
        }

        Vector3 toTarget = target.position - transform.position;
        Vector3 planarDir = new Vector3(toTarget.x, 0f, toTarget.z);

        // Face target smoothly
        if (planarDir.sqrMagnitude > 0.0001f)
        {
            var targetRot = Quaternion.LookRotation(planarDir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        float curSpeed = 0f;

        if (agent != null && useNavMeshIfAvailable && agent.isOnNavMesh)
        {
            agent.speed = chaseSpeed;
            agent.stoppingDistance = stopDistance;
            agent.SetDestination(target.position);
            curSpeed = agent.velocity.magnitude;
        }
        else
        {
            // Fallback: simple planar move towards target, keep Y
            float dist = planarDir.magnitude;
            if (dist > stopDistance)
            {
                Vector3 step = planarDir.normalized * (chaseSpeed * Time.deltaTime);
                transform.position += step;
                var p = transform.position; p.y = startY; transform.position = p;
                curSpeed = chaseSpeed;
            }
        }

        SetAnim(true, curSpeed);

        // Se estiver bem perto, toca Punch
        float distToTarget = (target.position - transform.position).magnitude;
        if (distToTarget <= Mathf.Max(stopDistance, attackRange))
        {
            TryPunch();
        }
    }

    void StartChase()
    {
        isChasing = true;
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;
        if (!string.IsNullOrEmpty(chaseStartTriggerParam) && animator != null)
        {
            animator.ResetTrigger(chaseStartTriggerParam);
            animator.SetTrigger(chaseStartTriggerParam);
        }
        // Já começa indicando velocidade de corrida para o Animator,
        // assim o blend tree/estado de Run entra imediatamente ao detectar o player.
        SetAnim(true, chaseSpeed);
        // Além disso, força o estado de corrida via trigger/crossfade,
        // independente de usar bool de Run ou não.
        PlayRun();

        // Som de aviso (grunhido) ao detectar o jogador
        if (audioAviso != null && MusicManager.SfxEnabled)
        {
            audioAviso.volume = volumeAviso;
            audioAviso.Play();
        }
    }

    void StopChase()
    {
        isChasing = false;
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
        SetAnim(false, 0f);
        PlayIdle();

        // StopChase não força o áudio; AtualizarAudioCorrida
        // cuida de parar/ligar baseado em isChasing + SfxEnabled.
    }

    void SetAnim(bool running, float speed)
    {
        if (animator == null) return;
        bool hasRun = !string.IsNullOrEmpty(runBoolParam) && HasBoolParam(runBoolParam);
        bool hasWalk = !string.IsNullOrEmpty(walkBoolParam) && HasBoolParam(walkBoolParam);

        if (hasRun)
        {
            animator.SetBool(runBoolParam, running);
            // Se também existir Walk, mantenha Walk=false durante corrida para evitar conflitos
            if (hasWalk) animator.SetBool(walkBoolParam, !running);
        }
        else if (hasWalk)
        {
            animator.SetBool(walkBoolParam, running);
        }

        if (!string.IsNullOrEmpty(speedFloatParam) && HasFloatParam(speedFloatParam))
        {
            animator.SetFloat(speedFloatParam, speed);
        }
    }

    void AtualizarAudioCorrida()
    {
        if (audioCorrida == null) return;

        bool deveTocar = MusicManager.SfxEnabled &&
                         isChasing &&
                         Time.timeScale > 0.01f;

        audioCorrida.volume = volumeCorrida;

        if (deveTocar)
        {
            if (!audioCorrida.isPlaying)
            {
                audioCorrida.Play();
            }
        }
        else
        {
            if (audioCorrida.isPlaying)
            {
                audioCorrida.Stop();
            }
        }
    }

    void PlayRun()
    {
        if (animator == null) return;
        // Mesmo tendo bool de Run, garantimos que o estado de corrida seja tocado
        // via CrossFade quando começarmos a perseguir, caso o controller não tenha
        // transições configuradas para o parâmetro.
        if (!string.IsNullOrEmpty(runTriggerParam) && HasTriggerParam(runTriggerParam))
        {
            animator.ResetTrigger(runTriggerParam);
            animator.SetTrigger(runTriggerParam);
            return;
        }
        if (!string.IsNullOrEmpty(runStateName) && HasState(runStateName))
        {
            animator.CrossFade(runStateName, 0.1f, 0, 0f);
        }
    }

    void PlayIdle()
    {
        if (animator == null) return;
        if (!string.IsNullOrEmpty(idleStateName) && HasState(idleStateName))
        {
            animator.CrossFade(idleStateName, 0.1f, 0, 0f);
        }
    }

    void TryPunch()
    {
        if (!enableAttack) return;

        if (Time.time < lastAttackTime + attackCooldown) return;
        lastAttackTime = Time.time;
        if (animator == null) return;
        if (!string.IsNullOrEmpty(punchTriggerParam) && HasTriggerParam(punchTriggerParam))
        {
            animator.ResetTrigger(punchTriggerParam);
            animator.SetTrigger(punchTriggerParam);
            return;
        }
        if (!string.IsNullOrEmpty(punchStateName) && HasState(punchStateName))
        {
            animator.CrossFade(punchStateName, 0.05f, 0, 0f);
        }
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

    void AutoMapAnimatorStates()
    {
        if (animator == null) return;

        // Idle: tenta casar nomes comuns de estado (Idle simples ou com prefixo do armature)
        if (string.IsNullOrEmpty(idleStateName) || !HasState(idleStateName))
        {
            string[] candidates = { "Idle", "CharacterArmature|Idle" };
            foreach (var c in candidates)
            {
                if (HasState(c)) { idleStateName = c; break; }
            }
        }

        // Run: tenta achar um estado de corrida, seja com ou sem prefixo
        if (string.IsNullOrEmpty(runStateName) || !HasState(runStateName))
        {
            string[] candidates = { "Run", "CharacterArmature|Run" };
            foreach (var c in candidates)
            {
                if (HasState(c)) { runStateName = c; break; }
            }
        }

        // Punch: tenta achar um estado de soco/ataque
        if (string.IsNullOrEmpty(punchStateName) || !HasState(punchStateName))
        {
            string[] candidates = { "Punch", "CharacterArmature|Punch", "Attack" };
            foreach (var c in candidates)
            {
                if (HasState(c)) { punchStateName = c; break; }
            }
        }
    }

    void AutoMapAnimatorParams()
    {
        if (animator == null) return;
        var pars = animator.parameters;
        // Tenta encontrar um bool de caminhada comum em pacotes como USK
        if (string.IsNullOrEmpty(walkBoolParam) || !HasBoolParam(walkBoolParam))
        {
            string[] candidates = { "Walk", "Walking", "IsWalking", "Move", "Moving" };
            foreach (var c in candidates)
            {
                if (HasBoolParam(c)) { walkBoolParam = c; break; }
            }
        }
        // Tenta encontrar bool de corrida
        if (string.IsNullOrEmpty(runBoolParam) || !HasBoolParam(runBoolParam))
        {
            string[] candidates = { "Run", "Running", "IsRunning", "Move", "Moving" };
            foreach (var c in candidates)
            {
                if (HasBoolParam(c)) { runBoolParam = c; break; }
            }
        }
        // Se não tiver Speed, tenta nomes comuns
        if (string.IsNullOrEmpty(speedFloatParam) || !HasFloatParam(speedFloatParam))
        {
            string[] candidates = { "Speed", "MoveSpeed", "Velocity" };
            foreach (var c in candidates)
            {
                if (HasFloatParam(c)) { speedFloatParam = c; break; }
            }
        }
        // Trigger de Punch
        if (string.IsNullOrEmpty(punchTriggerParam) || !HasTriggerParam(punchTriggerParam))
        {
            string[] candidates = { "Punch", "Attack", "Hit" };
            foreach (var c in candidates)
            {
                if (HasTriggerParam(c)) { punchTriggerParam = c; break; }
            }
        }
    }

    bool HasTriggerParam(string name)
    {
        if (animator == null || string.IsNullOrEmpty(name)) return false;
        var pars = animator.parameters;
        for (int i = 0; i < pars.Length; i++)
            if (pars[i].type == AnimatorControllerParameterType.Trigger && pars[i].name == name)
                return true;
        return false;
    }

    bool HasState(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName)) return false;
        // Tenta tanto o nome simples quanto o nome com o prefixo da layer (ex.: "Base Layer.")
        int idSimple = Animator.StringToHash(stateName);
        if (animator.HasState(0, idSimple)) return true;
        string layerName = animator.GetLayerName(0);
        int idWithLayer = Animator.StringToHash(layerName + "." + stateName);
        if (animator.HasState(0, idWithLayer)) return true;
        int idBaseLayer = Animator.StringToHash("Base Layer." + stateName);
        return animator.HasState(0, idBaseLayer);
    }

    // Dano ao contato (3D/2D, trigger ou colisão). Player tem i-frames.
    void DamageIfPlayer(Component c)
    {
        var health = c.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage, transform.position);
        }
    }

    void OnTriggerEnter(Collider other) => DamageIfPlayer(other);
    void OnCollisionEnter(Collision collision) => DamageIfPlayer(collision.collider);
    void OnTriggerEnter2D(Collider2D other) => DamageIfPlayer(other);
    void OnCollisionEnter2D(Collision2D collision) => DamageIfPlayer(collision.collider);

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, lostRange);
    }
}
