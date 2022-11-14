using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    private void Update()
    {
        Vector2 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit)
        {
            if (Input.GetMouseButtonDown(0))
            {
                IClickable objectHit = hit.collider.GetComponent<IClickable>();
                objectHit.OnPointerDown();
            }
            
        }
        
    }
}
