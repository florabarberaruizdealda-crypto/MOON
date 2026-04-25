using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Conectar con el script de la barra emocional al final para que pueda funcionar dependiendo de ella
//Crear los NPCS de cada tipo como prefabs con características 
public class NPCSpawner : MonoBehaviour
{
   //Configuracion general
    [SerializeField] private GameObject npcPrefabGood;
    [SerializeField] private GameObject npcPrefabNeutral;
    [SerializeField] private GameObject npcPrefabBad;
    [SerializeField] private int totalNPCsPerScene = 20; //mas o menos
    
    //Configuracion barra emocional 1: declarar si no se encuentra para notificar del fallo del script
    [SerializeField] private EmotionSystem emotionSystem;
    
    private Dictionary<string, GameObject> npcsSpawned = new Dictionary<string, GameObject>();
    
    void Awake()
    {
        // Referencia al sistema emocional si no se conecta por inspec tor
        if (emotionSystem == null)
        {
            // Intentar encontrar en la escena
            emotionSystem = FindObjectOfType<EmotionSystem>(); //Conexion con la barra emocional
        }
        
        if (npcPrefabGood == null || npcPrefabNeutral == null || npcPrefabBad == null)
        {
            Debug.LogWarning("NPCSpawner: Faltan prefabs de NPCs. Configura los slots en el inspector.");
        }
    }
    
    
    /// Funcion que establece la llamada a este script solo al entrar en una escena de exploración y dependiente de emotionValue
    
    public void SpawnNPCsBasedOnEmotion(float emotionValue) 
    {
        if (emotionSystem == null)
        {
            Debug.LogWarning("NPCSpawner: No se ha encontrado el EmotionSystem. Configúralo en el inspector.");
            return;
        }
        
        // Limpiar NPCs existentes
        ClearNPCs();
        
        // Calcular distribución según valor emocional
        int goodNPCs = 0;
        int neutralNPCs = 0;
        int badNPCs = 0;
        
        if (emotionValue >= 61 && emotionValue <= 100) // Éxtasis
        {
            // 50% buenos, 50% malos, sin neutros
            goodNPCs = (int)(totalNPCsPerScene * 0.50f);   // 50% de la escena
            badNPCs = (int)(totalNPCsPerScene * 0.50f);     // 50% de la escena (50% de 100%)
        }
        else if (emotionValue >= 31 && emotionValue <= 61) // Nivel medio
        {
            // 33% neutros, restante dividido entre buenos y malos
            neutralNPCs = (int)(totalNPCsPerScene * 0.33f);
            int remaining = totalNPCsPerScene - neutralNPCs;
            goodNPCs = remaining / 2;
            badNPCs = remaining - goodNPCs;
        }
        else // 0-30 (Nivel bajo)
        {
            // 60% malos, 40% restantes entre neutros y buenos
            badNPCs = (int)(totalNPCsPerScene * 0.7f);
            int remaining = totalNPCsPerScene - badNPCs;
            neutralNPCs = remaining / 3;
            goodNPCs = remaining - neutralNPCs;
        }
        
        // Ajustar para que sumen exactamente totalNPCsPerScene
        AdjustNPCCounts(goodNPCs, neutralNPCs, badNPCs);
        
        // Spawneamos los NPCs
        SpawnNPCs(goodNPCs, npcPrefabGood, "Good");
        SpawnNPCs(neutralNPCs, npcPrefabNeutral, "Neutral");
        SpawnNPCs(badNPCs, npcPrefabBad, "Bad");
        
        // Debug log
        Debug.Log($"[NPCSpawner] Emotion: {emotionValue:F0}% | Spawned: {goodNPCs} Good, {neutralNPCs} Neutral, {badNPCs} Bad (Total: {totalNPCsPerScene})");
    }
    

    /// Funciones que sirven para asegurar que el total es siempre el establecido en el totalNPCsPerScene

    private void AdjustNPCCounts(int good, int neutral, int bad)
    {
        int currentTotal = good + neutral + bad;
        if (currentTotal > totalNPCsPerScene)
        {
            // Reducir en exceso
            while (currentTotal > totalNPCsPerScene)
            {
                if (neutral > 0)
                {
                    neutral--;
                }
                else if (bad > good)
                {
                    bad--;
                }
                else
                {
                    good--;
                }
                currentTotal--;
            }
        }
        else if (currentTotal < totalNPCsPerScene)
        {
            // Aumentar hasta alcanzar total
            while (currentTotal < totalNPCsPerScene)
            {
                if (neutral > 0)
                {
                    neutral++;
                }
                else if (good < bad)
                {
                    good++;
                }
                else
                {
                    bad++;
                }
                currentTotal++;
            }
        }
    }
    
  
    /// Spawnea la cantidad especificada de NPCs
    
    private void SpawnNPCs(int count, GameObject prefab, string type)
    {
        if (prefab == null || count <= 0) return;
        
        for (int i = 0; i < count; i++)
        {
            GameObject instance = Instantiate(prefab);
            instance.name = $"NPC_{type}_{i}";
            
            // Guardar referencia
            if (npCsSpawned.ContainsKey(type))
            {
                npCsSpawned[type].Add(instance);
            }
            else
            {
                npCsSpawned[type] = new List<GameObject> { instance };
            }
            
            // Posicionar aleatoriamente (para evitar superposición)
            SetRandomPosition(instance);
            
            // Asignar propiedades según tipo
            if (prefab.CompareTag("Good"))
            {
                instance.GetComponent<NPCController>()?.SetHostile(false);
            }
            else if (prefab.CompareTag("Bad"))
            {
                instance.GetComponent<NPCController>()?.SetHostile(true);
            }
        }
    }
    
     
    /// Establece una posición aleatoria para el NPC: peremitida en todo el mapa
    
    private void SetRandomPosition(GameObject npc)
    {
        float randomX = Random.Range(-10f, 10f);
        float randomY = 0f;
        float randomZ = Random.Range(-10f, 10f);
        
        npc.transform.position = new Vector3(randomX, randomY, randomZ);

        //Limitada a una area especifica estableciendo unas coordenadas como "GetValidSpawnPosition"
       
        // npc.transform.position = GetValidSpawnPosition(npc.transform.position);
    }
    
  
    /// Limpia todos los NPCs existentes
    /// </summary>
    private void ClearNPCs()
    {
        foreach (var kvp in npCsSpawned)
        {
            foreach (var npc in kvp.Value)
            {
                if (npc != null)
                {
                    DestroyImmediate(npc);
                }
            }
        }
        npCsSpawned.Clear();
    }
    
    
    /// Método para que el EmotionSystem pueda notificar cambios
 
    public void OnEmotionChanged(float newEmotionValue)
    {
        // Solo regenerar NPCs si estamos en una nueva escena de exploración
        // Implementar lógica de detección de escena si es necesario
    }
}

/// Interface simple para el sistema emocional (si no existe, crearlo aparte)

public class //NombreScriptBarrraEmocionalEmotionSystem : MonoBehaviour
{
    public float // Funcion de Barra emocional qe imprime el emotionValue { get; set; } = 50f;
    public event System.Action<float> OnEmotionChanged;
    
    public void SetEmotion(float value)
    {
        emotionValue = Mathf.Clamp01(value) * 100f; // 0-100
        OnEmotionChanged?.Invoke(emotionValue);
        //Esto establece los intérvalos para que tanto la barra, el gestor del color y el creador de NPCs sepa los valores
        //de cada emocion
    }
    
    public float GetEmotion()
    {
        return emotionValue;
    }
}
```

``
