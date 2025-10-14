using UnityEditor;

[CustomEditor(typeof(Interactable),true)]
public class InteractableEditor : Editor
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnInspectorGUI()
    {
        Interactable interactable = (Interactable)target;
        if (target.GetType() == typeof(EventOnlyInteractable))
        {
            interactable.prompt = EditorGUILayout.TextField("Prompt Message", interactable.prompt);
            EditorGUILayout.HelpBox("This interactable only uses unity events. No custom interaction logic can be added.", MessageType.Info);
            if(interactable.GetComponent<InteractionEvent>() == null) // If the InteractableEvent component doesn't exist, add it
            { 
                interactable.useEvents = true; // Force useEvents to be true
                interactable.gameObject.AddComponent<InteractionEvent>(); // Add the component
            }
        }
        base.OnInspectorGUI();
        if (interactable.useEvents) 
        {
            if(interactable.GetComponent<InteractionEvent>() == null) // If the InteractionEvent component doesn't exist, add it
            { 
                    interactable.gameObject.AddComponent<InteractionEvent>(); // Add the component
            }
                
        }
        else
        {
            if (interactable.GetComponent<InteractionEvent>() != null) 
                DestroyImmediate(interactable.GetComponent<InteractionEvent>()); // DestroyImmediate is used in editor scripts to immediately remove the component
        }
    }
}
