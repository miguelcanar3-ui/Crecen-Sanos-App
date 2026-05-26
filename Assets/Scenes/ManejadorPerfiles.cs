using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class ManejadorPerfiles : MonoBehaviour
{
    public static Mascota mascotaActiva;

    [Header("UI Registro")]
    public TMP_InputField inputNombre;
    public TMP_Dropdown dropdownAnos, dropdownMeses, dropdownSemanas;
    public Image cuadroFotoRegistro;
    public Sprite fotoPredeterminadaRegistro; // <--- NUEVO: Aquí guardaremos la huellita
    public GameObject panelRegistro, panelPerfiles, panelAdvertencia;

    [Header("UI Vacunación PERROS")]
    public GameObject panelVacunasPerro;
    public TextMeshProUGUI textoNombrePerro;
    public Image imagenPerfilPerro;

    [Header("UI Vacunación GATOS")]
    public GameObject panelVacunasGato;
    public TextMeshProUGUI textoNombreGato;
    public Image imagenPerfilGato;

    [Header("UI Edición (Universal)")]
    public GameObject panelEdicion;
    public TextMeshProUGUI textoTituloEdicion;
    public TMP_InputField inputNombreEdicion;
    public Image imagenPerfilEdicion;
    public TextMeshProUGUI textoEdadEdicion;
    private string rutaFotoEdicion;

    [Header("Sistema de Perfiles")]
    public GameObject prefabPerfil;
    public Transform contenedorPerfiles;
    public GameObject panelConfirmacion;

    private Mascota mascotaAEliminar;
    private string especieSeleccionada = "";
    private string rutaFotoActual = "";
    private List<Mascota> todasLasMascotas = new List<Mascota>();

    void Start() { CargarMascotas(); }

    public void AbrirPanelRegistro()
    {
        mascotaActiva = null;
        inputNombre.text = "";
        rutaFotoActual = "";
        especieSeleccionada = ""; // Resetea la elección de perro/gato internamente

        // --- AQUÍ OCURRE LA MAGIA: Volvemos a poner la huellita ---
        if (cuadroFotoRegistro != null && fotoPredeterminadaRegistro != null)
        {
            cuadroFotoRegistro.sprite = fotoPredeterminadaRegistro;
        }

        panelRegistro.SetActive(true);
        panelRegistro.transform.SetAsLastSibling();
    }

    public void AbrirPanelEdicion()
    {
        if (mascotaActiva == null) return;
        panelEdicion.SetActive(true);
        panelEdicion.transform.SetAsLastSibling();

        if (textoTituloEdicion != null) textoTituloEdicion.text = mascotaActiva.nombre;
        if (textoEdadEdicion != null) textoEdadEdicion.text = mascotaActiva.edad;

        inputNombreEdicion.text = mascotaActiva.nombre;
        rutaFotoEdicion = mascotaActiva.rutaFoto;

        if (!string.IsNullOrEmpty(rutaFotoEdicion))
        {
            CargarImagenEnEditor(rutaFotoEdicion);
        }
    }

    public void SeleccionarPerro() { especieSeleccionada = "Perro"; }
    public void SeleccionarGato() { especieSeleccionada = "Gato"; }

    public void AbrirGaleria() { NativeGallery.GetImageFromGallery((path) => { CargarImagenEnCuadro(path); }, "Foto", "image/*"); }
    public void AbrirCamara() { NativeCamera.TakePicture((path) => { CargarImagenEnCuadro(path); }, 512); }

    private void CargarImagenEnCuadro(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        Texture2D texture = NativeGallery.LoadImageAtPath(path, 512);
        if (texture != null)
        {
            cuadroFotoRegistro.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            rutaFotoActual = path;
        }
    }

    public void GuardarYContinuar()
    {
        if (string.IsNullOrEmpty(inputNombre.text) || especieSeleccionada == "")
        {
            if (panelAdvertencia != null)
            {
                panelAdvertencia.SetActive(true);
                panelAdvertencia.transform.SetAsLastSibling();
            }
            return;
        }
        Mascota nueva = new Mascota();
        nueva.nombre = inputNombre.text;
        nueva.especie = especieSeleccionada;

        nueva.edad = dropdownAnos.options[dropdownAnos.value].text + ", " +
                     dropdownMeses.options[dropdownMeses.value].text + ", " +
                     dropdownSemanas.options[dropdownSemanas.value].text;

        nueva.rutaFoto = rutaFotoActual;

        todasLasMascotas.Add(nueva);
        GuardarEnMemoria();
        ActualizarTarjetasVisuales();

        inputNombre.text = "";
        panelRegistro.SetActive(false);
        IrAVacunas(nueva);
    }

    public void CerrarAdvertencia() { if (panelAdvertencia != null) panelAdvertencia.SetActive(false); }

    public void AbrirGaleriaEdicion() { NativeGallery.GetImageFromGallery((path) => { CargarImagenEnEditor(path); }, "Foto", "image/*"); }
    public void AbrirCamaraEdicion() { NativeCamera.TakePicture((path) => { CargarImagenEnEditor(path); }, 512); }

    private void CargarImagenEnEditor(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        Texture2D texture = NativeGallery.LoadImageAtPath(path, 512);
        if (texture != null)
        {
            imagenPerfilEdicion.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            rutaFotoEdicion = path;
        }
    }

    public void GuardarCambiosEdicion()
    {
        if (string.IsNullOrEmpty(inputNombreEdicion.text)) return;
        mascotaActiva.nombre = inputNombreEdicion.text;
        mascotaActiva.rutaFoto = rutaFotoEdicion;
        GuardarEnMemoria();
        ActualizarTarjetasVisuales();
        IrAVacunas(mascotaActiva);
        panelEdicion.SetActive(false);
    }

    public void CancelarEdicion() { panelEdicion.SetActive(false); }

    public void IrAVacunas(Mascota m)
    {
        mascotaActiva = m;
        panelPerfiles.SetActive(false);

        if (m.especie == "Perro")
        {
            if (textoNombrePerro != null) textoNombrePerro.text = m.nombre;
            if (imagenPerfilPerro != null && !string.IsNullOrEmpty(m.rutaFoto))
            {
                Texture2D tex = NativeGallery.LoadImageAtPath(m.rutaFoto, 256);
                if (tex) imagenPerfilPerro.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            panelVacunasPerro.SetActive(true);
            panelVacunasPerro.transform.SetAsLastSibling();
            panelVacunasGato.SetActive(false);
        }
        else
        {
            if (textoNombreGato != null) textoNombreGato.text = m.nombre;
            if (imagenPerfilGato != null && !string.IsNullOrEmpty(m.rutaFoto))
            {
                Texture2D tex = NativeGallery.LoadImageAtPath(m.rutaFoto, 256);
                if (tex) imagenPerfilGato.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            panelVacunasGato.SetActive(true);
            panelVacunasGato.transform.SetAsLastSibling();
            panelVacunasPerro.SetActive(false);
        }
    }

    void ActualizarTarjetasVisuales()
    {
        foreach (Transform child in contenedorPerfiles) Destroy(child.gameObject);
        foreach (Mascota m in todasLasMascotas) CrearTarjetaPerfil(m);
    }

    void CrearTarjetaPerfil(Mascota m)
    {
        GameObject tarjeta = Instantiate(prefabPerfil, contenedorPerfiles);

        Transform txtN = tarjeta.transform.Find("TextoNombre");
        if (txtN != null) txtN.GetComponent<TextMeshProUGUI>().text = m.nombre;

        Transform txtE = tarjeta.transform.Find("TextoEdad");
        if (txtE != null) txtE.GetComponent<TextMeshProUGUI>().text = m.edad;

        Transform imgF = tarjeta.transform.Find("Foto");
        if (imgF && !string.IsNullOrEmpty(m.rutaFoto))
        {
            Texture2D tex = NativeGallery.LoadImageAtPath(m.rutaFoto, 256);
            if (tex) imgF.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        Button btnTarjeta = tarjeta.GetComponent<Button>();
        if (btnTarjeta != null) btnTarjeta.onClick.AddListener(() => IrAVacunas(m));

        Transform transformBotonX = tarjeta.transform.Find("BotonX");
        if (transformBotonX != null)
        {
            Button btnX = transformBotonX.GetComponent<Button>();
            if (btnX != null)
            {
                btnX.onClick.AddListener(() => {
                    mascotaAEliminar = m;
                    if (panelConfirmacion != null)
                    {
                        panelConfirmacion.SetActive(true);
                        panelConfirmacion.transform.SetAsLastSibling();
                    }
                });
            }
        }
    }

    public void ConfirmarEliminacion()
    {
        todasLasMascotas.Remove(mascotaAEliminar);
        GuardarEnMemoria();
        ActualizarTarjetasVisuales();
        panelConfirmacion.SetActive(false);
        panelVacunasPerro.SetActive(false); panelVacunasGato.SetActive(false);
        panelPerfiles.SetActive(true);
        panelPerfiles.transform.SetAsLastSibling();
    }

    public void CancelarEliminacion() { if (panelConfirmacion != null) panelConfirmacion.SetActive(false); }

    void GuardarEnMemoria()
    {
        ListaMascotas wrapper = new ListaMascotas(); wrapper.lista = todasLasMascotas;
        PlayerPrefs.SetString("DataMascotasFinal", JsonUtility.ToJson(wrapper));
        PlayerPrefs.Save();
    }

    void CargarMascotas()
    {
        string data = PlayerPrefs.GetString("DataMascotasFinal", "");
        if (!string.IsNullOrEmpty(data))
        {
            todasLasMascotas = JsonUtility.FromJson<ListaMascotas>(data).lista;
            ActualizarTarjetasVisuales();
        }
    }
}

[Serializable]
public class Mascota { public string nombre, especie, edad, rutaFoto; }

[Serializable]
public class ListaMascotas { public List<Mascota> lista = new List<Mascota>(); }