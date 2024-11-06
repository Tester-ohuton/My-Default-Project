using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy01Move : MonoBehaviour
{
    public enum Enemy01Mode
    {
        WALK,       // 歩く
        BACK,       // 戻る（初期位置へ）
        RUSH,       // 突進
        DIE,        // 倒れる
        KNOCK,      // ノックバック
        PLAYER_DIE, // プレイヤーが倒れた後
        MAX
    }

    // 現在のモードをインスペクタで設定可能にする
    [SerializeField] private Enemy01Mode curMode;

    [SerializeField] private Enemy01Mode initialMode = Enemy01Mode.WALK;

    // 各モードごとのカスタマイズ用パラメータ
    [Header("Movement Parameters")]
    [SerializeField] private float walkRange = 2.0f;      // 歩く範囲(ゲーム開始時のスポーン位置を起点)
    [SerializeField] private float visualRange = 5.0f;    // プレイヤーを視認する範囲
    [SerializeField] private float walkSpeed = 1.0f;      // 歩く速度
    [SerializeField] private float rushSpeed = 2.0f;      // 突進速度
    
    // 他のフィールド
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

        // 体力０になったらモード変更
        if (status.GetHp() <= 0)
        {
            curMode = Enemy01Mode.DIE;
        }

        // プレイヤーが倒れたら歩きモードへ
        if (playerObj.GetComponent<PlayerStatus>().GetCurHp() <= 0 &&
            curMode != Enemy01Mode.WALK)
        {
            animator.SetBool("isAttack", false);
            curMode = Enemy01Mode.PLAYER_DIE;
        }

        // 攻撃が当たってノックバック処理してないとき
        if (scissors.GetComponent<AttackContoroll>().GethitFlg() && !isStart)
        {
            isStart = true;
            preMode = curMode;
            curMode = Enemy01Mode.KNOCK;
        }

        // モードごとに行動パターンを変える
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

    // ノックバック処理
    private void KnockBack()
    {
        switch (Step)
        {
            // プレイヤー仰け反る（ヒットストップ？）
            case 0:
                // 自分の位置と接触したオブジェクトの位置を計算して
                // 距離と方向を出して正規化
                Vector3 distination = new Vector3((this.transform.position.x - player.transform.position.x), 0, 0).normalized;

                // ノックバックアニメが再生されている間
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Knock"))
                {
                    // ノックバック
                    pos.x += distination.x * Time.deltaTime;
                }
                // 再生されているアニメが終わったら
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                {
                    // 移動完了したら次のステップへ
                    Step++;
                }
                break;

            case 1:

                // 処理順を最初に戻す
                Step = 0;
                // ノックバック前のモードに戻す
                curMode = preMode;
                // ノックバックアニメ終了
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
