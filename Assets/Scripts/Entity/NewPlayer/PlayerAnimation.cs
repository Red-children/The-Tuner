using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("组件引用")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    [Header("近战攻击")]
    public float meleeRange = 1.5f;
    public int meleeDamage = 20;
    public float meleeCoolDown = 0.5f;

    private PlayerStats stats;
    private PlayerWeapon playerWeapon;
    private PlayerMovement playerMovement;
    private PlayerDash playerDash;

    private double lastMeleeTime = -999f;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();

        stats = GetComponent<PlayerStats>();
        playerWeapon = GetComponent<PlayerWeapon>();
        playerMovement = GetComponent<PlayerMovement>();
        playerDash = GetComponent<PlayerDash>();
    }

    private void Update()
    {
        HandleFacing();
        HandleMovementInput();
        HandleShootInput();
        HandleMeleeInput();
        HandleDashInput();
        HandleWeaponSwitchInput();
    }

    private void HandleFacing()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        Vector2 directionMouse = mouseWorldPos - transform.position;

        if (directionMouse.x > 0)
            spriteRenderer.flipX = true;
        else if (directionMouse.x < 0)
            spriteRenderer.flipX = false;
    }

    private void HandleMovementInput()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        bool isMoving = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f;

        animator.SetBool("isMoving", isMoving);

        if (playerMovement != null)
            playerMovement.SetMovementInput(moveX, moveY);
    }

    private void HandleShootInput()
    {
        if (playerWeapon == null) return;

        WeaponInfo currentWeapon = playerWeapon.GetCurrentWeapon();
        if (currentWeapon == null) return;

        if (currentWeapon is BassCannon bassCannon)
        {
            if (Input.GetMouseButtonDown(0))
            {
                animator.SetBool("Charging", true);
                bassCannon.StartCharge();
            }
            if (Input.GetMouseButtonUp(0))
            {
                animator.SetBool("Charging", false);
                bassCannon.ReleaseCharge();
            }
        }
        else if (currentWeapon is StandardFirearm)
        {
            bool shootPressed = Input.GetMouseButtonDown(0);
            animator.SetBool("isShooting", shootPressed);
        }
    }

    private void HandleMeleeInput()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            double currentTime = AudioSettings.dspTime;
            if (currentTime <= lastMeleeTime + meleeCoolDown)
                return;

            lastMeleeTime = currentTime;
            animator.SetBool("isMeleeing", true);
        }
    }

    private void HandleDashInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (playerDash != null && playerDash.TryDash())
            {
                animator.SetBool("isDashing", true);
            }
        }
    }

    private void HandleWeaponSwitchInput()
    {
        if (playerWeapon == null) return;

        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                playerWeapon.SwitchWeapon(i);
                return;
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            playerWeapon.SwitchWeaponByScroll(scroll);
        }
    }

    public void OnShootAnimationEvent()
    {
        var weapon = playerWeapon?.GetCurrentWeapon();
        if (weapon is StandardFirearm firearm)
        {
            firearm.HandleFireInput();
        }
        else if (weapon is BassCannon cannon)
        {
            cannon.ReleaseCharge();
        }
    }

    public void OnMeleeAnimationEvent()
    {
        if (stats == null) return;

        var rhythmResult = SampleRhythm(AudioSettings.dspTime, "Melee");
        float finalDamage = (stats.TotalAttack + meleeDamage) * rhythmResult.multiplier;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, meleeRange, LayerMask.GetMask("Enemy"));

        bool hitEnemy = false;
        RhythmRank highestRank = RhythmRank.Miss;

        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                if (enemy is EnemyController ec)
                    ec.SetAttackerPosition(transform.position);

                enemy.Wound(finalDamage, rhythmResult.rank);
                hitEnemy = true;
                highestRank = (RhythmRank)Mathf.Max((int)highestRank, (int)rhythmResult.rank);

                if (HitStopManager.Instance != null)
                    HitStopManager.Instance.TriggerEnemyHitStop(hit.gameObject, rhythmResult.rank, finalDamage);
            }
        }

        if (hitEnemy && HitStopManager.Instance != null)
            HitStopManager.Instance.TriggerPlayerHitStop(highestRank, finalDamage);

        EventBus.Instance.Trigger(new PlayerMeleeEvent
        {
            damage = finalDamage,
            hitPoint = transform.position
        });
    }

    public void OnShootAnimationEnd()
    {
        animator.SetBool("isShooting", false);
    }

    public void OnMeleeAnimationEnd()
    {
        animator.SetBool("isMeleeing", false);
    }

    public void OnDashAnimationEnd()
    {
        animator.SetBool("isDashing", false);
    }

    public void OnChargeAnimationEnd()
    {
        animator.SetBool("isCharging", false);
    }

    private RhythmManager.RankResult SampleRhythm(double inputDspTime, string source)
    {
        if (RhythmManager.Instance == null)
        {
            return new RhythmManager.RankResult
            {
                rank = RhythmRank.Miss,
                multiplier = 1f,
                isInWindow = false,
                judgedDspTime = inputDspTime,
                referenceBeatTime = inputDspTime,
                offsetSeconds = 0,
                offsetMilliseconds = 0
            };
        }

        var gameplayResult = RhythmManager.Instance.GetRankAtTime(inputDspTime);
        var debugResult = RhythmManager.Instance.GetDebugRankAtTime(inputDspTime);
        EventBus.Instance.Trigger(new RhythmInputDebugEvent
        {
            inputDspTime = inputDspTime,
            judgedDspTime = debugResult.judgedDspTime,
            referenceBeatTime = debugResult.referenceBeatTime,
            offsetSeconds = debugResult.offsetSeconds,
            offsetMilliseconds = debugResult.offsetMilliseconds,
            rank = debugResult.rank,
            multiplier = gameplayResult.multiplier,
            isInWindow = debugResult.isInWindow,
            source = source
        });
        return gameplayResult;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}
