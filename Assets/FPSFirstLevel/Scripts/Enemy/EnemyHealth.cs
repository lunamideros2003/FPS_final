using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;

public class EnemyHealth : MonoBehaviour {
    [Header("Health Settings")]
    [Tooltip("Maximum health of enemy")]
    public float maxHealth = 50f;
    [Tooltip("Current health")]
    public float currentHealth;

    [Header("Health Bar UI (World Space)")]
    [Tooltip("Canvas with health bar (optional)")]
    public Canvas healthBarCanvas;
    [Tooltip("Health bar image")]
    public Image healthBarImage;
    [Tooltip("Hide health bar when full")]
    public bool hideWhenFull = true;
    [Tooltip("Time to hide health bar after full")]
    public float hideDelay = 2f;

    [Header("Death Settings")]
    [Tooltip("Time before destroying the body")]
    public float destroyDelay = 5f;
    [Tooltip("Drops on death")]
    public GameObject[] dropItems;
    [Tooltip("Drop chance (0-1)")]
    public float dropChance = 0.5f;

    [Header("Audio")]
    [Tooltip("Sound when hit")]
    public AudioSource hitSound;
    [Tooltip("Sound when dying")]
    public AudioSource deathSound;

    private bool isDead = false;
    private EnemyAI enemyAI;
    private Animator animator;
    private NavMeshAgent agent;
    private float lastHitTime;

    void Awake() {
        currentHealth = maxHealth;
        enemyAI = GetComponent<EnemyAI>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (healthBarCanvas && hideWhenFull) {
            healthBarCanvas.enabled = false;
        }

        Debug.Log("[EnemyHealth] " + gameObject.name + " inicializado con " + maxHealth + " de vida");
    }

    void Start() {
        if (currentHealth <= 0) {
            currentHealth = maxHealth;
            Debug.LogWarning("[EnemyHealth] " + gameObject.name + " tenía vida en 0, reiniciada a " + maxHealth);
        }
    }

    void Update() {
        // Hide health bar after delay when full
        if (healthBarCanvas && hideWhenFull && currentHealth >= maxHealth) {
            if (Time.time - lastHitTime > hideDelay) {
                healthBarCanvas.enabled = false;
            }
        }

        // Make health bar face camera
        if (healthBarCanvas && healthBarCanvas.enabled) {
            Camera mainCam = Camera.main;
            if (mainCam) {
                healthBarCanvas.transform.LookAt(mainCam.transform);
                healthBarCanvas.transform.Rotate(0, 180, 0);
            }
        }
    }

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection) {
        if (isDead) {
            Debug.Log(gameObject.name + " is already dead, ignoring damage");
            return;
        }

        Debug.Log(gameObject.name + " taking " + damage + " damage. Current health: " + currentHealth);

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        lastHitTime = Time.time;

        if (ScoreManager.Instance) {
            ScoreManager.Instance.AddDamageScore(damage);
        }   
        Debug.Log(gameObject.name + " health after damage: " + currentHealth);

        // Show health bar
        if (healthBarCanvas) {
            healthBarCanvas.enabled = true;
        }

        // Update health bar
        if (healthBarImage) {
            healthBarImage.fillAmount = currentHealth / maxHealth;
        }

        // Play hit sound
        if (hitSound) {
            hitSound.Play();
        }

        // Hit reaction animation
        if (animator && currentHealth > 0) {
            animator.SetTrigger("Hit");
        }

        // Alert AI
        if (enemyAI && !enemyAI.isAggro) {
            enemyAI.AlertEnemy(hitPoint);
        }

        if (currentHealth <= 0) {
            Die(hitDirection);
        }
    }

    void Die(Vector3 deathDirection) {
        if (isDead) return;

        isDead = true;
        Debug.Log(gameObject.name + " has died!");

        if (ScoreManager.Instance) {
           ScoreManager.Instance.AddEnemyKillBonus(100);
        }

        // 🔹 Play death sound
        if (deathSound) {
            deathSound.Play();
        }

        // 🔹 Disable AI and navigation
        if (enemyAI) enemyAI.enabled = false;
        if (agent) {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // 🔹 Play death animation
        if (animator) {
            animator.SetTrigger("Death");
            animator.SetFloat("Speed", 0);
        }

        // 🔹 Disable colliders to prevent physics glitches
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders) {
            col.enabled = false;
        }

        // 🔹 Stop any Rigidbody from falling
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) {
            rb.isKinematic = true;   // desactiva físicas
            rb.useGravity = false;   // evita caída
        }

        // 🔹 Asegura que el enemigo quede fijo en el suelo
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, pos.y + 0.05f, pos.z);

        // 🔹 Ocultar barra de vida
        if (healthBarCanvas) {
            healthBarCanvas.enabled = false;
        }

        // 🔹 Soltar loot (si aplica)
        DropLoot();

        // 🔹 Destruir después de unos segundos
        Destroy(gameObject, destroyDelay);
    }

    void DropLoot() {
        if (dropItems.Length == 0) return;
        if (Random.value > dropChance) return;

        GameObject itemToDrop = dropItems[Random.Range(0, dropItems.Length)];
        if (itemToDrop) {
            Vector3 dropPos = transform.position + Vector3.up;
            Instantiate(itemToDrop, dropPos, Quaternion.identity);
        }
    }

    public bool IsDead() {
        return isDead;
    }

    public float GetHealthPercentage() {
        return currentHealth / maxHealth;
    }
}
