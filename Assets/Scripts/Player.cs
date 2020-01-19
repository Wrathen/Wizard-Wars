using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Vector2 currentSpeed;
    private Vector2 maxSpeed;
    private Vector2 velocity;
    private float dampening = 0.975f;
    public byte id = 0;
    public int selectedSpellIndex = 0;

    private int maxHealth;
    private int health;
    private float lastX = -9999;
    private float lastY = -9999;

    void Start()
    {
        currentSpeed = Vector2.zero;
        maxSpeed = new Vector2(5, 5);
        velocity = Vector2.zero;
        maxHealth = 100;
        health = maxHealth;
    }
    public void GotHitBy(LoginScreen.EntityTypes entity, byte entityID)
    {
        LoginScreen.GetHit(id, entity, entityID);
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        if ((id == 1) == LoginScreen._Instance.game.iAmRed)
        {
            GameObject.Find("health").GetComponent<Text>().text = "Health: " + health + "/" + maxHealth;
            if (health < 1) LoginScreen.Die();
        }
    }
    public void ChangeVelocity(Vector2 newVelocity)
    {
        velocity = newVelocity;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        string target = other.transform.name;
        if (target == "Left_Wall" || target == "Right_Wall")
        {
            currentSpeed.x = -currentSpeed.x * 1.5f;
            velocity.x = -velocity.x * 1.5f;
        } else if (target == "Top_Wall" || target == "Bottom_Wall" || target == "Middle_Wall") {
            currentSpeed.y = -currentSpeed.y * 1.5f;
            velocity.y = -velocity.y * 1.5f;
        }
    }


    public void Move()
    {
        if (velocity != Vector2.zero)
        {
            Vector2 timeBasedVelocity = velocity * Time.deltaTime;
            if (Mathf.Abs(currentSpeed.x) < maxSpeed.x) currentSpeed.x += timeBasedVelocity.x;
            if (Mathf.Abs(currentSpeed.y) < maxSpeed.y) currentSpeed.y += timeBasedVelocity.y;
            velocity -= timeBasedVelocity;
            if (Mathf.Abs(velocity.x) < 0.05f) velocity.x = 0;
            if (Mathf.Abs(velocity.y) < 0.05f) velocity.y = 0;
        }

        if (currentSpeed != Vector2.zero) transform.Translate(currentSpeed * Time.deltaTime);
        currentSpeed *= dampening;
        if (currentSpeed.magnitude < 0.1f) currentSpeed = Vector2.zero;

        if (lastX != transform.position.x || lastY != transform.position.y)
        {
            if (currentSpeed == Vector2.zero) return;
            LoginScreen.SendPosition(transform.position.x, transform.position.y);
            lastX = transform.position.x;
            lastY = transform.position.y;
        }
    }
}