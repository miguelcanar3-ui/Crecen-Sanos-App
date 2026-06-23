using UnityEngine;

public class ZoomImagenTouch : MonoBehaviour
{
    public float velocidadZoomTactil = 0.01f;
    public float velocidadZoomRaton = 0.5f;
    public float velocidadPaneo = 1f; // Nueva variable: Velocidad para arrastrar la foto

    private Vector3 ultimaPosicionRaton;

    // Esto centra la imagen automáticamente cada vez que abres el visor
    void OnEnable()
    {
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
    }

    void Update()
    {
        // --- 1. ZOOM PARA CELULAR (2 DEDOS) ---
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;

            float diferencia = currentMagnitude - prevMagnitude;
            AplicarZoom(diferencia * velocidadZoomTactil);
        }
        // --- 2. PANEO PARA CELULAR (1 DEDO ARRASTRANDO) ---
        else if (Input.touchCount == 1 && transform.localScale.x > 1.01f)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                transform.localPosition += (Vector3)touch.deltaPosition * velocidadPaneo;
            }
        }
        // --- 3. ZOOM PARA PC (RUEDA DEL RATÓN) ---
        else if (Input.mouseScrollDelta.y != 0)
        {
            AplicarZoom(Input.mouseScrollDelta.y * velocidadZoomRaton);
        }
        // --- 4. PANEO PARA PC (CLIC IZQUIERDO SOSTENIDO) ---
        else if (Input.GetMouseButton(0) && Input.touchCount == 0 && transform.localScale.x > 1.01f)
        {
            Vector3 delta = Input.mousePosition - ultimaPosicionRaton;
            transform.localPosition += delta * velocidadPaneo;
        }

        // Actualizamos la posición del ratón para el siguiente frame
        ultimaPosicionRaton = Input.mousePosition;
    }

    private void AplicarZoom(float incremento)
    {
        Vector3 nuevaEscala = transform.localScale + Vector3.one * incremento;

        // Limitamos el zoom entre 1 (tamaño original) y 5 (zoom máximo)
        nuevaEscala.x = Mathf.Clamp(nuevaEscala.x, 1f, 5f);
        nuevaEscala.y = Mathf.Clamp(nuevaEscala.y, 1f, 5f);
        nuevaEscala.z = 1f;

        transform.localScale = nuevaEscala;

        // Si el usuario aleja la imagen a su tamaño original, la centramos automáticamente
        if (transform.localScale.x <= 1.01f)
        {
            transform.localPosition = Vector3.zero;
        }
    }
}