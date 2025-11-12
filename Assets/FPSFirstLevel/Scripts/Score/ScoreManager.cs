using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona la puntuación del jugador durante todo el juego
/// Persiste entre escenas usando DontDestroyOnLoad
/// </summary>
public class ScoreManager : MonoBehaviour {
    public static ScoreManager Instance;

    [Header("Score Settings")]
    [Tooltip("Multiplicador de puntos por daño a enemigos")]
    public float damageScoreMultiplier = 1f;
    [Tooltip("Multiplicador de puntos perdidos por recibir daño")]
    public float damagePenaltyMultiplier = 0.5f;
    [Tooltip("Puntuación mínima (no puede bajar de aquí)")]
    public int minimumScore = 0;

    [Header("Level Tracking")]
    [Tooltip("Número total de niveles en el juego")]
    public int totalLevels = 3;

    // Puntuación actual del nivel
    private int currentLevelScore = 0;
    
    // Puntuación de cada nivel (índice 0 = nivel 1)
    private int[] levelScores;
    
    // Nivel actual (1, 2, 3)
    private int currentLevel = 1;
    
    // Estadísticas
    private int totalDamageDealt = 0;
    private int totalDamageTaken = 0;
    private int enemiesKilled = 0;

    void Awake() {
        // Singleton pattern con persistencia
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            levelScores = new int[totalLevels];
            Debug.Log("[ScoreManager] Sistema de puntuación inicializado");
        } else {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Añade puntos por hacer daño a enemigos
    /// </summary>
    public void AddDamageScore(float damage) {
        int points = Mathf.RoundToInt(damage * damageScoreMultiplier);
        currentLevelScore += points;
        totalDamageDealt += Mathf.RoundToInt(damage);
        
        // CORREGIDO: Usar Debug.LogFormat para argumentos
        Debug.LogFormat("[ScoreManager] +{0} puntos por daño. Score actual: {1}", points, currentLevelScore);
        
        // Notificar al UI
        ScoreUI scoreUI = FindObjectOfType<ScoreUI>();
        if (scoreUI) {
            scoreUI.UpdateScore(currentLevelScore);
            scoreUI.ShowScorePopup(points, true);
        }
    }

    /// <summary>
    /// Resta puntos por recibir daño
    /// </summary>
    public void SubtractDamageScore(float damage) {
        int points = Mathf.RoundToInt(damage * damagePenaltyMultiplier);
        currentLevelScore -= points;
        currentLevelScore = Mathf.Max(currentLevelScore, minimumScore);
        totalDamageTaken += Mathf.RoundToInt(damage);
        
        // CORREGIDO: Usar Debug.LogFormat para argumentos
        Debug.LogFormat("[ScoreManager] -{0} puntos por recibir daño. Score actual: {1}", points, currentLevelScore);
        
        // Notificar al UI
        ScoreUI scoreUI = FindObjectOfType<ScoreUI>();
        if (scoreUI) {
            scoreUI.UpdateScore(currentLevelScore);
            scoreUI.ShowScorePopup(points, false);
        }
    }

    /// <summary>
    /// Añade puntos bonus por matar un enemigo
    /// </summary>
    public void AddEnemyKillBonus(int bonus = 50) {
        currentLevelScore += bonus;
        enemiesKilled++;
        
        // CORREGIDO: Usar Debug.LogFormat para argumentos
        Debug.LogFormat("[ScoreManager] +{0} puntos BONUS por muerte. Score: {1}", bonus, currentLevelScore);
        
        ScoreUI scoreUI = FindObjectOfType<ScoreUI>();
        if (scoreUI) {
            scoreUI.UpdateScore(currentLevelScore);
            scoreUI.ShowScorePopup(bonus, true, "KILL!");
        }
    }

    /// <summary>
    /// Guarda la puntuación del nivel actual y prepara el siguiente
    /// </summary>
    public void CompleteLevel() {
        if (currentLevel <= totalLevels) {
            levelScores[currentLevel - 1] = currentLevelScore;
            // CORREGIDO: Usar Debug.LogFormat para argumentos
            Debug.LogFormat("[ScoreManager] Nivel {0} completado con {1} puntos", currentLevel, currentLevelScore);
        }
    }

    /// <summary>
    /// Avanza al siguiente nivel
    /// </summary>
    public void NextLevel() {
        CompleteLevel();
        currentLevel++;
        currentLevelScore = 0;
        
        // CORREGIDO: Usar Debug.LogFormat para argumentos
        Debug.LogFormat("[ScoreManager] Avanzando al nivel {0}", currentLevel);
    }

    /// <summary>
    /// Reinicia el nivel actual
    /// </summary>
    public void RestartLevel() {
        currentLevelScore = 0;
        // CORREGIDO: Usar Debug.LogFormat para argumentos
        Debug.LogFormat("[ScoreManager] Nivel {0} reiniciado", currentLevel);
    }

    /// <summary>
    /// Reinicia todo el juego
    /// </summary>
    public void ResetGame() {
        currentLevel = 1;
        currentLevelScore = 0;
        levelScores = new int[totalLevels];
        totalDamageDealt = 0;
        totalDamageTaken = 0;
        enemiesKilled = 0;
        
        Debug.Log("[ScoreManager] Juego reiniciado completamente");
    }

    // Getters
    public int GetCurrentLevelScore() => currentLevelScore;
    public int GetLevelScore(int level) => (level > 0 && level <= totalLevels) ? levelScores[level - 1] : 0;
    public int GetTotalScore() {
        int total = currentLevelScore;
        for (int i = 0; i < levelScores.Length; i++) {
            total += levelScores[i];
        }
        return total;
    }
    public int GetCurrentLevel() => currentLevel;
    public int GetTotalLevels() => totalLevels;
    public int GetEnemiesKilled() => enemiesKilled;
    public int GetTotalDamageDealt() => totalDamageDealt;
    public int GetTotalDamageTaken() => totalDamageTaken;
    public int[] GetAllLevelScores() => levelScores;

    /// <summary>
    /// Verifica si es el último nivel
    /// </summary>
    public bool IsLastLevel() {
        return currentLevel >= totalLevels;
    }
}