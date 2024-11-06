using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy01Move : MonoBehaviour
{
    public enum Enemy01Mode
    {
        WALK,       // ����
        BACK,       // �߂�i�����ʒu�ցj
        RUSH,       // �ːi
        DIE,        // �|���
        KNOCK,      // �m�b�N�o�b�N
        PLAYER_DIE, // �v���C���[���|�ꂽ��
        MAX
    }

    // ���݂̃��[�h���C���X�y�N�^�Őݒ�\�ɂ���
    [SerializeField] private Enemy01Mode curMode;

    [SerializeField] private Enemy01Mode initialMode = Enemy01Mode.WALK;

    // �e���[�h���Ƃ̃J�X�^�}�C�Y�p�p�����[�^
    [Header("Movement Parameters")]
    [SerializeField] private float walkRange = 2.0f;      // �����͈�(�Q�[���J�n���̃X�|�[���ʒu���N�_)
    [SerializeField] private float visualRange = 5.0f;    // �v���C���[�����F����͈�
    [SerializeField] private float walkSpeed = 1.0f;      // �������x
    [SerializeField] private float rushSpeed = 2.0f;      // �ːi���x
    
    // ���̃t�B�[���h
    private Vector3 initPos;
    private GameObject playerObj;
    private Player player;
    private Animator animator;
    private AnimatorStateInfo animeInfo;
    private Transform thistrans;
    private Rigidbody2D rb2D;
    private GameObject scissors;
    private Vector3 pos;
    private float KnockTime = 0.0f;
    private int Step;
    private bool isStart = false;
    [SerializeField] private Enemy01Mode preMode;
    private bool isDead = false;
    private Enemy enemy;
    private EnemyStatus status;
    [SerializeField] private float dir;
    [SerializeField] private Vector3 BackDir;

    void Start()
    {
        enemy = this.transform.GetChild(0).GetComponent<Enemy>();
        status = this.transform.GetChild(0).GetComponent<EnemyStatus>();

        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        initPos = this.transform.position;
        playerObj = GameObject.Find("Actor");
        player = playerObj.GetComponent<Player>();

        curMode = initialMode;

        dir = 1;
        scissors = GameObject.Find("scissors1");
        Step = 0;
    }

    void Update()
    {
        thistrans = this.transform;
        pos = thistrans.position;

        // �̗͂O�ɂȂ����烂�[�h�ύX
        if (status.GetHp() <= 0)
        {
            curMode = Enemy01Mode.DIE;
        }

        // �v���C���[���|�ꂽ��������[�h��
        if (playerObj.GetComponent<PlayerStatus>().GetCurHp() <= 0 &&
            curMode != Enemy01Mode.WALK)
        {
            animator.SetBool("isAttack", false);
            curMode = Enemy01Mode.PLAYER_DIE;
        }

        // �U�����������ăm�b�N�o�b�N�������ĂȂ��Ƃ�
        if (scissors.GetComponent<AttackContoroll>().GethitFlg() && !isStart)
        {
            isStart = true;
            preMode = curMode;
            curMode = Enemy01Mode.KNOCK;
        }

        // ���[�h���Ƃɍs���p�^�[����ς���
        switch (curMode)
        {
            case Enemy01Mode.WALK:
                if (thistrans.position.x > initPos.x + walkRange)
                {
                    dir = -1;
                }
                if (thistrans.position.x < initPos.x - walkRange)
                {
                    dir = 1;
                }
                Search(dir);

                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                {
                    pos.x += dir * Time.deltaTime * walkSpeed;
                }
                break;

            case Enemy01Mode.BACK:
                BackDir = new Vector3((initPos.x - thistrans.position.x), 0, 0).normalized;
                Search(BackDir.x);
                dir = BackDir.x;

                if (Mathf.Abs(initPos.x - thistrans.position.x) < 1.0f)
                {
                    curMode = Enemy01Mode.WALK;
                }

                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                {
                    pos.x += BackDir.x * Time.deltaTime * walkSpeed;
                }
                break;

            case Enemy01Mode.RUSH:
                animator.SetBool("isAttack", true);

                if ((dir == 1 && (thistrans.position.x + dir * visualRange < player.transform.position.x ||
                    player.transform.position.x < thistrans.position.x)) ||
                    (dir == -1 && (thistrans.position.x + dir * visualRange > player.transform.position.x ||
                    player.transform.position.x > thistrans.position.x)))
                {
                    animator.SetBool("isAttack", false);
                    animator.SetBool("isCollide", false);
                    curMode = Enemy01Mode.BACK;
                }

                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Rush"))
                {
                    pos.x += dir * Time.deltaTime * rushSpeed;
                }
                break;

            case Enemy01Mode.DIE:
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (enemy != null)
                    {
                        enemy.SetIsDead(true);
                    }

                    if (!isDead)
                    {
                        StaticEnemy.IsUpdate = true;
                        isDead = true;
                    }
                }

                animator.SetBool("isDie", true);

                if (animator.GetCurrentAnimatorStateInfo(0).IsName("End"))
                {
                    if (enemy != null)
                    {
                        enemy.SetIsDead(true);
                    }

                    if (!isDead)
                    {
                        StaticEnemy.IsUpdate = true;
                        isDead = true;
                    }
                }
                break;

            case Enemy01Mode.PLAYER_DIE:
                BackDir = new Vector3((initPos.x - thistrans.position.x), 0, 0).normalized;
                dir = BackDir.x;

                if (Mathf.Abs(initPos.x - thistrans.position.x) < 1.0f)
                {
                    curMode = Enemy01Mode.WALK;
                }

                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                {
                    pos.x += BackDir.x * Time.deltaTime * walkSpeed;
                }
                break;

            case Enemy01Mode.KNOCK:
                if (isStart)
                {
                    KnockBack();
                }
                break;
        }

        thistrans.position = pos;
    }

    public void Search(float Dir)
    {
        if (playerObj.GetComponent<PlayerStatus>().GetCurHp() > 0)
        {
            if (Dir == 1.0f &&
            thistrans.position.x + Dir * visualRange > player.transform.position.x &&
            thistrans.position.x < player.transform.position.x)
            {
                curMode = Enemy01Mode.RUSH;
            }
            if (Dir == -1.0f &&
                thistrans.position.x + Dir * visualRange < player.transform.position.x &&
                thistrans.position.x > player.transform.position.x)
            {
                curMode = Enemy01Mode.RUSH;
            }
        }
    }

    // �m�b�N�o�b�N����
    private void KnockBack()
    {
        switch (Step)
        {
            // �v���C���[������i�q�b�g�X�g�b�v�H�j
            case 0:
                // �����̈ʒu�ƐڐG�����I�u�W�F�N�g�̈ʒu���v�Z����
                // �����ƕ������o���Đ��K��
                Vector3 distination = new Vector3((this.transform.position.x - player.transform.position.x), 0, 0).normalized;

                // �m�b�N�o�b�N�A�j�����Đ�����Ă����
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Knock"))
                {
                    // �m�b�N�o�b�N
                    pos.x += distination.x * Time.deltaTime;
                }
                // �Đ�����Ă���A�j�����I�������
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                {
                    // �ړ����������玟�̃X�e�b�v��
                    Step++;
                }
                break;

            case 1:

                // ���������ŏ��ɖ߂�
                Step = 0;
                // �m�b�N�o�b�N�O�̃��[�h�ɖ߂�
                curMode = preMode;
                // �m�b�N�o�b�N�A�j���I��
                animator.SetBool("isKnock", false);
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        rb2D.isKinematic = true;

        if (collision.gameObject.tag == "Player")
        {
            animator.SetBool("isCollide", true);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        rb2D.isKinematic = false;
        if (collision.gameObject.tag == "Player")
        {
            animator.SetBool("isCollide", false);
            isStart = false;
        }
    }
}
