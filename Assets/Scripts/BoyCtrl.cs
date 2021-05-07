using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoyCtrl : MonoBehaviour
{
     Animator animator;
     Rigidbody2D rigibody1;
     AudioSource audiosource;
    [SerializeField] AudioClip jumpAudioClip, landAudioClip;
    [SerializeField] float maxspeed = 3;
    [SerializeField] float jumpForce = 8;
    [SerializeField] LayerMask whatIsGround;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rigibody1 = GetComponent<Rigidbody2D>();
        audiosource = GetComponent<AudioSource>();
    }

    //判斷腳色狀態
    float speed;
    bool isLand;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            speed = Mathf.Lerp(speed , 1 , 0.33f);
        }
        else
        {
            speed = Mathf.Lerp(speed,0.5f,0.1f);
        }

        bool isJump = Input.GetMouseButtonDown(1);

        if (isJump && animator.GetBool("Onground"))
        {
            isLand = false;
            audiosource.clip = jumpAudioClip;
            audiosource.Play();
        }
        else
        {

            if (animator.GetBool("Onground"))
            {
                if (!isLand)
                {
                    isLand = true;
                    audiosource.clip = landAudioClip;
                    audiosource.Play();
                }
            }
        }
        Move(speed, isJump);

        
    }

    private void Move(float speed , bool jump)
    {
        //一直往右邊跑的設計
        Vector2 move = Vector2.right * maxspeed * speed;
        move.y = rigibody1.velocity.y;
        //rigidbody.velocity = Vector2.right*maxspeed*speed;
        rigibody1.velocity = move;
        animator.SetFloat("Speed", speed);

        //觀察是否在地面
        bool isOnground = false;
        Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(transform.position, 0.02f, whatIsGround);
        if (collider2Ds.Length > 0)
        {
            isOnground = true;
            
        }
        animator.SetBool("Onground", isOnground);
            
        //針對跳躍的設計
        if (jump && isOnground)
        {
            rigibody1.AddForce(Vector2.up*jumpForce,ForceMode2D.Impulse);
            
        }
        animator.SetFloat("vSpeed", rigibody1.velocity.y);
    }
}
