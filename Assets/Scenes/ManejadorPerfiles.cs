using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Notifications.Android;
using UnityEngine.Android;

public class ManejadorPerfiles : MonoBehaviour
{
    public static Mascota mascotaActiva;

    [Header("UI General y Debug")]
    public GameObject panelBotonesDebug;

    [Header("Control de Scrolls (Resets Automáticos)")]
    public ScrollRect scrollPerfiles;
    public ScrollRect scrollVacunasPerro;
    public ScrollRect scrollVacunasGato;

    [Header("UI Registro")]
    public TMP_InputField inputNombre;
    public TMP_InputField inputAnos, inputMeses, inputSemanas;
    public Image cuadroFotoRegistro;
    public Sprite fotoPredeterminadaRegistro;
    public GameObject panelRegistro, panelPerfiles, panelAdvertencia;
    public GameObject botonRegistrarDespuesUI;
    public GameObject botonAtrasRegistro; // <--- FLECHA DE VOLVER EN REGISTRO

    [Header("Modo Invitado")]
    public GameObject panelAvisoInvitado;
    public GameObject panelSeleccionInvitado;
    public Button[] botonesBloqueadosInvitado;
    public GameObject botonAtrasInvitadoPerro;
    public GameObject botonAtrasInvitadoGato;
    private bool modoInvitadoActivo = false;

    [Header("UI Bienvenida")]
    public GameObject panelBienvenida;
    public TextMeshProUGUI textoBienvenida;

    [Header("UI Aviso Automático Edad")]
    public GameObject panelAvisoEdadAutomatico;
    public TextMeshProUGUI textoAvisoEdadAutomatico;

    [Header("UI Vacunación PERROS")]
    public GameObject panelVacunasPerro;
    public TextMeshProUGUI textoNombrePerro;
    public Image imagenPerfilPerro;
    public Toggle[] togglesVacunasPerro;
    public GrupoAlertasVacuna[] alertasPorVacuna = new GrupoAlertasVacuna[7];
    public GameObject panelPreguntaTraqueo;
    public GameObject vacunaSegundaDosisTraqueo;
    public GameObject vacunaRefuerzoAnual;

    [Header("UI Vacunación GATOS")]
    public GameObject panelVacunasGato;
    public TextMeshProUGUI textoNombreGato;
    public Image imagenPerfilGato;
    public Toggle[] togglesVacunasGato;
    public GrupoAlertasVacuna[] alertasPorVacunaGato = new GrupoAlertasVacuna[6];
    public GameObject vacunaRefuerzoAnualGato;

    [Header("UI ALIMENTOS (Nombres y Fotos Duplicados)")]
    public TextMeshProUGUI textoNombrePerroAlimento;
    public Image imagenPerfilPerroAlimento;
    public TextMeshProUGUI textoNombreGatoAlimento;
    public Image imagenPerfilGatoAlimento;

    [Header("UI Confirmación Vacuna")]
    public GameObject panelConfirmacionVacuna;
    public TextMeshProUGUI textoConfirmacionVacuna;
    private int indiceVacunaPendiente = -1;
    private bool nuevoEstadoVacunaPendiente = false;

    [Header("Nuevos Paneles Informativos y Ajustes")]
    public GameObject panelPerrosMayores;
    public GameObject panelGatosMayores;
    public GameObject panelEnfermedadesListado;

    [Header("Barra Navegación Inferior (Tabs)")]
    public GameObject vistaAlimentacionPerro;
    public GameObject vistaAlimentacionGato;
    public Sprite iconoVacunaAzul, iconoVacunaGris;
    public Sprite iconoComidaAzul, iconoComidaGris;

    [Header("UI Recordatorios (Notificaciones)")]
    public GameObject panelRecordatorios;
    public GameObject tarjetaNotificacion;
    public Image imagenEstadoNotificacion;
    public GameObject textoSinNotificaciones;

    [Header("Iconos Campana (Exterior)")]
    public Image iconoCampanaPerro;
    public Image iconoCampanaGato;
    public Sprite spriteCampanaNormal;
    public Sprite spriteCampanaAlerta;

    [Header("Botones de Control Notificación")]
    public Button btnPausarNotificaciones;
    public Button btnReanudarNotificaciones;
    public Sprite spritePausarAzul;
    public Sprite spritePausarGris;
    public Sprite spriteReanudarAzul;
    public Sprite spriteReanudarGris;

    [Header("UI Edición (Universal)")]
    public GameObject panelEdicion;
    public TextMeshProUGUI textoTituloEdicion;
    public TMP_InputField inputNombreEdicion;
    public Image imagenPerfilEdicion;
    public TextMeshProUGUI textoEdadEdicion;
    private string rutaFotoEdicion;

    [Header("UI Carnet Unificado")]
    public GameObject panelCarnetUnificado;
    public Image imagenCarnetPrincipal;
    public TextMeshProUGUI textoFechaPrincipal;
    public GameObject botonEliminarPrincipal;
    public Transform contenedorHistorialCarnet;
    public GameObject prefabCarnetHistorial;
    public GameObject panelConfirmacionEliminarCarnet;
    public GameObject panelVisorFullscreen;
    public Image imagenVisorFullscreen;
    private int indiceCarnetAEliminar = -1;

    [Header("Sistema de Perfiles")]
    public GameObject prefabPerfil;
    public Transform contenedorPerfiles;
    public GameObject panelConfirmacion;

    private Mascota mascotaAEliminar;
    private string especieSeleccionada = "";
    private string rutaFotoActual = "";
    private List<Mascota> todasLasMascotas = new List<Mascota>();

    private bool cargandoVacunas = false;
    private int idNotificacionActual = 0;
    private int tempIndicePendiente = -1;
    private int tempEstadoAlerta = 0;

    void Start()
    {
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.orientation = ScreenOrientation.Portrait;

        CargarMascotas();
        if (panelPreguntaTraqueo != null) panelPreguntaTraqueo.SetActive(false);
        if (panelAvisoInvitado != null) panelAvisoInvitado.SetActive(false);
        if (panelAvisoEdadAutomatico != null) panelAvisoEdadAutomatico.SetActive(false);

        ConfigurarCanalNotificaciones();

#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
#endif
    }

    private void ResetearScrollMascotaActual()
    {
        if (mascotaActiva != null)
        {
            if (mascotaActiva.especie == "Perro" && scrollVacunasPerro != null) scrollVacunasPerro.verticalNormalizedPosition = 1f;
            else if (mascotaActiva.especie == "Gato" && scrollVacunasGato != null) scrollVacunasGato.verticalNormalizedPosition = 1f;
        }
    }

    public void TraerBotonesDebugAlFrente()
    {
        if (panelBotonesDebug != null) panelBotonesDebug.transform.SetAsLastSibling();
    }

    public void BotonContinuarInicio()
    {
        if (todasLasMascotas.Count > 0)
        {
            if (panelPerfiles != null) { panelPerfiles.SetActive(true); panelPerfiles.transform.SetAsLastSibling(); }
        }
        else { AbrirPanelRegistro(); }
        TraerBotonesDebugAlFrente();
    }

    public void BotonRegistrarDespues()
    {
        if (panelRegistro != null) panelRegistro.SetActive(false);
        especieSeleccionada = "";
        if (panelAvisoInvitado != null)
        {
            panelAvisoInvitado.SetActive(true);
            panelAvisoInvitado.transform.SetAsLastSibling();
        }
        TraerBotonesDebugAlFrente();
    }

    public void AceptarAvisoInvitado()
    {
        if (panelAvisoInvitado != null) panelAvisoInvitado.SetActive(false);
        if (panelSeleccionInvitado != null)
        {
            panelSeleccionInvitado.SetActive(true);
            panelSeleccionInvitado.transform.SetAsLastSibling();
        }
        TraerBotonesDebugAlFrente();
    }

    public void SeleccionarPerro() { especieSeleccionada = "Perro"; }
    public void SeleccionarGato() { especieSeleccionada = "Gato"; }

    public void BotonContinuarInvitado()
    {
        if (string.IsNullOrEmpty(especieSeleccionada))
        {
            if (panelAdvertencia != null) { panelAdvertencia.SetActive(true); panelAdvertencia.transform.SetAsLastSibling(); }
            return;
        }

        if (panelSeleccionInvitado != null) panelSeleccionInvitado.SetActive(false);

        Mascota invitado = new Mascota();
        invitado.nombre = "Invitado";
        invitado.especie = especieSeleccionada;
        invitado.edad = "Desconocida";
        for (int i = 0; i < 15; i++) { invitado.estadosVacunas.Add(false); invitado.fechasVacunas.Add(""); }

        IrAVacunas(invitado, true);
    }

    public void VolverASeleccionInvitado()
    {
        if (panelVacunasPerro != null) panelVacunasPerro.SetActive(false);
        if (panelVacunasGato != null) panelVacunasGato.SetActive(false);
        if (vistaAlimentacionPerro != null) vistaAlimentacionPerro.SetActive(false);
        if (vistaAlimentacionGato != null) vistaAlimentacionGato.SetActive(false);

        mascotaActiva = null;
        especieSeleccionada = "";

        if (panelSeleccionInvitado != null)
        {
            panelSeleccionInvitado.SetActive(true);
            panelSeleccionInvitado.transform.SetAsLastSibling();
        }
        TraerBotonesDebugAlFrente();
    }

    private void CambiarColorSpritesBarra(GameObject panelActivo, bool esVacunas)
    {
        if (panelActivo == null) return;

        Image[] imagenes = panelActivo.GetComponentsInChildren<Image>(true);
        foreach (Image img in imagenes)
        {
            string nombreLower = img.gameObject.name.ToLower();

            if (nombreLower.Contains("perfil"))
                continue;

            if (!nombreLower.Contains("boton") && !nombreLower.Contains("icono"))
                continue;

            if (nombreLower.Contains("vacuna"))
            {
                img.sprite = esVacunas ? iconoVacunaAzul : iconoVacunaGris;
            }
            else if (nombreLower.Contains("comida") || nombreLower.Contains("aliment"))
            {
                img.sprite = esVacunas ? iconoComidaGris : iconoComidaAzul;
            }
        }
    }

    public void ClickPestanaVacunas()
    {
        if (vistaAlimentacionPerro != null) vistaAlimentacionPerro.SetActive(false);
        if (vistaAlimentacionGato != null) vistaAlimentacionGato.SetActive(false);

        if (mascotaActiva != null)
        {
            if (mascotaActiva.especie == "Perro")
            {
                if (panelVacunasPerro != null)
                {
                    panelVacunasPerro.SetActive(true);
                    panelVacunasPerro.transform.SetAsLastSibling();
                    CambiarColorSpritesBarra(panelVacunasPerro, true);
                }
                if (scrollVacunasPerro != null) scrollVacunasPerro.verticalNormalizedPosition = 1f;
            }
            else if (mascotaActiva.especie == "Gato")
            {
                if (panelVacunasGato != null)
                {
                    panelVacunasGato.SetActive(true);
                    panelVacunasGato.transform.SetAsLastSibling();
                    CambiarColorSpritesBarra(panelVacunasGato, true);
                }
                if (scrollVacunasGato != null) scrollVacunasGato.verticalNormalizedPosition = 1f;
            }
        }
        TraerBotonesDebugAlFrente();
    }

    public void ClickPestanaComida()
    {
        if (panelVacunasPerro != null) panelVacunasPerro.SetActive(false);
        if (panelVacunasGato != null) panelVacunasGato.SetActive(false);

        if (mascotaActiva != null)
        {
            if (mascotaActiva.especie == "Perro")
            {
                if (vistaAlimentacionPerro != null)
                {
                    vistaAlimentacionPerro.SetActive(true);
                    vistaAlimentacionPerro.transform.SetAsLastSibling();
                    CambiarColorSpritesBarra(vistaAlimentacionPerro, false);
                }
                if (scrollVacunasPerro != null) scrollVacunasPerro.verticalNormalizedPosition = 1f;
            }
            else if (mascotaActiva.especie == "Gato")
            {
                if (vistaAlimentacionGato != null)
                {
                    vistaAlimentacionGato.SetActive(true);
                    vistaAlimentacionGato.transform.SetAsLastSibling();
                    CambiarColorSpritesBarra(vistaAlimentacionGato, false);
                }
                if (scrollVacunasGato != null) scrollVacunasGato.verticalNormalizedPosition = 1f;
            }
        }
        TraerBotonesDebugAlFrente();
    }

    public void VolverAListaPerfiles()
    {
        modoInvitadoActivo = false;

        if (vistaAlimentacionPerro != null) vistaAlimentacionPerro.SetActive(false);
        if (vistaAlimentacionGato != null) vistaAlimentacionGato.SetActive(false);
        if (panelVacunasPerro != null) panelVacunasPerro.SetActive(false);
        if (panelVacunasGato != null) panelVacunasGato.SetActive(false);

        if (panelPerfiles != null)
        {
            panelPerfiles.SetActive(true);
            panelPerfiles.transform.SetAsLastSibling();
        }

        if (scrollPerfiles != null) scrollPerfiles.verticalNormalizedPosition = 1f;

        ActualizarTarjetasVisuales(); TraerBotonesDebugAlFrente();
    }

    // --- NUEVO: FUNCIÓN PARA LA FLECHA DE VOLVER EN EL REGISTRO ---
    public void CancelarRegistroYVolver()
    {
        if (panelRegistro != null) panelRegistro.SetActive(false);
        if (panelPerfiles != null)
        {
            panelPerfiles.SetActive(true);
            panelPerfiles.transform.SetAsLastSibling();
        }
        TraerBotonesDebugAlFrente();
    }
    // --------------------------------------------------------------

    public void VolverAMascotaActiva()
    {
        if (mascotaActiva != null && !modoInvitadoActivo)
        {
            if (panelPerfiles != null) panelPerfiles.SetActive(false);
            IrAVacunas(mascotaActiva);
        }
    }

    public void IrAVacunas(Mascota m, bool esInvitado = false)
    {
        mascotaActiva = m;
        modoInvitadoActivo = esInvitado;

        if (panelPerfiles != null) panelPerfiles.SetActive(false);
        cargandoVacunas = true;

        if (vistaAlimentacionPerro != null) vistaAlimentacionPerro.SetActive(false);
        if (vistaAlimentacionGato != null) vistaAlimentacionGato.SetActive(false);

        if (botonesBloqueadosInvitado != null)
        {
            foreach (Button btn in botonesBloqueadosInvitado)
            {
                if (btn != null) btn.interactable = !esInvitado;
            }
        }

        if (botonAtrasInvitadoPerro != null) botonAtrasInvitadoPerro.SetActive(esInvitado && m.especie == "Perro");
        if (botonAtrasInvitadoGato != null) botonAtrasInvitadoGato.SetActive(esInvitado && m.especie == "Gato");

        Sprite spriteMascota = fotoPredeterminadaRegistro;
        if (!string.IsNullOrEmpty(m.rutaFoto) && !esInvitado)
        {
            try
            {
                Texture2D tex = NativeGallery.LoadImageAtPath(m.rutaFoto, 256, false);
                if (tex) spriteMascota = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            catch { }
        }

        if (m.especie == "Perro")
        {
            if (textoNombrePerro != null) textoNombrePerro.text = m.nombre;
            if (textoNombrePerroAlimento != null) textoNombrePerroAlimento.text = m.nombre;
            if (imagenPerfilPerro != null) imagenPerfilPerro.sprite = spriteMascota;
            if (imagenPerfilPerroAlimento != null) imagenPerfilPerroAlimento.sprite = spriteMascota;

            if (togglesVacunasPerro != null)
            {
                for (int i = 0; i < togglesVacunasPerro.Length; i++)
                {
                    if (togglesVacunasPerro[i] != null && i < m.estadosVacunas.Count)
                    {
                        togglesVacunasPerro[i].isOn = m.estadosVacunas[i];
                        togglesVacunasPerro[i].interactable = esInvitado ? false : !m.estadosVacunas[i];
                        if (!esInvitado) { if (m.perroMayor1Ano && (i == 0 || i == 1)) togglesVacunasPerro[i].interactable = false; else if (m.perroMayor4Meses && i == 0) togglesVacunasPerro[i].interactable = false; }
                    }
                }
            }
            if (panelVacunasGato != null) panelVacunasGato.SetActive(false);
            if (panelVacunasPerro != null) { panelVacunasPerro.SetActive(true); panelVacunasPerro.transform.SetAsLastSibling(); }

            if (scrollVacunasPerro != null) scrollVacunasPerro.verticalNormalizedPosition = 1f;
            CambiarColorSpritesBarra(panelVacunasPerro, true);
        }
        else
        {
            if (textoNombreGato != null) textoNombreGato.text = m.nombre;
            if (textoNombreGatoAlimento != null) textoNombreGatoAlimento.text = m.nombre;
            if (imagenPerfilGato != null) imagenPerfilGato.sprite = spriteMascota;
            if (imagenPerfilGatoAlimento != null) imagenPerfilGatoAlimento.sprite = spriteMascota;

            if (togglesVacunasGato != null)
            {
                for (int i = 0; i < togglesVacunasGato.Length; i++)
                {
                    if (togglesVacunasGato[i] != null && i < m.estadosVacunas.Count)
                    {
                        togglesVacunasGato[i].isOn = m.estadosVacunas[i];
                        togglesVacunasGato[i].interactable = esInvitado ? false : !m.estadosVacunas[i];
                        if (!esInvitado) { if (m.gatoMayor2Anos && i == 0) togglesVacunasGato[i].interactable = false; }
                    }
                }
            }
            if (panelVacunasPerro != null) panelVacunasPerro.SetActive(false);
            if (panelVacunasGato != null) { panelVacunasGato.SetActive(true); panelVacunasGato.transform.SetAsLastSibling(); }

            if (scrollVacunasGato != null) scrollVacunasGato.verticalNormalizedPosition = 1f;
            CambiarColorSpritesBarra(panelVacunasGato, true);
        }
        cargandoVacunas = false;
        ActualizarEstadoNotificacionesUI();
        TraerBotonesDebugAlFrente();
    }

    public void ValidarInputAnos()
    {
        if (inputAnos != null && !string.IsNullOrEmpty(inputAnos.text))
        {
            int a = 0;
            if (int.TryParse(inputAnos.text, out a))
            {
                if (a > 30) a = 30;
                inputAnos.text = a.ToString();
            }
        }
    }

    public void ValidarInputMeses()
    {
        if (inputMeses != null && !string.IsNullOrEmpty(inputMeses.text))
        {
            int m = 0;
            if (int.TryParse(inputMeses.text, out m))
            {
                if (m > 11) m = 11;
                inputMeses.text = m.ToString();
            }
        }
    }

    public void ValidarInputSemanas()
    {
        if (inputSemanas != null && !string.IsNullOrEmpty(inputSemanas.text))
        {
            int s = 0;
            if (int.TryParse(inputSemanas.text, out s))
            {
                if (s > 3) s = 3;
                inputSemanas.text = s.ToString();
            }
        }
    }

    private string ObtenerEdadDinamica(Mascota m)
    {
        if (string.IsNullOrEmpty(m.fechaNacimiento)) return m.edad;
        DateTime fn;
        if (DateTime.TryParse(m.fechaNacimiento, out fn))
        {
            DateTime hoy = DateTime.Now; if (hoy < fn) hoy = fn;
            int anos = hoy.Year - fn.Year; int meses = hoy.Month - fn.Month; int dias = hoy.Day - fn.Day;
            if (dias < 0) { meses--; dias += DateTime.DaysInMonth(hoy.AddMonths(-1).Year, hoy.AddMonths(-1).Month); }
            if (meses < 0) { anos--; meses += 12; }
            int semanas = dias / 7; if (semanas > 3) semanas = 3;
            return anos + " años, " + meses + " meses, " + semanas + " semanas";
        }
        return m.edad;
    }

    public void AbrirPanelRegistro()
    {
        mascotaActiva = null;
        if (inputNombre != null) inputNombre.text = "";
        if (inputAnos != null) inputAnos.text = "";
        if (inputMeses != null) inputMeses.text = "";
        if (inputSemanas != null) inputSemanas.text = "";
        rutaFotoActual = ""; especieSeleccionada = "";
        if (cuadroFotoRegistro != null && fotoPredeterminadaRegistro != null) cuadroFotoRegistro.sprite = fotoPredeterminadaRegistro;

        if (botonRegistrarDespuesUI != null)
        {
            botonRegistrarDespuesUI.SetActive(todasLasMascotas.Count == 0);
        }

        // --- CONTROL INTELIGENTE DE LA FLECHA DE ATRÁS ---
        if (botonAtrasRegistro != null)
        {
            botonAtrasRegistro.SetActive(todasLasMascotas.Count > 0);
        }
        // -------------------------------------------------

        if (panelRegistro != null) { panelRegistro.SetActive(true); panelRegistro.transform.SetAsLastSibling(); }
        TraerBotonesDebugAlFrente();
    }

    public void AbrirGaleria() { NativeGallery.GetImageFromGallery((path) => { CargarImagenEnCuadro(path); }, "Foto", "image/*"); }
    public void AbrirCamara() { NativeCamera.TakePicture((path) => { CargarImagenEnCuadro(path); }, 512); }

    private void CargarImagenEnCuadro(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        Texture2D texture = NativeGallery.LoadImageAtPath(path, 512, false);
        if (texture != null && cuadroFotoRegistro != null)
        {
            string nombreUnico = "Perfil_" + Guid.NewGuid().ToString() + ".png";
            string nuevaRuta = Path.Combine(Application.persistentDataPath, nombreUnico);

            byte[] bytesImagen = texture.EncodeToPNG();
            File.WriteAllBytes(nuevaRuta, bytesImagen);

            cuadroFotoRegistro.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            rutaFotoActual = nuevaRuta;
        }
    }

    public void GuardarYContinuar()
    {
        if (inputNombre == null || string.IsNullOrEmpty(inputNombre.text) || string.IsNullOrEmpty(especieSeleccionada))
        {
            if (panelAdvertencia != null) { panelAdvertencia.SetActive(true); panelAdvertencia.transform.SetAsLastSibling(); }
            TraerBotonesDebugAlFrente(); return;
        }

        int numAnos = (inputAnos != null && !string.IsNullOrEmpty(inputAnos.text)) ? int.Parse(inputAnos.text) : 0;
        int numMeses = (inputMeses != null && !string.IsNullOrEmpty(inputMeses.text)) ? int.Parse(inputMeses.text) : 0;
        int totalMeses = (numAnos * 12) + numMeses;
        string mensajeAdvertencia = "";

        if (especieSeleccionada == "Perro")
        {
            if (totalMeses >= 6)
            {
                mensajeAdvertencia = "Debido a que tu cachorro tiene 6 meses o más, la vacuna <b><color=#F174E4>Puppy</color></b> y la primera <b><color=#428CEA>Vacuna Múltiple</color></b> se desactivarán del cronograma de vacunación.";
            }
            else if (totalMeses >= 3)
            {
                mensajeAdvertencia = "Debido a que tu cachorro tiene 3 meses o más, la vacuna <b><color=#F174E4>Puppy</color></b> se desactivará del cronograma de vacunación.";
            }
        }
        else if (especieSeleccionada == "Gato")
        {
            if (totalMeses >= 6)
            {
                mensajeAdvertencia = "Debido a que tu gatito tiene 6 meses o más, la primera dosis de <b><color=#428CEA>Triple Felina</color></b> se desactivará del cronograma de vacunación.";
            }
        }

        if (!string.IsNullOrEmpty(mensajeAdvertencia) && panelAvisoEdadAutomatico != null)
        {
            if (textoAvisoEdadAutomatico != null) textoAvisoEdadAutomatico.text = mensajeAdvertencia;
            panelAvisoEdadAutomatico.SetActive(true);
            panelAvisoEdadAutomatico.transform.SetAsLastSibling();
            TraerBotonesDebugAlFrente();
            return;
        }

        EjecutarGuardadoMascota();
    }

    public void EntendidoAvisoEdadAutomatico()
    {
        if (panelAvisoEdadAutomatico != null) panelAvisoEdadAutomatico.SetActive(false);
        EjecutarGuardadoMascota();
    }

    public void CancelarAvisoEdadAutomatico()
    {
        if (panelAvisoEdadAutomatico != null) panelAvisoEdadAutomatico.SetActive(false);
    }

    private void EjecutarGuardadoMascota()
    {
        Mascota nueva = new Mascota();
        nueva.nombre = inputNombre.text;
        nueva.especie = especieSeleccionada;

        int numAnos = (inputAnos != null && !string.IsNullOrEmpty(inputAnos.text)) ? int.Parse(inputAnos.text) : 0;
        int numMeses = (inputMeses != null && !string.IsNullOrEmpty(inputMeses.text)) ? int.Parse(inputMeses.text) : 0;
        int numSemanas = (inputSemanas != null && !string.IsNullOrEmpty(inputSemanas.text)) ? int.Parse(inputSemanas.text) : 0;

        if (numAnos > 30) numAnos = 30; if (numMeses > 11) numMeses = 11; if (numSemanas > 3) numSemanas = 3;

        DateTime fechaNac = DateTime.Now.AddYears(-numAnos).AddMonths(-numMeses).AddDays(-(numSemanas * 7));
        nueva.fechaNacimiento = fechaNac.ToString("yyyy-MM-dd");
        nueva.edad = numAnos + " años, " + numMeses + " meses, " + numSemanas + " semanas";
        nueva.rutaFoto = rutaFotoActual;

        for (int i = 0; i < 15; i++) { nueva.estadosVacunas.Add(false); nueva.fechasVacunas.Add(""); }

        int totalMeses = (numAnos * 12) + numMeses;
        if (nueva.especie == "Perro")
        {
            if (totalMeses >= 6)
            {
                nueva.perroMayor1Ano = true;
                nueva.perroMayor4Meses = false;
            }
            else if (totalMeses >= 3)
            {
                nueva.perroMayor4Meses = true;
                nueva.perroMayor1Ano = false;
            }
        }
        else if (nueva.especie == "Gato")
        {
            if (totalMeses >= 6)
            {
                nueva.gatoMayor2Anos = true;
            }
        }

        todasLasMascotas.Add(nueva);
        GuardarEnMemoria();
        ActualizarTarjetasVisuales();

        if (panelRegistro != null) panelRegistro.SetActive(false);
        MostrarBienvenida(nueva);
    }

    public void CerrarAdvertencia() { if (panelAdvertencia != null) panelAdvertencia.SetActive(false); }

    public void MostrarBienvenida(Mascota m)
    {
        mascotaActiva = m; if (textoBienvenida != null) textoBienvenida.text = "¡Bienvenido, " + m.nombre + "!";
        if (panelBienvenida != null) { panelBienvenida.SetActive(true); panelBienvenida.transform.SetAsLastSibling(); TraerBotonesDebugAlFrente(); } else { AceptarBienvenida(); }
    }

    public void AceptarBienvenida()
    {
        if (panelBienvenida != null) panelBienvenida.SetActive(false);
        if (mascotaActiva != null) IrAVacunas(mascotaActiva);
    }

    public void AbrirPanelEdicion()
    {
        if (mascotaActiva == null || panelEdicion == null || modoInvitadoActivo) return;
        panelEdicion.SetActive(true); panelEdicion.transform.SetAsLastSibling();
        if (textoTituloEdicion != null) textoTituloEdicion.text = mascotaActiva.nombre;
        if (textoEdadEdicion != null) textoEdadEdicion.text = ObtenerEdadDinamica(mascotaActiva);
        if (inputNombreEdicion != null) inputNombreEdicion.text = mascotaActiva.nombre;
        rutaFotoEdicion = mascotaActiva.rutaFoto;

        if (!string.IsNullOrEmpty(rutaFotoEdicion))
        {
            Texture2D texture = NativeGallery.LoadImageAtPath(rutaFotoEdicion, 512, false);
            if (texture != null && imagenPerfilEdicion != null)
            {
                imagenPerfilEdicion.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
        else
        {
            if (imagenPerfilEdicion != null) imagenPerfilEdicion.sprite = fotoPredeterminadaRegistro;
        }
        TraerBotonesDebugAlFrente();
    }

    public void AbrirGaleriaEdicion() { NativeGallery.GetImageFromGallery((path) => { CargarImagenEnEditor(path); }, "Foto", "image/*"); }
    public void AbrirCamaraEdicion() { NativeCamera.TakePicture((path) => { CargarImagenEnEditor(path); }, 512); }

    private void CargarImagenEnEditor(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        Texture2D texture = NativeGallery.LoadImageAtPath(path, 512, false);
        if (texture != null && imagenPerfilEdicion != null)
        {
            string nombreUnico = "Perfil_" + Guid.NewGuid().ToString() + ".png";
            string nuevaRuta = Path.Combine(Application.persistentDataPath, nombreUnico);

            byte[] bytesImagen = texture.EncodeToPNG();
            File.WriteAllBytes(nuevaRuta, bytesImagen);

            imagenPerfilEdicion.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            rutaFotoEdicion = nuevaRuta;
        }
    }

    public void GuardarCambiosEdicion()
    {
        if (inputNombreEdicion == null || string.IsNullOrEmpty(inputNombreEdicion.text)) return;
        mascotaActiva.nombre = inputNombreEdicion.text; mascotaActiva.rutaFoto = rutaFotoEdicion;
        GuardarEnMemoria(); ActualizarTarjetasVisuales(); IrAVacunas(mascotaActiva);
        if (panelEdicion != null) panelEdicion.SetActive(false);
        ResetearScrollMascotaActual();
    }

    public void CancelarEdicion()
    {
        if (panelEdicion != null) panelEdicion.SetActive(false);
        ResetearScrollMascotaActual();
    }

    public void AbrirPanelPerrosMayores() { if (panelPerrosMayores != null && mascotaActiva != null) { panelPerrosMayores.SetActive(true); panelPerrosMayores.transform.SetAsLastSibling(); TraerBotonesDebugAlFrente(); } }
    public void CerrarPanelPerrosMayores() { if (panelPerrosMayores != null) panelPerrosMayores.SetActive(false); ResetearScrollMascotaActual(); }
    public void AbrirPanelGatosMayores() { if (panelGatosMayores != null && mascotaActiva != null) { panelGatosMayores.SetActive(true); panelGatosMayores.transform.SetAsLastSibling(); TraerBotonesDebugAlFrente(); } }
    public void CerrarPanelGatosMayores() { if (panelGatosMayores != null) panelGatosMayores.SetActive(false); ResetearScrollMascotaActual(); }
    public void AbrirPanelEnfermedades() { if (panelEnfermedadesListado != null) { panelEnfermedadesListado.SetActive(true); panelEnfermedadesListado.transform.SetAsLastSibling(); TraerBotonesDebugAlFrente(); } }
    public void CerrarPanelEnfermedades() { if (panelEnfermedadesListado != null) panelEnfermedadesListado.SetActive(false); ResetearScrollMascotaActual(); }

    public void GuardarEstadoVacunas()
    {
        if (mascotaActiva == null || cargandoVacunas || modoInvitadoActivo) return;
        Toggle[] togglesActuales = (mascotaActiva.especie == "Perro") ? togglesVacunasPerro : togglesVacunasGato;
        for (int i = 0; i < togglesActuales.Length; i++)
        {
            if (togglesActuales[i] == null) continue;
            while (mascotaActiva.estadosVacunas.Count <= i) { mascotaActiva.estadosVacunas.Add(false); mascotaActiva.fechasVacunas.Add(""); }
            if (togglesActuales[i].isOn != mascotaActiva.estadosVacunas[i])
            {
                indiceVacunaPendiente = i; nuevoEstadoVacunaPendiente = togglesActuales[i].isOn;
                cargandoVacunas = true; togglesActuales[i].isOn = mascotaActiva.estadosVacunas[i]; cargandoVacunas = false;
                if (i == 4 && mascotaActiva.especie == "Perro" && nuevoEstadoVacunaPendiente == true) { if (panelPreguntaTraqueo != null) { panelPreguntaTraqueo.SetActive(true); panelPreguntaTraqueo.transform.SetAsLastSibling(); TraerBotonesDebugAlFrente(); } }
                else
                {
                    if (textoConfirmacionVacuna != null)
                    {
                        string nombreLimpio = togglesActuales[i].gameObject.name.Replace("toogle", "").Replace("Toggle", "").Trim();
                        string animalTexto = (mascotaActiva.especie == "Perro") ? "perrito" : "gatito";
                        string parrafo1 = "Has marcado que tu " + animalTexto + " ya recibió la vacuna " + nombreLimpio + ".";
                        string parrafo2 = "<color=#666666>Recibirás una notificación diaria dos días antes de la fecha recomendada para la siguiente dosis.</color>";
                        string parrafo3 = "<color=#666666>Puedes desactivar estas notificaciones en la sección de notificaciones.</color>";
                        if ((mascotaActiva.especie == "Perro" && i == 5) || (mascotaActiva.especie == "Gato" && i == 4)) { parrafo2 = "<color=#666666>Recibirás una notificación diaria dos días antes de la fecha recomendada para la siguiente dosis del refuerzo anual.</color>"; }
                        else if ((mascotaActiva.especie == "Perro" && i == 6) || (mascotaActiva.especie == "Gato" && i == 5)) { parrafo2 = "<color=#666666>Recibirás una notificación diaria dos días antes de la fecha recomendada para el próximo refuerzo anual.</color>"; }
                        textoConfirmacionVacuna.text = parrafo1 + "\n\n" + parrafo2 + "\n\n" + parrafo3;
                    }
                    if (panelConfirmacionVacuna != null) { panelConfirmacionVacuna.SetActive(true); panelConfirmacionVacuna.transform.SetAsLastSibling(); TraerBotonesDebugAlFrente(); }
                }
                break;
            }
        }
    }

    public void ConfirmarCambioVacuna()
    {
        if (indiceVacunaPendiente == -1 || mascotaActiva == null) return;
        mascotaActiva.estadosVacunas[indiceVacunaPendiente] = nuevoEstadoVacunaPendiente;
        if (nuevoEstadoVacunaPendiente)
        {
            mascotaActiva.notificacionOcultaID = 0; mascotaActiva.fechasVacunas[indiceVacunaPendiente] = DateTime.Now.ToString("yyyy-MM-dd");
            if (mascotaActiva.notificacionesPausadas) { mascotaActiva.notificacionesPausadas = false; mascotaActiva.indiceVacunaPausada = -1; mascotaActiva.estadoAlertaPausada = 0; }
            Toggle[] togglesActuales = (mascotaActiva.especie == "Perro") ? togglesVacunasPerro : togglesVacunasGato;
            if (togglesActuales != null && indiceVacunaPendiente < togglesActuales.Length && togglesActuales[indiceVacunaPendiente] != null)
            {
                string nombreVacuna = togglesActuales[indiceVacunaPendiente].gameObject.name.Replace("toogle", "").Replace("Toggle", "").Trim();
                ProgramarNotificacionVacunaReal(mascotaActiva, indiceVacunaPendiente, nombreVacuna);
            }
        }
        GuardarEnMemoria(); ActualizarEstadoNotificacionesUI();
        Toggle[] togglesActualesFinal = (mascotaActiva.especie == "Perro") ? togglesVacunasPerro : togglesVacunasGato;
        if (togglesActualesFinal != null && indiceVacunaPendiente < togglesActualesFinal.Length && togglesActualesFinal[indiceVacunaPendiente] != null)
        {
            cargandoVacunas = true; togglesActualesFinal[indiceVacunaPendiente].isOn = nuevoEstadoVacunaPendiente;
            if (nuevoEstadoVacunaPendiente) togglesActualesFinal[indiceVacunaPendiente].interactable = false; cargandoVacunas = false;
        }
        indiceVacunaPendiente = -1; if (panelConfirmacionVacuna != null) panelConfirmacionVacuna.SetActive(false);
    }

    public void CancelarCambioVacuna() { if (indiceVacunaPendiente != -1 && mascotaActiva != null) { Toggle[] togglesActuales = (mascotaActiva.especie == "Perro") ? togglesVacunasPerro : togglesVacunasGato; if (togglesActuales != null && indiceVacunaPendiente < togglesActuales.Length && togglesActuales[indiceVacunaPendiente] != null) { cargandoVacunas = true; togglesActuales[indiceVacunaPendiente].isOn = mascotaActiva.estadosVacunas[indiceVacunaPendiente]; cargandoVacunas = false; } } indiceVacunaPendiente = -1; if (panelConfirmacionVacuna != null) panelConfirmacionVacuna.SetActive(false); if (panelPreguntaTraqueo != null) panelPreguntaTraqueo.SetActive(false); }

    public void SeleccionoNasal() { if (mascotaActiva != null) { mascotaActiva.traqueoFueNasal = true; } if (panelPreguntaTraqueo != null) panelPreguntaTraqueo.SetActive(false); if (textoConfirmacionVacuna != null) { textoConfirmacionVacuna.text = "Has marcado que tu perrito ya recibió la Rabia + Traqueobronquitis.\n\n<color=#666666>Recibirás una notificación diaria dos días antes de la fecha recomendada para el refuerzo anual.</color>\n\n<color=#666666>Puedes desactivar estas notificaciones en la sección de notificaciones.</color>"; } if (panelConfirmacionVacuna != null) { panelConfirmacionVacuna.SetActive(true); panelConfirmacionVacuna.transform.SetAsLastSibling(); TraerBotonesDebugAlFrente(); } }
    public void SeleccionoInyeccion() { if (mascotaActiva != null) { mascotaActiva.traqueoFueNasal = false; } if (panelPreguntaTraqueo != null) panelPreguntaTraqueo.SetActive(false); if (textoConfirmacionVacuna != null) { textoConfirmacionVacuna.text = "Has marcado que tu perrito ya recibió la Rabia + Traqueobronquitis.\n\n<color=#666666>Recibirás una notificación diaria dos días antes de la fecha recomendada para la segunda dosis de Traqueobronquitis.</color>\n\n<color=#666666>Puedes desactivar estas notificaciones en la sección de notificaciones.</color>"; } if (panelConfirmacionVacuna != null) { panelConfirmacionVacuna.SetActive(true); panelConfirmacionVacuna.transform.SetAsLastSibling(); TraerBotonesDebugAlFrente(); } }

    public void AbrirCarnetUnificado() { if (mascotaActiva == null || panelCarnetUnificado == null || modoInvitadoActivo) return; ActualizarUICarnet(); panelCarnetUnificado.SetActive(true); panelCarnetUnificado.transform.SetAsLastSibling(); TraerBotonesDebugAlFrente(); }

    public void CerrarCarnetUnificado()
    {
        if (imagenVisorFullscreen != null) imagenVisorFullscreen.transform.localScale = Vector3.one;
        if (panelCarnetUnificado != null) panelCarnetUnificado.SetActive(false);
    }

    public void TomarFotoCarnet() { NativeCamera.TakePicture((path) => { ProcesarNuevaFotoCarnet(path); }, 512); }
    public void AbrirGaleriaCarnet() { NativeGallery.GetImageFromGallery((path) => { ProcesarNuevaFotoCarnet(path); }, "Foto Carnet", "image/*"); }

    private void ProcesarNuevaFotoCarnet(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024, false);
        if (texture == null) return;

        string nombreUnico = "Carnet_" + Guid.NewGuid().ToString() + ".png";
        string nuevaRuta = Path.Combine(Application.persistentDataPath, nombreUnico);

        byte[] bytesImagen = texture.EncodeToPNG();
        File.WriteAllBytes(nuevaRuta, bytesImagen);

        mascotaActiva.rutasCarnet.Insert(0, nuevaRuta); mascotaActiva.fechasCarnet.Insert(0, DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        if (mascotaActiva.rutasCarnet.Count > 30) { if (File.Exists(mascotaActiva.rutasCarnet[mascotaActiva.rutasCarnet.Count - 1])) { File.Delete(mascotaActiva.rutasCarnet[mascotaActiva.rutasCarnet.Count - 1]); } mascotaActiva.rutasCarnet.RemoveAt(mascotaActiva.rutasCarnet.Count - 1); mascotaActiva.fechasCarnet.RemoveAt(mascotaActiva.fechasCarnet.Count - 1); }
        GuardarEnMemoria(); ActualizarUICarnet();
    }

    public void ActualizarUICarnet()
    {
        if (contenedorHistorialCarnet == null) return;
        foreach (Transform child in contenedorHistorialCarnet) Destroy(child.gameObject);
        if (mascotaActiva.rutasCarnet.Count == 0) { if (imagenCarnetPrincipal != null) imagenCarnetPrincipal.sprite = null; if (textoFechaPrincipal != null) textoFechaPrincipal.text = "Sin registros. Sube una foto."; if (botonEliminarPrincipal != null) botonEliminarPrincipal.SetActive(false); return; }
        if (imagenCarnetPrincipal != null) { Texture2D texMain = NativeGallery.LoadImageAtPath(mascotaActiva.rutasCarnet[0], 1024, false); if (texMain) imagenCarnetPrincipal.sprite = Sprite.Create(texMain, new Rect(0, 0, texMain.width, texMain.height), new Vector2(0.5f, 0.5f)); }
        if (textoFechaPrincipal != null) textoFechaPrincipal.text = mascotaActiva.fechasCarnet[0]; if (botonEliminarPrincipal != null) botonEliminarPrincipal.SetActive(true);
        for (int i = 1; i < mascotaActiva.rutasCarnet.Count; i++) { if (prefabCarnetHistorial == null) break; GameObject miniatura = Instantiate(prefabCarnetHistorial, contenedorHistorialCarnet); Image imgMin = miniatura.transform.Find("ImagenMiniatura")?.GetComponent<Image>(); TextMeshProUGUI txtF = miniatura.transform.Find("TextoFecha")?.GetComponent<TextMeshProUGUI>(); Button btnX = miniatura.transform.Find("BotonX")?.GetComponent<Button>(); Button btnZoom = miniatura.GetComponent<Button>(); if (txtF != null) txtF.text = mascotaActiva.fechasCarnet[i]; Texture2D texMin = NativeGallery.LoadImageAtPath(mascotaActiva.rutasCarnet[i], 512, false); if (texMin && imgMin != null) imgMin.sprite = Sprite.Create(texMin, new Rect(0, 0, texMin.width, texMin.height), new Vector2(0.5f, 0.5f)); int indexACapturar = i; if (btnX != null) btnX.onClick.AddListener(() => SolicitarEliminarCarnet(indexACapturar)); if (btnZoom != null) btnZoom.onClick.AddListener(() => AbrirVisorCarnet(mascotaActiva.rutasCarnet[indexACapturar])); }
    }

    public void SolicitarEliminarCarnetPrincipal() { SolicitarEliminarCarnet(0); }
    public void SolicitarEliminarCarnet(int index) { indiceCarnetAEliminar = index; if (panelConfirmacionEliminarCarnet != null) { panelConfirmacionEliminarCarnet.SetActive(true); panelConfirmacionEliminarCarnet.transform.SetAsLastSibling(); TraerBotonesDebugAlFrente(); } }
    public void ConfirmarEliminacionCarnet() { if (mascotaActiva != null && indiceCarnetAEliminar >= 0 && indiceCarnetAEliminar < mascotaActiva.rutasCarnet.Count) { mascotaActiva.rutasCarnet.RemoveAt(indiceCarnetAEliminar); mascotaActiva.fechasCarnet.RemoveAt(indiceCarnetAEliminar); GuardarEnMemoria(); ActualizarUICarnet(); } indiceCarnetAEliminar = -1; if (panelConfirmacionEliminarCarnet != null) panelConfirmacionEliminarCarnet.SetActive(false); }
    public void CancelarEliminacionCarnet() { indiceCarnetAEliminar = -1; if (panelConfirmacionEliminarCarnet != null) panelConfirmacionEliminarCarnet.SetActive(false); }
    public void AbrirVisorCarnetPrincipal() { if (mascotaActiva != null && mascotaActiva.rutasCarnet.Count > 0) AbrirVisorCarnet(mascotaActiva.rutasCarnet[0]); }

    public void AbrirVisorCarnet(string path)
    {
        if (string.IsNullOrEmpty(path) || panelVisorFullscreen == null || imagenVisorFullscreen == null) return;
        Texture2D tex = NativeGallery.LoadImageAtPath(path, 1024, false);
        if (tex)
        {
            imagenVisorFullscreen.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            imagenVisorFullscreen.preserveAspect = true;

            // --- NUEVO: Ajustar para que llene el ancho exacto de la pantalla ---
            RectTransform rt = imagenVisorFullscreen.GetComponent<RectTransform>();
            RectTransform parentRt = panelVisorFullscreen.GetComponent<RectTransform>();

            float anchoPantalla = parentRt.rect.width;
            float proporcionOriginal = (float)tex.width / (float)tex.height;

            // Le damos el ancho de la pantalla y calculamos su altura real sin deformar
            rt.sizeDelta = new Vector2(anchoPantalla, anchoPantalla / proporcionOriginal);
            // --------------------------------------------------------------------

            imagenVisorFullscreen.transform.localScale = Vector3.one;
            imagenVisorFullscreen.transform.localPosition = Vector3.zero;
            panelVisorFullscreen.SetActive(true);
            panelVisorFullscreen.transform.SetAsLastSibling();
            TraerBotonesDebugAlFrente();
        }
    }
    public void CerrarVisorCarnet()
    {
        if (panelVisorFullscreen != null)
        {
            panelVisorFullscreen.SetActive(false);
        }
    }

    public void AbrirPanelRecordatorios() { if (mascotaActiva == null || panelRecordatorios == null) return; ActualizarEstadoNotificacionesUI(); panelRecordatorios.SetActive(true); panelRecordatorios.transform.SetAsLastSibling(); TraerBotonesDebugAlFrente(); }
    public void CerrarPanelRecordatorios() { if (panelRecordatorios != null) panelRecordatorios.SetActive(false); }
    public void PausarNotificaciones() { if (mascotaActiva != null && !mascotaActiva.notificacionesPausadas) { mascotaActiva.notificacionesPausadas = true; mascotaActiva.indiceVacunaPausada = tempIndicePendiente; mascotaActiva.estadoAlertaPausada = tempEstadoAlerta; GuardarEnMemoria(); ActualizarEstadoNotificacionesUI(); } }
    public void ReanudarNotificaciones() { if (mascotaActiva != null && mascotaActiva.notificacionesPausadas) { mascotaActiva.notificacionesPausadas = false; mascotaActiva.indiceVacunaPausada = -1; mascotaActiva.estadoAlertaPausada = 0; GuardarEnMemoria(); ActualizarEstadoNotificacionesUI(); } }
    public void OcultarTarjetaManual() { if (mascotaActiva != null) { mascotaActiva.notificacionOcultaID = idNotificacionActual; GuardarEnMemoria(); ActualizarEstadoNotificacionesUI(); } }

    private void ActualizarVisibilidadVacunasEspeciales(Mascota m)
    {
        if (m == null) return;
        if (m.especie == "Perro")
        {
            if (vacunaSegundaDosisTraqueo != null) { bool rabiaMarcada = m.estadosVacunas.Count > 4 ? m.estadosVacunas[4] : false; vacunaSegundaDosisTraqueo.SetActive(rabiaMarcada && !m.traqueoFueNasal); }
            if (vacunaRefuerzoAnual != null) { bool mostrarRefuerzo = false; if (m.estadosVacunas.Count > 6) { if (m.estadosVacunas[6] == true || !string.IsNullOrEmpty(m.fechasVacunas[6])) mostrarRefuerzo = true; else if (m.estadosVacunas[4] == true) { DateTime fechaBase = DateTime.MinValue; if (!m.traqueoFueNasal) { if (m.estadosVacunas.Count > 5 && m.estadosVacunas[5] == true && !string.IsNullOrEmpty(m.fechasVacunas[5])) DateTime.TryParse(m.fechasVacunas[5], out fechaBase); } else { if (!string.IsNullOrEmpty(m.fechasVacunas[4])) DateTime.TryParse(m.fechasVacunas[4], out fechaBase); } if (fechaBase != DateTime.MinValue) { DateTime fechaAviso = fechaBase.Date.AddYears(1).AddDays(-2); if (DateTime.Now.Date >= fechaAviso) mostrarRefuerzo = true; } } } vacunaRefuerzoAnual.SetActive(mostrarRefuerzo); }
        }
        else if (m.especie == "Gato") { if (vacunaRefuerzoAnualGato != null) { bool mostrarRefuerzo = false; if (m.estadosVacunas.Count > 5) { if (m.estadosVacunas[5] == true || !string.IsNullOrEmpty(m.fechasVacunas[5])) mostrarRefuerzo = true; else if (m.estadosVacunas[4] == true) { DateTime fechaRabia; if (!string.IsNullOrEmpty(m.fechasVacunas[4]) && DateTime.TryParse(m.fechasVacunas[4], out fechaRabia)) { DateTime fechaAviso = fechaRabia.Date.AddYears(1).AddDays(-2); if (DateTime.Now.Date >= fechaAviso) mostrarRefuerzo = true; } } } vacunaRefuerzoAnualGato.SetActive(mostrarRefuerzo); } }
    }

    private void ActualizarEstadoNotificacionesUI()
    {
        if (mascotaActiva == null) return;
        if (mascotaActiva.especie == "Perro" && mascotaActiva.estadosVacunas.Count > 6) { if (mascotaActiva.estadosVacunas[6] == true && !string.IsNullOrEmpty(mascotaActiva.fechasVacunas[6])) { if (DateTime.TryParse(mascotaActiva.fechasVacunas[6], out DateTime fechaUltimoRefuerzo)) { DateTime fechaAviso = fechaUltimoRefuerzo.Date.AddYears(1).AddDays(-2); if (DateTime.Now.Date >= fechaAviso) { mascotaActiva.estadosVacunas[6] = false; GuardarEnMemoria(); if (togglesVacunasPerro.Length > 6 && togglesVacunasPerro[6] != null) { cargandoVacunas = true; togglesVacunasPerro[6].isOn = false; togglesVacunasPerro[6].interactable = true; cargandoVacunas = false; } } } } }
        else if (mascotaActiva.especie == "Gato" && mascotaActiva.estadosVacunas.Count > 5) { if (mascotaActiva.estadosVacunas[5] == true && !string.IsNullOrEmpty(mascotaActiva.fechasVacunas[5])) { if (DateTime.TryParse(mascotaActiva.fechasVacunas[5], out DateTime fechaUltimoRefuerzo)) { DateTime fechaAviso = fechaUltimoRefuerzo.Date.AddYears(1).AddDays(-2); if (DateTime.Now.Date >= fechaAviso) { mascotaActiva.estadosVacunas[5] = false; GuardarEnMemoria(); if (togglesVacunasGato.Length > 5 && togglesVacunasGato[5] != null) { cargandoVacunas = true; togglesVacunasGato[5].isOn = false; togglesVacunasGato[5].interactable = true; cargandoVacunas = false; } } } } }

        ActualizarVisibilidadVacunasEspeciales(mascotaActiva); idNotificacionActual = 0; tempIndicePendiente = -1; tempEstadoAlerta = 0; Sprite spriteAMostrar = null; int limiteVacunas = (mascotaActiva.especie == "Perro") ? 6 : 5;

        if (mascotaActiva.estadosVacunas.Count >= limiteVacunas + 1)
        {
            int indicePendiente = -1; int indiceAnterior = -1; int inicioBucle = (mascotaActiva.especie == "Perro") ? 1 : 0;
            if (mascotaActiva.especie == "Perro" && mascotaActiva.perroMayor1Ano) inicioBucle = 2; else if (mascotaActiva.especie == "Gato" && mascotaActiva.gatoMayor2Anos) inicioBucle = 1;
            for (int i = inicioBucle; i <= limiteVacunas; i++) { if (mascotaActiva.estadosVacunas[i] == false) { if (mascotaActiva.especie == "Perro" && i == 5 && mascotaActiva.traqueoFueNasal) continue; if (i == limiteVacunas && mascotaActiva.estadosVacunas[limiteVacunas - 2] == false) continue; indicePendiente = i; if (i == limiteVacunas) { if (!string.IsNullOrEmpty(mascotaActiva.fechasVacunas[limiteVacunas])) { indiceAnterior = limiteVacunas; } else { if (mascotaActiva.especie == "Perro") { indiceAnterior = mascotaActiva.traqueoFueNasal ? 4 : 5; } else { indiceAnterior = 4; } } } else { indiceAnterior = i - 1; } break; } }
            if (indicePendiente != -1 && indiceAnterior != -1 && !string.IsNullOrEmpty(mascotaActiva.fechasVacunas[indiceAnterior])) { if (DateTime.TryParse(mascotaActiva.fechasVacunas[indiceAnterior], out DateTime fechaBase)) { DateTime hoy = DateTime.Now.Date; DateTime fechaBaseSoloDia = fechaBase.Date; int diasRestantes = 0; if (indicePendiente == limiteVacunas) { DateTime fechaSiguienteAnual = fechaBaseSoloDia.AddYears(1); diasRestantes = (int)(fechaSiguienteAnual - hoy).TotalDays; } else { DateTime fechaSiguienteNormal = fechaBaseSoloDia.AddDays(21); diasRestantes = (int)(fechaSiguienteNormal - hoy).TotalDays; } tempIndicePendiente = indicePendiente; if (diasRestantes < -7) tempEstadoAlerta = 4; else if (diasRestantes <= 0 && diasRestantes >= -7) tempEstadoAlerta = 3; else if (diasRestantes == 1) tempEstadoAlerta = 2; else if (diasRestantes == 2) tempEstadoAlerta = 1; } }
        }

        int indiceAUsar = mascotaActiva.notificacionesPausadas ? mascotaActiva.indiceVacunaPausada : tempIndicePendiente;
        int estadoAUsar = mascotaActiva.notificacionesPausadas ? mascotaActiva.estadoAlertaPausada : tempEstadoAlerta;
        GrupoAlertasVacuna[] arrayAlertasAUsar = (mascotaActiva.especie == "Perro") ? alertasPorVacuna : alertasPorVacunaGato;

        if (indiceAUsar != -1 && estadoAUsar != 0 && indiceAUsar < arrayAlertasAUsar.Length && arrayAlertasAUsar[indiceAUsar] != null) { idNotificacionActual = indiceAUsar * 1000 + estadoAUsar; if (estadoAUsar == 4) spriteAMostrar = arrayAlertasAUsar[indiceAUsar].atrasada; else if (estadoAUsar == 3) spriteAMostrar = arrayAlertasAUsar[indiceAUsar].esHoy; else if (estadoAUsar == 2) spriteAMostrar = arrayAlertasAUsar[indiceAUsar].falta1Dia; else if (estadoAUsar == 1) spriteAMostrar = arrayAlertasAUsar[indiceAUsar].faltan2Dias; }
        if (idNotificacionActual == 0 || mascotaActiva.notificacionOcultaID == idNotificacionActual) { if (tarjetaNotificacion != null) tarjetaNotificacion.SetActive(false); if (textoSinNotificaciones != null) textoSinNotificaciones.SetActive(true); if (iconoCampanaPerro != null) iconoCampanaPerro.sprite = spriteCampanaNormal; if (iconoCampanaGato != null) iconoCampanaGato.sprite = spriteCampanaNormal; } else { if (tarjetaNotificacion != null) tarjetaNotificacion.SetActive(true); if (textoSinNotificaciones != null) textoSinNotificaciones.SetActive(false); if (imagenEstadoNotificacion != null && spriteAMostrar != null) imagenEstadoNotificacion.sprite = spriteAMostrar; if (iconoCampanaPerro != null) iconoCampanaPerro.sprite = spriteCampanaAlerta; if (iconoCampanaGato != null) iconoCampanaGato.sprite = spriteCampanaAlerta; }
        if (mascotaActiva.notificacionesPausadas) { if (btnPausarNotificaciones != null) { btnPausarNotificaciones.interactable = false; btnPausarNotificaciones.GetComponent<Image>().sprite = spritePausarGris; } if (btnReanudarNotificaciones != null) { btnReanudarNotificaciones.interactable = true; btnReanudarNotificaciones.GetComponent<Image>().sprite = spriteReanudarAzul; } } else { if (btnPausarNotificaciones != null) { btnPausarNotificaciones.interactable = true; btnPausarNotificaciones.GetComponent<Image>().sprite = spritePausarAzul; } if (btnReanudarNotificaciones != null) { btnReanudarNotificaciones.interactable = false; btnReanudarNotificaciones.GetComponent<Image>().sprite = spriteReanudarGris; } }
    }

    public void ConfigurarCanalNotificaciones()
    {
        var channel = new AndroidNotificationChannel()
        {
            Id = "canal_vacunas",
            Name = "Recordatorios de Vacunas",
            Importance = Importance.High,
            Description = "Avisos importantes de Crecen Sanos"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    // --- NUEVO: 3 NOTIFICACIONES EXACTAS AL DÍA ---
    private void ProgramarNotificacionVacunaReal(Mascota m, int indice, string nombreVacuna)
    {
        DateTime fechaAviso;
        int limite = (m.especie == "Perro") ? 6 : 5;

        if (indice == limite || (m.especie == "Perro" && indice == 5))
        {
            fechaAviso = DateTime.Now.AddYears(1).AddDays(-2);
        }
        else
        {
            fechaAviso = DateTime.Now.AddDays(19);
        }

        // Calculamos las horas exactas de ese día
        DateTime fechaBase = fechaAviso.Date; // Día exacto a las 00:00:00
        DateTime hora8AM = fechaBase.AddHours(8);   // 08:00 AM
        DateTime hora2PM = fechaBase.AddHours(14);  // 02:00 PM
        DateTime hora7PM = fechaBase.AddHours(19);  // 07:00 PM

        // 1. Notificación de las 8:00 AM
        var notif8AM = new AndroidNotification();
        notif8AM.Title = "🐾 Vacuna Pendiente: " + m.nombre;
        notif8AM.Text = "Buenos días. No olvides la vacuna de " + m.nombre + " hoy.";
        notif8AM.FireTime = hora8AM;
        notif8AM.SmallIcon = "default";
        notif8AM.LargeIcon = "default";
        AndroidNotificationCenter.SendNotification(notif8AM, "canal_vacunas");

        // 2. Notificación de las 2:00 PM
        var notif2PM = new AndroidNotification();
        notif2PM.Title = "🐾 Recordatorio: " + m.nombre;
        notif2PM.Text = "¡Hola! Es momento de revisar el cronograma de " + m.nombre + ".";
        notif2PM.FireTime = hora2PM;
        notif2PM.SmallIcon = "default";
        notif2PM.LargeIcon = "default";
        AndroidNotificationCenter.SendNotification(notif2PM, "canal_vacunas");

        // 3. Notificación de las 7:00 PM
        var notif7PM = new AndroidNotification();
        notif7PM.Title = "🐾 Último aviso del día: " + m.nombre;
        notif7PM.Text = "Recuerda que la vacuna de " + m.nombre + " está pendiente.";
        notif7PM.FireTime = hora7PM;
        notif7PM.SmallIcon = "default";
        notif7PM.LargeIcon = "default";
        AndroidNotificationCenter.SendNotification(notif7PM, "canal_vacunas");
    }
    // ----------------------------------------------

    public void BotonPruebaNotificacionNativa()
    {
        var notification = new AndroidNotification();
        if (mascotaActiva != null)
        {
            notification.Title = "🐾 Vacuna Pendiente: " + mascotaActiva.nombre;
        }
        else
        {
            notification.Title = "¡Crecen Sanos!";
        }

        notification.Text = "Notificación de vacuna";
        notification.FireTime = System.DateTime.Now.AddSeconds(5);
        notification.SmallIcon = "default";
        notification.LargeIcon = "default";
        AndroidNotificationCenter.SendNotification(notification, "canal_vacunas");
    }

    void GuardarEnMemoria() { ListaMascotas wrapper = new ListaMascotas(); wrapper.lista = todasLasMascotas; PlayerPrefs.SetString("DataMascotasFinal", JsonUtility.ToJson(wrapper)); PlayerPrefs.Save(); }
    void CargarMascotas() { string data = PlayerPrefs.GetString("DataMascotasFinal", ""); if (!string.IsNullOrEmpty(data)) { todasLasMascotas = JsonUtility.FromJson<ListaMascotas>(data).lista; foreach (Mascota m in todasLasMascotas) { if (m.rutasCarnet == null) m.rutasCarnet = new List<string>(); if (m.fechasCarnet == null) m.fechasCarnet = new List<string>(); } } ActualizarTarjetasVisuales(); }

    void ActualizarTarjetasVisuales()
    {
        if (contenedorPerfiles == null) return;
        foreach (Transform child in contenedorPerfiles) Destroy(child.gameObject);
        foreach (Mascota m in todasLasMascotas)
        {
            if (prefabPerfil == null) continue;
            GameObject t = Instantiate(prefabPerfil, contenedorPerfiles);
            Transform txtN = t.transform.Find("TextoNombre"); if (txtN != null) txtN.GetComponent<TextMeshProUGUI>().text = m.nombre;
            Transform txtE = t.transform.Find("TextoEdad"); if (txtE != null) txtE.GetComponent<TextMeshProUGUI>().text = ObtenerEdadDinamica(m);
            Transform imgF = t.transform.Find("Foto");

            if (imgF != null)
            {
                if (!string.IsNullOrEmpty(m.rutaFoto))
                {
                    try { Texture2D tex = NativeGallery.LoadImageAtPath(m.rutaFoto, 256, false); if (tex) imgF.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)); } catch { }
                }
                else
                {
                    imgF.GetComponent<Image>().sprite = fotoPredeterminadaRegistro;
                }
            }

            t.GetComponent<Button>().onClick.AddListener(() => IrAVacunas(m));
            Transform btnX = t.transform.Find("BotonX"); if (btnX != null) { btnX.GetComponent<Button>().onClick.AddListener(() => { mascotaAEliminar = m; if (panelConfirmacion != null) { panelConfirmacion.SetActive(true); panelConfirmacion.transform.SetAsLastSibling(); TraerBotonesDebugAlFrente(); } }); }
        }
    }

    public void ConfirmarEliminacion() { todasLasMascotas.Remove(mascotaAEliminar); GuardarEnMemoria(); ActualizarTarjetasVisuales(); if (panelConfirmacion != null) panelConfirmacion.SetActive(false); if (panelVacunasPerro != null) panelVacunasPerro.SetActive(false); if (panelVacunasGato != null) panelVacunasGato.SetActive(false); if (panelPerfiles != null) { panelPerfiles.SetActive(true); panelPerfiles.transform.SetAsLastSibling(); TraerBotonesDebugAlFrente(); } }
    public void CancelarEliminacion() { if (panelConfirmacion != null) panelConfirmacion.SetActive(false); }
    public void DEBUG_BorrarTodaLaMemoria() { PlayerPrefs.DeleteAll(); PlayerPrefs.Save(); todasLasMascotas.Clear(); ActualizarTarjetasVisuales(); Debug.Log("¡Toda la memoria borrada!"); }
    public void DEBUG_AdelantarUnDia() { if (mascotaActiva == null) return; for (int i = 0; i < mascotaActiva.fechasVacunas.Count; i++) { if (!string.IsNullOrEmpty(mascotaActiva.fechasVacunas[i]) && DateTime.TryParse(mascotaActiva.fechasVacunas[i], out DateTime fechaGuardada)) mascotaActiva.fechasVacunas[i] = fechaGuardada.AddDays(-1).ToString("yyyy-MM-dd"); } if (!string.IsNullOrEmpty(mascotaActiva.fechaNacimiento) && DateTime.TryParse(mascotaActiva.fechaNacimiento, out DateTime fn)) mascotaActiva.fechaNacimiento = fn.AddDays(-1).ToString("yyyy-MM-dd"); GuardarEnMemoria(); ActualizarEstadoNotificacionesUI(); ActualizarTarjetasVisuales(); IrAVacunas(mascotaActiva); }
    public void DEBUG_SaltoEnElTiempo_19Dias() { if (mascotaActiva == null) return; for (int i = 0; i < mascotaActiva.fechasVacunas.Count; i++) { if (!string.IsNullOrEmpty(mascotaActiva.fechasVacunas[i]) && DateTime.TryParse(mascotaActiva.fechasVacunas[i], out DateTime fechaGuardada)) mascotaActiva.fechasVacunas[i] = fechaGuardada.AddDays(-19).ToString("yyyy-MM-dd"); } if (!string.IsNullOrEmpty(mascotaActiva.fechaNacimiento) && DateTime.TryParse(mascotaActiva.fechaNacimiento, out DateTime fn)) mascotaActiva.fechaNacimiento = fn.AddDays(-19).ToString("yyyy-MM-dd"); GuardarEnMemoria(); ActualizarEstadoNotificacionesUI(); ActualizarTarjetasVisuales(); IrAVacunas(mascotaActiva); }
    public void DEBUG_SaltoEnElTiempo_363Dias() { if (mascotaActiva == null) return; for (int i = 0; i < mascotaActiva.fechasVacunas.Count; i++) { if (!string.IsNullOrEmpty(mascotaActiva.fechasVacunas[i]) && DateTime.TryParse(mascotaActiva.fechasVacunas[i], out DateTime fechaGuardada)) mascotaActiva.fechasVacunas[i] = fechaGuardada.AddDays(-363).ToString("yyyy-MM-dd"); } if (!string.IsNullOrEmpty(mascotaActiva.fechaNacimiento) && DateTime.TryParse(mascotaActiva.fechaNacimiento, out DateTime fn)) mascotaActiva.fechaNacimiento = fn.AddDays(-363).ToString("yyyy-MM-dd"); GuardarEnMemoria(); ActualizarEstadoNotificacionesUI(); ActualizarTarjetasVisuales(); IrAVacunas(mascotaActiva); }
    public void DEBUG_SaltoEnElTiempo_1Año() { if (mascotaActiva == null) return; for (int i = 0; i < mascotaActiva.fechasVacunas.Count; i++) { if (!string.IsNullOrEmpty(mascotaActiva.fechasVacunas[i]) && DateTime.TryParse(mascotaActiva.fechasVacunas[i], out DateTime fechaGuardada)) mascotaActiva.fechasVacunas[i] = fechaGuardada.AddYears(-1).ToString("yyyy-MM-dd"); } if (!string.IsNullOrEmpty(mascotaActiva.fechaNacimiento) && DateTime.TryParse(mascotaActiva.fechaNacimiento, out DateTime fn)) mascotaActiva.fechaNacimiento = fn.AddYears(-1).ToString("yyyy-MM-dd"); GuardarEnMemoria(); ActualizarEstadoNotificacionesUI(); ActualizarTarjetasVisuales(); IrAVacunas(mascotaActiva); }
}

[Serializable]
public class GrupoAlertasVacuna { public string nombreParaOrganizar; public Sprite faltan2Dias; public Sprite falta1Dia; public Sprite esHoy; public Sprite atrasada; }

[Serializable]
public class Mascota { public string nombre, especie, edad, rutaFoto, fechaNacimiento; public List<bool> estadosVacunas = new List<bool>(); public List<string> fechasVacunas = new List<string>(); public List<string> rutasCarnet = new List<string>(); public List<string> fechasCarnet = new List<string>(); public bool perroMayor4Meses, perroMayor1Ano, gatoMayor2Anos; public bool notificacionesPausadas = false; public int notificacionOcultaID = 0; public int indiceVacunaPausada = -1; public int estadoAlertaPausada = 0; public bool traqueoFueNasal = false; }

[Serializable]
public class ListaMascotas { public List<Mascota> lista = new List<Mascota>(); }