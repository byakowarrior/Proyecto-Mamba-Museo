using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DebugRaycast : MonoBehaviour
{
    private void Update()
    {
        if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame) return;

        var data = new PointerEventData(EventSystem.current)
        {
            position = Pointer.current.position.ReadValue()
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        if (results.Count == 0) { Debug.Log("Raycast: no toca nada"); return; }

        string lista = "";
        foreach (var r in results) lista += r.gameObject.name + "  >  ";
        Debug.Log("Raycast (de arriba hacia abajo): " + lista);
    }
}