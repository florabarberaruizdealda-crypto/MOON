using UnityEngine;
using UnityEngine.InputSystem;
public class Moon4 : MonoBehaviour
{
    public float velocidad = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movimiento;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.dKey.isPressed) moveX = 1f;
        else if (Keyboard.current.aKey.isPressed) moveX = -1f;

        if (Keyboard.current.wKey.isPressed) moveY = 1f;
        else if (Keyboard.current.sKey.isPressed) moveY = -1f;

        movimiento = new Vector2(moveX, moveY).normalized;

        animator.SetBool("Mov", movimiento != Vector2.zero);
        animator.SetFloat("X", moveX);
        animator.SetFloat("Y", moveY);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movimiento * velocidad * Time.fixedDeltaTime);
    }
}
