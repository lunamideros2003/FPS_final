using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script para puertas que se desbloquean al recopilar objetos
/// </summary>
public class DoorUnlock : MonoBehaviour {
    [Header("Door Settings")]
    [Tooltip("Nombre de la escena a cargar al interactuar")]
    public string nextSceneName = "LevelTwo";
    [Tooltip("Distancia a la que el jugador puede interactuar")]
    public float interactionRange = 3f;
    [Tooltip("Mensaje que se muestra cuando falta desbloquear")]
    public string lockedMessage = "Necesitas recopilar todos los objetos para abrir esta puerta";
    [Tooltip("Mensaje que se muestra cuando está desbloqueada")]
    public string unlockedMessage = "Presiona E para continuar";

    [Header("Visual Feedback")]
    [Tooltip("Material cuando está bloqueada")]
    public Material lockedMaterial;
    [Tooltip("Material cuando está desbloqueada")]
    public Material unlockedMaterial;
    [Tooltip("Efecto de luz (opcional)")]
    public Light doorLight;

    private bool isUnlocked = false;
    private bool isPlayerNear = false;
    private GameObject player;
    private Renderer doorRenderer;

    void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
        doorRenderer = GetComponent<Renderer>();

        // Aplicar material bloqueado inicialmente
        if (doorRenderer && lockedMaterial) {
            doorRenderer.material = lockedMaterial;
        }

        // Apagar la luz si existe
        if (doorLight) {
            doorLight.enabled = false;
        }

        Debug.Log("[DoorUnlock] Puerta creada - Estado: BLOQUEADA");
    }

    void Update() {
        if (!player) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        isPlayerNear = distance <= interactionRange;

        // Mostrar mensaje si está cerca
        if (isPlayerNear) {
            if (isUnlocked) {
                if (Input.GetKeyDown(KeyCode.E)) {
                    InteractWithDoor();
                }
            }
        }
    }

    /// <summary>
    /// Desbloquea la puerta
    /// </summary>
    public void Unlock() {
        if (isUnlocked) return;

        isUnlocked = true;
        Debug.Log("[DoorUnlock] Puerta desbloqueada");

        // Cambiar material
        if (doorRenderer && unlockedMaterial) {
            doorRenderer.material = unlockedMaterial;
        }

        // Encender la luz
        if (doorLight) {
            doorLight.enabled = true;
            doorLight.color = Color.green;
        }

        // Reproducir animación de desbloqueo (opcional)
        PlayUnlockAnimation();
    }

    /// <summary>
    /// Interactuar con la puerta
    /// </summary>
    void InteractWithDoor() {
        if (!isUnlocked) return;

        Debug.Log("[DoorUnlock] Puerta abierta - Cargando escena: " + nextSceneName);
        Time.timeScale = 1f; // Asegurarse de despauser
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// Animar el desbloqueo (opcional)
    /// </summary>
    void PlayUnlockAnimation() {
        // Aquí puedes añadir animaciones de apertura, sonidos, etc.
        // Por ahora solo cambios visuales
        Debug.Log("[DoorUnlock] Puerta animada");
    }

    void OnDrawGizmosSelected() {
        // Mostrar el rango de interacción en el editor
        Gizmos.color = isUnlocked ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    void OnGUI() {
        if (!isPlayerNear) return;

        int w = Screen.width;
        int h = Screen.height;
        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(w / 2 - 200, h - 100, 400, 80);

        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 20;
        style.normal.textColor = isUnlocked ? Color.green : Color.red;

        string message = isUnlocked ? unlockedMessage : lockedMessage;
        GUI.Label(rect, message, style);
    }
}