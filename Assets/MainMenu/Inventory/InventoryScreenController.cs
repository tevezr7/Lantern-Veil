using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InventoryScreenController : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private CanvasGroup canvasGroup;   
    [SerializeField] private GameObject root;          
    [SerializeField] private Image potionIcon;          
    [SerializeField] private TMP_Text potionCountText;  
    [SerializeField] private Sprite potionSprite;      
    [SerializeField] private PotionInventory potionInventory;

    [Header("Behavior")]
    [SerializeField] private bool pauseOnOpen = true;
    [SerializeField] private KeyCode fallbackKey = KeyCode.B; 
    [SerializeField] private float fade = 0.15f;

    bool isOpen;

    void Awake()
    {
        if (!root) root = gameObject;
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();

        if (!potionInventory)
        {
#if UNITY_2023_1_OR_NEWER
            potionInventory = Object.FindAnyObjectByType<PotionInventory>();
#else
            potionInventory = FindObjectOfType<PotionInventory>();
#endif
        }

       
        if (potionIcon && potionSprite) potionIcon.sprite = potionSprite;

       
        root.SetActive(false);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

       
        if (potionInventory != null)
            potionInventory.OnPotionChanged += OnPotionChanged;
    }

    void OnDestroy()
    {
        if (potionInventory != null)
            potionInventory.OnPotionChanged -= OnPotionChanged;
    }

    void Update()
    {
        
        if (isOpen)
        {
            
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Close();
               
                MarkEscConsumed();
                return;
            }
         
            if (Keyboard.current == null && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
                MarkEscConsumed();
                return;
            }
        }

        
        if (Keyboard.current == null)
        {
            
            if (Input.GetKeyDown(fallbackKey)) Toggle();
        }
        else
        {
           
            if (Keyboard.current.bKey.wasPressedThisFrame) Toggle();
        }

        
        float target = isOpen ? 1f : 0f;
        if (canvasGroup)
            canvasGroup.alpha = Mathf.MoveTowards(
                canvasGroup.alpha, target,
                Time.unscaledDeltaTime / Mathf.Max(0.0001f, fade)
            );
    }

    static float _escConsumedUntil = -1f;
    static void MarkEscConsumed()
    {
        _escConsumedUntil = Time.unscaledTime + 0.05f; 
    }
    public static bool EscRecentlyConsumed =>
        Time.unscaledTime <= _escConsumedUntil;


    void OnPotionChanged(int count)
    {
        if (potionCountText) potionCountText.text = "x" + count;
    }

    public void Toggle()
    {
        

        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        
        if (potionCountText && potionInventory)
            potionCountText.text = "x" + potionInventory.potion_counter;

        root.SetActive(true);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (pauseOnOpen) Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (pauseOnOpen) Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        StartCoroutine(DeactivateWhenInvisible());
    }

    System.Collections.IEnumerator DeactivateWhenInvisible()
    {
      
        while (canvasGroup && canvasGroup.alpha > 0.001f)
            yield return null;

        if (root) root.SetActive(false);
    }

    
    public void OnInventory(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;
        Toggle();
    }
}
