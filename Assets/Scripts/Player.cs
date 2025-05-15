using UnityEngine;
using System.Collections; // Coroutine 사용을 위해 추가
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Volume 사용을 위해 추가

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    // 컴포넌트 참조
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    [Header("이동 설정")]
    public float moveSpeed = 5f;
    private float horizontalInput; // 현재 프레임의 수평 입력 값 (-1, 0, 1)
    private float lastHorizontalInput = 1f; // 마지막으로 바라본 방향 (1: 오른쪽, -1: 왼쪽)
    public float airControlFactor = 0.2f; // 공중에서의 수평 제어 강도 (0: 제어 불가, 1: 지상과 동일)

    [Header("점프 설정")]
    public float jumpForce = 10f;
    public Transform groundCheck; // 땅 감지 오브젝트 위치 (캐릭터 발 밑)
    public float groundCheckRadius = 0.2f; // 땅 감지 범위
    public LayerMask groundLayer; // 땅으로 인식할 레이어
    private bool isGrounded; // 현재 땅에 있는지 여부
    private bool canDoubleJump = false; // 더블 점프 가능 여부

    [Header("웅크리기 설정")]
    private bool isCrouchingInput = false; // 웅크리기 입력 상태 (버튼 눌림 여부)
    // public Transform ceilingCheck; // 일어설 때 천장 체크 (옵션)
    // public float ceilingCheckRadius = 0.2f; // 천장 감지 범위 (옵션)

    [Header("발포 설정")]
    private bool isAttacking = false; // 현재 발포 애니메이션 중인지 여부 (애니메이션 이벤트로 제어 권장)
    // 발포 관련 추가 변수 (예: 총알 프리팹, 발사 위치 등)
    public GameObject bulletPrefab; // 총알 프리팹 (인스펙터에서 연결)

    // 플레이어 상태에 따른 발사 위치 Transform들 (인스펙터에서 연결)
    public Transform standRightFirePoint;
    public Transform standLeftFirePoint;
    public Transform crouchRightFirePoint;
    public Transform crouchLeftFirePoint;

    [Header("플레이어 체력")]
    public int maxHealth = 10; // 최대 체력
    private int currentHealth; // 현재 체력


    [Header("스테미나 설정")]
    public float maxStamina = 10f;
    private float currentStamina;
    public float staminaRechargeRate = 1f; // 초당 스테미나 회복량
    public float staminaRechargeDelay = 1f; // 스테미나 소모 후 회복 시작까지의 지연 시간
    private float lastStaminaUseTime;

    [Header("대시 설정")]
    public float dashForce = 15f; // 대시 시 적용될 힘
    public float dashStaminaCost = 3.5f; // 대시 소모 스테미나
    public float dashDuration = 0.1f; // 대시 지속 시간 (순간 이동에 가깝다면 짧게)
    public float additionalYForce = 4.5f;
    private bool isDashing = false; // 현재 대시 중인지 여부
    private float dashEndTime;

    // 대시 더블 탭 감지용 변수 (수정)
    public float doubleTapTimeThreshold = 0.3f; // 더블 탭으로 인식할 최대 시간 간격
    private float lastHorizontalPressTime = 0f; // 마지막 수평 입력(눌림) 시간
    private int lastPressDirection = 0; // 마지막으로 눌린 수평 방향 (-1: 왼쪽, 1: 오른쪽)


    [Header("더블 점프 설정")]
    public float doubleJumpStaminaCost = 5f; // 더블 점프 소모 스테미나
    public float doubleJumpForceFactor = 0.75f; // 일반 점프 힘 대비 더블 점프 힘 비율

    [Header("죽음 설정")] // 죽음 관련 헤더 추가
    public float deathYThreshold = -5.8f; // 플레이어가 죽는 Y 좌표 임계값
    public GameObject deathParticlesPrefab; // 플레이어 죽을 때 생성될 파티클 시스템 프리팹 (인스펙터에서 연결)
    private bool isDead = false; // 플레이어가 죽었는지 여부

    void Awake()
    {
        // 필요한 컴포넌트 가져오기
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (groundCheck == null)
        {
            Debug.LogError("GroundCheck Transform이 할당되지 않았습니다! 땅 감지 오브젝트를 연결해주세요.");
        }
        // 총알 프리팹이 필요하다면 확인
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Bullet Prefab이 할당되지 않았습니다. 총알 발사 기능이 작동하지 않습니다.");
        }
        // 모든 발사 위치 Transform들이 할당되었는지 확인
        if (standRightFirePoint == null || standLeftFirePoint == null || crouchRightFirePoint == null || crouchLeftFirePoint == null)
        {
            Debug.LogWarning("모든 발사 위치 Transform(Stand/Crouch, Left/Right)이 할당되지 않았습니다. 총알 발사 위치가 정확하지 않을 수 있습니다.");
        }
        // 죽음 파티클 프리팹 확인
        if (deathParticlesPrefab == null)
        {
            Debug.LogWarning("Death Particles Prefab이 할당되지 않았습니다. 플레이어 죽음 시 파티클 효과가 나타나지 않습니다.");
        }




        // 스테미나 초기화
        currentStamina = maxStamina;
        lastStaminaUseTime = Time.time; // 시작 시 회복 바로 시작하도록 설정
    }

    void Update()
    {
        // 플레이어가 죽었으면 더 이상 업데이트 로직을 실행하지 않음
        if (isDead) return;

        // ==== 죽음 감지 ====
        if (transform.position.y < deathYThreshold && !isDead)
        {
            Die();
            return; // 죽음 처리 후 Update 종료
        }

        // ==== 입력 처리 ====
        float currentHorizontalInput = Input.GetAxisRaw("Horizontal"); // 현재 프레임의 입력 값

        // 웅크리기 입력
        isCrouchingInput = Input.GetButton("Crouch");

        // 점프 입력
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded && !isCrouchingInput && !isAttacking) // 지상 점프
            {
                Jump(jumpForce);
                anim.SetTrigger("JumpTrigger");
            }
            else if (!isGrounded && canDoubleJump && !isCrouchingInput && !isAttacking && !isDashing && currentStamina >= doubleJumpStaminaCost) // 더블 점프
            {
                Jump(jumpForce * doubleJumpForceFactor);
                ConsumeStamina(doubleJumpStaminaCost);
                canDoubleJump = false;
                anim.SetTrigger("JumpTrigger");
            }
        }

        // ==== 발포 입력 및 애니메이션 트리거 발동 ====
        // 발포 버튼이 눌렸고, 공격 중이 아닐 때 발포 시도
        // 지상에서만 발포 가능하도록 isGrounded 조건 추가
        if (Input.GetButtonDown("Fire1") && !isAttacking && isGrounded) // 유니티 Input Manager에 "Fire1" 설정 필요 (기본 Left Mouse Button)
        {
            // 발포 애니메이션 트리거 발동
            anim.SetTrigger("ShootTrigger");

            // ==== 실제 발포 로직 호출 ====
            Shoot();
            // ===========================

            CameraEffectManager.Instance.ApplyCromaticAbb(); // 발포 시 색수차 효과 적용

        }


        // ==== 대시 더블 탭 감지 (수정) ====
        // 수평 입력 키가 눌리는 순간 감지
        if (currentHorizontalInput != 0 && horizontalInput == 0) // 이전 프레임은 0이었는데 현재 프레임은 0이 아닐 때
        {
            int direction = (int)Mathf.Sign(currentHorizontalInput); // 현재 눌린 방향

            // 마지막으로 눌린 방향과 같고, 시간 간격 내에 다시 눌렸다면 더블 탭
            if (direction == lastPressDirection && Time.time - lastHorizontalPressTime < doubleTapTimeThreshold)
            {
                TryDash(direction); // 대시 실행
                // 더블 탭 성공 후 관련 변수 초기화 (선택 사항이지만 명확하게)
                lastHorizontalPressTime = 0f;
                lastPressDirection = 0;
            }
            else // 첫 번째 탭이거나, 다른 방향 탭이거나, 시간 초과
            {
                // 현재 탭 정보 저장
                lastHorizontalPressTime = Time.time;
                lastPressDirection = direction;
            }
        }


        // 이전 프레임의 수평 입력 상태를 저장 (다음 프레임의 눌림 감지를 위해)
        horizontalInput = currentHorizontalInput;


        // ==== Animator 파라미터 업데이트 ====
        anim.SetFloat("Speed", Mathf.Abs(currentHorizontalInput)); // 현재 입력 값 사용
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        anim.SetBool("IsCrouching", isCrouchingInput);

        // ==== 스프라이트 방향 뒤집기 ====
        // 이동 입력이 있을 때만 방향 업데이트 (정지 후 마지막 방향 유지를 위해)
        if (currentHorizontalInput != 0) // 현재 입력 값 사용
        {
            sr.flipX = (currentHorizontalInput < 0);
            lastHorizontalInput = currentHorizontalInput; // 마지막 이동 방향 저장
        }
        else // 수평 입력이 0일 때 (정지)
        {
            sr.flipX = (lastHorizontalInput < 0);
        }

        // ==== 스테미나 회복 ====
        if (Time.time - lastStaminaUseTime >= staminaRechargeDelay)
        {
            currentStamina += staminaRechargeRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }

        // ==== 대시 종료 처리 ====
        if (isDashing && Time.time >= dashEndTime)
        {
            isDashing = false;
        }
    }

    void FixedUpdate()
    {
        // 플레이어가 죽었으면 더 이상 물리 로직을 실행하지 않음
        if (isDead)
        {
            // 죽었을 때 Rigidbody의 속도를 0으로 만들거나 물리 영향을 끄는 등의 처리
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false; // 물리 시뮬레이션 비활성화
            return; // 죽음 처리 후 FixedUpdate 종료
        }

        // ==== 물리 기반 이동 적용 ====
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            canDoubleJump = true;
            // FallFlag 처리 (StateMachineBehaviour 사용 시)
        }
        else if (!isGrounded && wasGrounded && rb.linearVelocity.y < -0.01f)
        {
            // FallFlag 처리 (StateMachineBehaviour 사용 시)
        }

        // 수평 이동 적용
        // 웅크리기 입력 상태가 아니고(!isCrouchingInput), 공격 중이 아니고(!isAttacking), 대시 중이 아닐 때만(!) 수평 이동 허용
        if (!isCrouchingInput && !isAttacking && !isDashing)
        {
            if (isGrounded) // 지상 이동
            {
                rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y); // Update에서 저장된 horizontalInput 사용
            }
            else // 공중 이동 제어 제한
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x + horizontalInput * moveSpeed * airControlFactor * Time.fixedDeltaTime, rb.linearVelocity.y); // Update에서 저장된 horizontalInput 사용
            }
        }
        else if (isDashing)
        {
            // 대시 중에는 다른 수평 이동 입력을 무시 (대시 시작 시 속도 설정)
        }
        else // 웅크리거나 공격 중일 때는 수평 속도를 0으로 만듦
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void Jump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(new Vector2(0f, force), ForceMode2D.Impulse);
    }

    void TryDash(int direction)
    {
        if (!isDashing && currentStamina >= dashStaminaCost && !isCrouchingInput && !isAttacking)
        {
            ConsumeStamina(dashStaminaCost);
            isDashing = true;
            dashEndTime = Time.time + dashDuration;

            rb.linearVelocity = new Vector2(direction * dashForce, rb.linearVelocity.y + additionalYForce);
        }
    }

    void ConsumeStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        lastStaminaUseTime = Time.time;
    }

    // 애니메이션 이벤트 함수: 발포 애니메이션 시작 시 호출
    public void StartAttack()
    {
        isAttacking = true;
    }

    // 애니메이션 이벤트 함수: 발포 애니메이션 종료 시 호출
    public void EndAttack()
    {
        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        // if (ceilingCheck != null)
        // {
        //     Gizmos.color = Color.blue;
        //     Gizwis.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);
        // }
    }

    public float CurrentStamina
    {
        get { return currentStamina; }
    }

    public int CurrentHealth {  get { return currentHealth; }    }

    public float MaxStamina
    {
        get { return maxStamina; }
    }

    // 실제 발포 로직을 구현할 함수
    void Shoot()
    {
        // 총알 프리팹이 할당되었는지 확인
        if (bulletPrefab == null)
        {
            Debug.LogWarning("총알 프리팹이 할당되지 않아 발사할 수 없습니다.");
            return; // 프리팹 없으면 함수 종료
        }

        // 현재 플레이어 상태에 맞는 발사 위치 Transform 선택
        Transform currentFirePoint = null;
        if (isCrouchingInput) // 웅크린 상태
        {
            if (sr.flipX) // 왼쪽 보고 있음
            {
                currentFirePoint = crouchLeftFirePoint;
            }
            else // 오른쪽 보고 있음
            {
                currentFirePoint = crouchRightFirePoint;
            }
        }
        else // 서있는 상태
        {
            if (sr.flipX) // 왼쪽 보고 있음
            {
                currentFirePoint = standLeftFirePoint;
            }
            else // 오른쪽 보고 있음
            {
                currentFirePoint = standRightFirePoint;
            }
        }

        // 선택된 발사 위치가 유효한지 확인
        if (currentFirePoint == null)
        {
            Debug.LogWarning("현재 플레이어 상태에 맞는 발사 위치(Fire Point) Transform이 할당되지 않았습니다!");
            return; // 발사 위치 없으면 함수 종료
        }


        // 총알 프리팹 인스턴스 생성
        GameObject bullet = Instantiate(bulletPrefab, currentFirePoint.position, currentFirePoint.rotation);

        // 총알의 Movement2D 컴포넌트 가져오기
        Movement2D bulletMovement = bullet.GetComponent<Movement2D>();

        // Movement2D 컴포넌트가 있다면 총알 방향 설정 및 스프라이트 뒤집기
        if (bulletMovement != null)
        {
            // 플레이어의 스프라이트 방향에 따라 총알 방향 결정
            Vector3 shootDirection = sr.flipX ? Vector3.left : Vector3.right;
            bulletMovement.MoveTo(shootDirection);

            // 총알 스프라이트 뒤집기 (플레이어 방향과 일치하도록)
            SpriteRenderer bulletSr = bullet.GetComponent<SpriteRenderer>();
            if (bulletSr != null)
            {
                bulletSr.flipX = sr.flipX; // 플레이어의 flipX 값을 그대로 적용
            }
            else
            {
                Debug.LogWarning("발사된 총알 프리팹에 SpriteRenderer 컴포넌트가 없습니다!");
            }
        }
        else
        {
            Debug.LogWarning("발사된 총알 프리팹에 Movement2D 컴포넌트가 없습니다!");
        }

        Debug.Log("총알 발사!"); // 실제 발포 기능 구현 (예: 총알 오브젝트 생성 및 초기화)
    }



    // 플레이어가 죽을 때 호출될 함수
    void Die()
    {
        // 이미 죽은 상태면 다시 처리하지 않음
        if (isDead) return;

        isDead = true; // 죽음 상태 플래그 설정
        Debug.Log("Player Died!"); // 디버그 메시지

        // 파티클 효과 생성
        if (deathParticlesPrefab != null)
        {
            Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Death Particles Prefab이 할당되지 않아 죽음 파티클을 생성할 수 없습니다.");
        }

        // 플레이어 게임 오브젝트 비활성화 또는 파괴
        // Rigidbody 비활성화 (물리 영향 받지 않도록)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        // SpriteRenderer 비활성화 (캐릭터 스프라이트 숨기기)
        if (sr != null)
        {
            sr.enabled = false;
        }

        // Collider 비활성화 (다른 오브젝트와 충돌하지 않도록)
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if(currentHealth <= 0)
        {
            Die();
        }
    }
}
