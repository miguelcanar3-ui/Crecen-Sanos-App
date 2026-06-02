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
    public TMP_InputField inputAnos, inputMeses, inputSemanas;
    public Image cuadroFotoRegistro;
    public Sprite fotoPredeterminadaRegistro;
    public GameObject panelRegistro, panelPerfiles, panelAdvertencia;

    [Header("UI Bienvenida")]
    public GameObject panelBienvenida;
    public TextMeshProUGUI textoBienvenida;

    [Header("UI Vacunación PERROS")]
    public GameObject panelVacunasPerro;
    public TextMeshProUGUI textoNombrePerro;
    public Image imagenPerfilPerro;
    public Toggle[] togglesVacunasPerro;
    [Tooltip("El Elemento 0 no se usa (Puppy). Elemento 1 = 1ra Múltiple, Elemento 6 = Refuerzo")]
    public GrupoAlertasVacuna[] alertasPorVacuna = new GrupoAlertasVacuna[7];
    public GameObject panelPreguntaTraqueo;
    public GameObject vacunaSegundaDosisTraqueo;
    public GameObject vacunaRefuerzoAnual;

    [Header("UI Vacunación GATOS")]
    public GameObject panelVacunasGato;
    public TextMeshProUGUI textoNombreGato;
    public Image imagenPerfilGato;
    public Toggle[] togglesVacunasGato;
    [Tooltip("El Elemento 0 no se usa (1ra Múltiple). Elemento 5 = Refuerzo Anual")]
    public GrupoAlertasVacuna[] alertasPorVacunaGato = new GrupoAlertasVacuna[6];
    public GameObject vacunaRefuerzoAnualGato;

    [Header("UI Confirmación Vacuna")]
    public GameObject panelConfirmacionVacuna;
    public TextMeshProUGUI textoConfirmacionVacuna;
    private int indiceVacunaPendiente = -1;
    private bool nuevoEstadoVacunaPendiente = false;

    [Header("Barra Navegación Inferior (Tabs)")]
    public GameObject vistaCronogramaVacunas;
    public GameObject vistaAlimentacionPerro;
    public GameObject vistaAlimentacionGato;
    public Image imagenBotonVacuna;
    public Image imagenBotonComida;
    public Sprite iconoVacunaAzul, iconoVacunaGris;
    public Sprite iconoComidaAzul, iconoComidaGris;

    [Header("UI Recordatorios (Notificaciones)")]
    public GameObject panelRecordatorios;
    public GameObject tarjetaNotificacion;
    public Image imagenEstadoNotificacion;
    public GameObject textoSinNotificaciones;

    [Header("Icono Campana (Exterior)")]
    public Image iconoCampana;
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
        CargarMascotas();
        if (panelPreguntaTraqueo != null) panelPreguntaTraqueo.SetActive(false);
    }

    public void ValidarInputMeses()
    {
        if (inputMeses != null && !string.IsNullOrEmpty(inputMeses.text))
        {
            int m = 0;
            if (int.TryParse(inputMeses.text, out m) && m > 11) inputMeses.text = "11";
        }
    }

    public void ValidarInputSemanas()
    {
        if (inputSemanas != null && !string.IsNullOrEmpty(inputSemanas.text))
        {
            int s = 0;
            if (int.TryParse(inputSemanas.text, out s) && s > 3) inputSemanas.text = "3";
        }
    }

    private string ObtenerEdadDinamica(Mascota m)
    {
        if (string.IsNullOrEmpty(m.fechaNacimiento)) return m.edad;

        DateTime fn;
        if (DateTime.TryParse(m.fechaNacimiento, out fn))
        {
            DateTime hoy = DateTime.Now;
            if (hoy < fn) hoy = fn;

            int anos = hoy.Year - fn.Year;
            int meses = hoy.Month - fn.Month;
            int dias = hoy.Day - fn.Day;

            if (dias < 0)
            {
                meses--;
                dias += DateTime.DaysInMonth(hoy.AddMonths(-1).Year, hoy.AddMonths(-1).Month);
            }
            if (meses < 0)
            {
                anos--;
                meses += 12;
            }
            int semanas = dias / 7;
            if (semanas > 3) semanas = 3;

            return anos + " años, " + meses + " meses, " + semanas + " semanas";
        }
        return m.edad;
    }

    public void AbrirPanelRecordatorios()
    {
        if (mascotaActiva == null || panelRecordatorios == null) return;
        ActualizarEstadoNotificacionesUI();
        panelRecordatorios.SetActive(true);
        panelRecordatorios.transform.SetAsLastSibling();
    }

    public void CerrarPanelRecordatorios()
    {
        if (panelRecordatorios != null) panelRecordatorios.SetActive(false);
    }

    public void PausarNotificaciones()
    {
        if (mascotaActiva != null && !mascotaActiva.notificacionesPausadas)
        {
            mascotaActiva.notificacionesPausadas = true;
            mascotaActiva.indiceVacunaPausada = tempIndicePendiente;
            mascotaActiva.estadoAlertaPausada = tempEstadoAlerta;

            GuardarEnMemoria();
            ActualizarEstadoNotificacionesUI();
        }
    }

    public void ReanudarNotificaciones()
    {
        if (mascotaActiva != null && mascotaActiva.notificacionesPausadas)
        {
            mascotaActiva.notificacionesPausadas = false;
            mascotaActiva.indiceVacunaPausada = -1;
            mascotaActiva.estadoAlertaPausada = 0;

            GuardarEnMemoria();
            ActualizarEstadoNotificacionesUI();
        }
    }

    public void OcultarTarjetaManual()
    {
        if (mascotaActiva != null)
        {
            mascotaActiva.notificacionOcultaID = idNotificacionActual;
            GuardarEnMemoria();
            ActualizarEstadoNotificacionesUI();
        }
    }

    // ================================================================
    // VISIBILIDAD INTELIGENTE CORREGIDA (Ya no desaparece al marcar)
    // ================================================================
    private void ActualizarVisibilidadVacunasEspeciales(Mascota m)
    {
        if (m == null) return;

        if (m.especie == "Perro")
        {
            if (vacunaSegundaDosisTraqueo != null)
            {
                bool rabiaMarcada = m.estadosVacunas.Count > 4 ? m.estadosVacunas[4] : false;
                vacunaSegundaDosisTraqueo.SetActive(rabiaMarcada && !m.traqueoFueNasal);
            }

            if (vacunaRefuerzoAnual != null)
            {
                bool mostrarRefuerzo = false;
                if (m.estadosVacunas.Count > 6)
                {
                    // Si alguna vez se marcó (tiene fecha) o está marcado ahora, se queda visible SIEMPRE.
                    if (m.estadosVacunas[6] == true || !string.IsNullOrEmpty(m.fechasVacunas[6]))
                    {
                        mostrarRefuerzo = true;
                    }
                    else if (m.estadosVacunas[4] == true) // Primer año de vida
                    {
                        DateTime fechaRabia;
                        if (!string.IsNullOrEmpty(m.fechasVacunas[4]) && DateTime.TryParse(m.fechasVacunas[4], out fechaRabia))
                        {
                            // Mostramos 2 días antes del año (Ignorando bisiestos con AddYears)
                            DateTime fechaAviso = fechaRabia.Date.AddYears(1).AddDays(-2);
                            if (DateTime.Now.Date >= fechaAviso) mostrarRefuerzo = true;
                        }
                    }
                }
                vacunaRefuerzoAnual.SetActive(mostrarRefuerzo);
            }
        }
        else if (m.especie == "Gato")
        {
            if (vacunaRefuerzoAnualGato != null)
            {
                bool mostrarRefuerzo = false;
                if (m.estadosVacunas.Count > 5)
                {
                    if (m.estadosVacunas[5] == true || !string.IsNullOrEmpty(m.fechasVacunas[5]))
                    {
                        mostrarRefuerzo = true;
                    }
                    else if (m.estadosVacunas[4] == true)
                    {
                        DateTime fechaRabia;
                        if (!string.IsNullOrEmpty(m.fechasVacunas[4]) && DateTime.TryParse(m.fechasVacunas[4], out fechaRabia))
                        {
                            DateTime fechaAviso = fechaRabia.Date.AddYears(1).AddDays(-2);
                            if (DateTime.Now.Date >= fechaAviso) mostrarRefuerzo = true;
                        }
                    }
                }
                vacunaRefuerzoAnualGato.SetActive(mostrarRefuerzo);
            }
        }
    }

    // ================================================================
    // MOTOR DE NOTIFICACIONES (Años exactos y Bisiestos arreglados)
    // ================================================================
    private void ActualizarEstadoNotificacionesUI()
    {
        if (mascotaActiva == null) return;

        // --- VIGILANTES DE REFUERZO ANUAL (Reseteo exacto a un año) ---
        if (mascotaActiva.especie == "Perro" && mascotaActiva.estadosVacunas.Count > 6)
        {
            if (mascotaActiva.estadosVacunas[6] == true && !string.IsNullOrEmpty(mascotaActiva.fechasVacunas[6]))
            {
                if (DateTime.TryParse(mascotaActiva.fechasVacunas[6], out DateTime fechaUltimoRefuerzo))
                {
                    // Desmarca automáticamente 2 días antes de que se cumpla el año exacto (AddYears)
                    DateTime fechaAviso = fechaUltimoRefuerzo.Date.AddYears(1).AddDays(-2);
                    if (DateTime.Now.Date >= fechaAviso)
                    {
                        mascotaActiva.estadosVacunas[6] = false; GuardarEnMemoria();
                        if (togglesVacunasPerro.Length > 6 && togglesVacunasPerro[6] != null)
                        {
                            cargandoVacunas = true; togglesVacunasPerro[6].isOn = false; togglesVacunasPerro[6].interactable = true; cargandoVacunas = false;
                        }
                    }
                }
            }
        }
        else if (mascotaActiva.especie == "Gato" && mascotaActiva.estadosVacunas.Count > 5)
        {
            if (mascotaActiva.estadosVacunas[5] == true && !string.IsNullOrEmpty(mascotaActiva.fechasVacunas[5]))
            {
                if (DateTime.TryParse(mascotaActiva.fechasVacunas[5], out DateTime fechaUltimoRefuerzo))
                {
                    DateTime fechaAviso = fechaUltimoRefuerzo.Date.AddYears(1).AddDays(-2);
                    if (DateTime.Now.Date >= fechaAviso)
                    {
                        mascotaActiva.estadosVacunas[5] = false; GuardarEnMemoria();
                        if (togglesVacunasGato.Length > 5 && togglesVacunasGato[5] != null)
                        {
                            cargandoVacunas = true; togglesVacunasGato[5].isOn = false; togglesVacunasGato[5].interactable = true; cargandoVacunas = false;
                        }
                    }
                }
            }
        }

        ActualizarVisibilidadVacunasEspeciales(mascotaActiva);

        idNotificacionActual = 0;
        Sprite spriteAMostrar = null;
        tempIndicePendiente = -1;
        tempEstadoAlerta = 0;

        int limiteVacunas = (mascotaActiva.especie == "Perro") ? 6 : 5;

        if (mascotaActiva.estadosVacunas.Count >= limiteVacunas + 1)
        {
            int indicePendiente = -1;
            int indiceAnterior = -1;

            for (int i = 1; i <= limiteVacunas; i++)
            {
                if (mascotaActiva.estadosVacunas[i] == false)
                {
                    if (mascotaActiva.especie == "Perro" && i == 5 && mascotaActiva.traqueoFueNasal) continue;
                    if (i == limiteVacunas && mascotaActiva.estadosVacunas[limiteVacunas - 2] == false) continue;

                    indicePendiente = i;

                    if (i == limiteVacunas)
                    {
                        indiceAnterior = !string.IsNullOrEmpty(mascotaActiva.fechasVacunas[limiteVacunas]) ? limiteVacunas : limiteVacunas - 2;
                    }
                    else
                    {
                        indiceAnterior = i - 1;
                    }
                    break;
                }
            }

            if (indicePendiente != -1 && indiceAnterior != -1 && !string.IsNullOrEmpty(mascotaActiva.fechasVacunas[indiceAnterior]))
            {
                if (DateTime.TryParse(mascotaActiva.fechasVacunas[indiceAnterior], out DateTime fechaBase))
                {
                    DateTime hoy = DateTime.Now.Date;
                    DateTime fechaBaseSoloDia = fechaBase.Date;

                    int diasRestantes = 0;

                    // Lógica exacta de Años vs Días
                    if (indicePendiente == limiteVacunas)
                    {
                        DateTime fechaSiguienteAnual = fechaBaseSoloDia.AddYears(1);
                        diasRestantes = (int)(fechaSiguienteAnual - hoy).TotalDays;
                    }
                    else
                    {
                        DateTime fechaSiguienteNormal = fechaBaseSoloDia.AddDays(21);
                        diasRestantes = (int)(fechaSiguienteNormal - hoy).TotalDays;
                    }

                    tempIndicePendiente = indicePendiente;

                    if (diasRestantes < -7) tempEstadoAlerta = 4;
                    else if (diasRestantes <= 0 && diasRestantes >= -7) tempEstadoAlerta = 3;
                    else if (diasRestantes == 1) tempEstadoAlerta = 2;
                    else if (diasRestantes == 2) tempEstadoAlerta = 1;
                }
            }
        }

        int indiceAUsar = mascotaActiva.notificacionesPausadas ? mascotaActiva.indiceVacunaPausada : tempIndicePendiente;
        int estadoAUsar = mascotaActiva.notificacionesPausadas ? mascotaActiva.estadoAlertaPausada : tempEstadoAlerta;

        GrupoAlertasVacuna[] arrayAlertasAUsar = (mascotaActiva.especie == "Perro") ? alertasPorVacuna : alertasPorVacunaGato;

        if (indiceAUsar != -1 && estadoAUsar != 0 && indiceAUsar < arrayAlertasAUsar.Length && arrayAlertasAUsar[indiceAUsar] != null)
        {
            idNotificacionActual = indiceAUsar * 1000 + estadoAUsar;

            if (estadoAUsar == 4) spriteAMostrar = arrayAlertasAUsar[indiceAUsar].atrasada;
            else if (estadoAUsar == 3) spriteAMostrar = arrayAlertasAUsar[indiceAUsar].esHoy;
            else if (estadoAUsar == 2) spriteAMostrar = arrayAlertasAUsar[indiceAUsar].falta1Dia;
            else if (estadoAUsar == 1) spriteAMostrar = arrayAlertasAUsar[indiceAUsar].faltan2Dias;
        }

        if (idNotificacionActual == 0 || mascotaActiva.notificacionOcultaID == idNotificacionActual)
        {
            if (tarjetaNotificacion != null) tarjetaNotificacion.SetActive(false);
            if (textoSinNotificaciones != null) textoSinNotificaciones.SetActive(true);
            if (iconoCampana != null && spriteCampanaNormal != null) iconoCampana.sprite = spriteCampanaNormal;
        }
        else
        {
            if (tarjetaNotificacion != null) tarjetaNotificacion.SetActive(true);
            if (textoSinNotificaciones != null) textoSinNotificaciones.SetActive(false);
            if (imagenEstadoNotificacion != null && spriteAMostrar != null) imagenEstadoNotificacion.sprite = spriteAMostrar;
            if (iconoCampana != null && spriteCampanaAlerta != null) iconoCampana.sprite = spriteCampanaAlerta;
        }

        if (mascotaActiva.notificacionesPausadas)
        {
            if (btnPausarNotificaciones != null) { btnPausarNotificaciones.interactable = false; btnPausarNotificaciones.GetComponent<Image>().sprite = spritePausarGris; }
            if (btnReanudarNotificaciones != null) { btnReanudarNotificaciones.interactable = true; btnReanudarNotificaciones.GetComponent<Image>().sprite = spriteReanudarAzul; }
        }
        else
        {
            if (btnPausarNotificaciones != null) { btnPausarNotificaciones.interactable = true; btnPausarNotificaciones.GetComponent<Image>().sprite = spritePausarAzul; }
            if (btnReanudarNotificaciones != null) { btnReanudarNotificaciones.interactable = false; btnReanudarNotificaciones.GetComponent<Image>().sprite = spriteReanudarGris; }
        }
    }

    public void SeleccionoNasal()
    {
        ConfirmarCambioVacuna();
        if (mascotaActiva != null) { mascotaActiva.traqueoFueNasal = true; GuardarEnMemoria(); }
        if (panelPreguntaTraqueo != null) panelPreguntaTraqueo.SetActive(false);
        ActualizarEstadoNotificacionesUI();
    }

    public void SeleccionoInyeccion()
    {
        ConfirmarCambioVacuna();
        if (mascotaActiva != null) { mascotaActiva.traqueoFueNasal = false; GuardarEnMemoria(); }
        if (panelPreguntaTraqueo != null) panelPreguntaTraqueo.SetActive(false);
        ActualizarEstadoNotificacionesUI();
    }

    public void ClickPestanaVacunas()
    {
        if (vistaAlimentacionPerro != null) vistaAlimentacionPerro.SetActive(false);
        if (vistaAlimentacionGato != null) vistaAlimentacionGato.SetActive(false);

        if (vistaCronogramaVacunas != null) { vistaCronogramaVacunas.SetActive(true); vistaCronogramaVacunas.transform.SetAsLastSibling(); }
        if (imagenBotonVacuna != null && iconoVacunaAzul != null) imagenBotonVacuna.sprite = iconoVacunaAzul;
        if (imagenBotonComida != null && iconoComidaGris != null) imagenBotonComida.sprite = iconoComidaGris;
    }

    public void ClickPestanaComida()
    {
        if (vistaCronogramaVacunas != null) vistaCronogramaVacunas.SetActive(false);

        if (mascotaActiva != null && mascotaActiva.especie == "Gato")
        {
            if (vistaAlimentacionPerro != null) vistaAlimentacionPerro.SetActive(false);
            if (vistaAlimentacionGato != null) { vistaAlimentacionGato.SetActive(true); vistaAlimentacionGato.transform.SetAsLastSibling(); }
        }
        else
        {
            if (vistaAlimentacionGato != null) vistaAlimentacionGato.SetActive(false);
            if (vistaAlimentacionPerro != null) { vistaAlimentacionPerro.SetActive(true); vistaAlimentacionPerro.transform.SetAsLastSibling(); }
        }

        if (imagenBotonVacuna != null && iconoVacunaGris != null) imagenBotonVacuna.sprite = iconoVacunaGris;
        if (imagenBotonComida != null && iconoComidaAzul != null) imagenBotonComida.sprite = iconoComidaAzul;
    }

    public void VolverAListaPerfiles()
    {
        if (vistaCronogramaVacunas != null) vistaCronogramaVacunas.SetActive(false);
        if (vistaAlimentacionPerro != null) vistaAlimentacionPerro.SetActive(false);
        if (vistaAlimentacionGato != null) vistaAlimentacionGato.SetActive(false);

        if (panelVacunasPerro != null) panelVacunasPerro.SetActive(false);
        if (panelVacunasGato != null) panelVacunasGato.SetActive(false);

        if (panelPerfiles != null) { panelPerfiles.SetActive(true); panelPerfiles.transform.SetAsLastSibling(); }
        if (imagenBotonVacuna != null && iconoVacunaGris != null) imagenBotonVacuna.sprite = iconoVacunaGris;
        if (imagenBotonComida != null && iconoComidaGris != null) imagenBotonComida.sprite = iconoComidaGris;
        ActualizarTarjetasVisuales();
    }

    public void AbrirPanelRegistro()
    {
        mascotaActiva = null;
        if (inputNombre != null) inputNombre.text = "";
        if (inputAnos != null) inputAnos.text = "";
        if (inputMeses != null) inputMeses.text = "";
        if (inputSemanas != null) inputSemanas.text = "";
        rutaFotoActual = "";
        especieSeleccionada = "";

        if (cuadroFotoRegistro != null && fotoPredeterminadaRegistro != null) cuadroFotoRegistro.sprite = fotoPredeterminadaRegistro;
        if (panelRegistro != null) { panelRegistro.SetActive(true); panelRegistro.transform.SetAsLastSibling(); }
    }

    public void SeleccionarPerro() { especieSeleccionada = "Perro"; }
    public void SeleccionarGato() { especieSeleccionada = "Gato"; }

    public void AbrirGaleria() { NativeGallery.GetImageFromGallery((path) => { CargarImagenEnCuadro(path); }, "Foto", "image/*"); }
    public void AbrirCamara() { NativeCamera.TakePicture((path) => { CargarImagenEnCuadro(path); }, 512); }

    private void CargarImagenEnCuadro(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        Texture2D texture = NativeGallery.LoadImageAtPath(path, 512);
        if (texture != null && cuadroFotoRegistro != null)
        {
            cuadroFotoRegistro.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            rutaFotoActual = path;
        }
    }

    public void GuardarYContinuar()
    {
        if (inputNombre == null || string.IsNullOrEmpty(inputNombre.text) || especieSeleccionada == "")
        {
            if (panelAdvertencia != null) { panelAdvertencia.SetActive(true); panelAdvertencia.transform.SetAsLastSibling(); }
            return;
        }
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

        todasLasMascotas.Add(nueva); GuardarEnMemoria(); ActualizarTarjetasVisuales();
        if (panelRegistro != null) panelRegistro.SetActive(false);
        MostrarBienvenida(nueva);
    }

    public void CerrarAdvertencia() { if (panelAdvertencia != null) panelAdvertencia.SetActive(false); }

    public void MostrarBienvenida(Mascota m)
    {
        mascotaActiva = m;
        if (textoBienvenida != null) textoBienvenida.text = "¡Bienvenido, " + m.nombre + "!";
        if (panelBienvenida != null) { panelBienvenida.SetActive(true); panelBienvenida.transform.SetAsLastSibling(); }
        else AceptarBienvenida();
    }

    public void AceptarBienvenida()
    {
        if (panelBienvenida != null) panelBienvenida.SetActive(false);
        if (mascotaActiva != null) IrAVacunas(mascotaActiva);
    }

    public void AbrirPanelEdicion()
    {
        if (mascotaActiva == null || panelEdicion == null) return;
        panelEdicion.SetActive(true); panelEdicion.transform.SetAsLastSibling();
        if (textoTituloEdicion != null) textoTituloEdicion.text = mascotaActiva.nombre;
        if (textoEdadEdicion != null) textoEdadEdicion.text = ObtenerEdadDinamica(mascotaActiva);
        if (inputNombreEdicion != null) inputNombreEdicion.text = mascotaActiva.nombre;
        rutaFotoEdicion = mascotaActiva.rutaFoto;
        if (!string.IsNullOrEmpty(rutaFotoEdicion)) CargarImagenEnEditor(rutaFotoEdicion);
    }

    public void AbrirGaleriaEdicion() { NativeGallery.GetImageFromGallery((path) => { CargarImagenEnEditor(path); }, "Foto", "image/*"); }
    public void AbrirCamaraEdicion() { NativeCamera.TakePicture((path) => { CargarImagenEnEditor(path); }, 512); }

    private void CargarImagenEnEditor(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        Texture2D texture = NativeGallery.LoadImageAtPath(path, 512);
        if (texture != null && imagenPerfilEdicion != null)
        {
            imagenPerfilEdicion.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            rutaFotoEdicion = path;
        }
    }

    public void GuardarCambiosEdicion()
    {
        if (inputNombreEdicion == null || string.IsNullOrEmpty(inputNombreEdicion.text)) return;
        mascotaActiva.nombre = inputNombreEdicion.text; mascotaActiva.rutaFoto = rutaFotoEdicion;
        GuardarEnMemoria(); ActualizarTarjetasVisuales(); IrAVacunas(mascotaActiva);
        if (panelEdicion != null) panelEdicion.SetActive(false);
    }

    public void CancelarEdicion() { if (panelEdicion != null) panelEdicion.SetActive(false); }

    public void IrAVacunas(Mascota m)
    {
        mascotaActiva = m;
        if (panelPerfiles != null) panelPerfiles.SetActive(false);
        cargandoVacunas = true;

        ClickPestanaVacunas();
        ActualizarEstadoNotificacionesUI();

        if (m.especie == "Perro")
        {
            if (textoNombrePerro != null) textoNombrePerro.text = m.nombre;
            if (imagenPerfilPerro != null && !string.IsNullOrEmpty(m.rutaFoto))
            {
                try
                {
                    Texture2D tex = NativeGallery.LoadImageAtPath(m.rutaFoto, 256);
                    if (tex) imagenPerfilPerro.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
                catch { }
            }
            if (togglesVacunasPerro != null)
            {
                for (int i = 0; i < togglesVacunasPerro.Length; i++)
                {
                    if (togglesVacunasPerro[i] != null && i < m.estadosVacunas.Count)
                    {
                        togglesVacunasPerro[i].isOn = m.estadosVacunas[i];
                        togglesVacunasPerro[i].interactable = !m.estadosVacunas[i];
                    }
                }
            }
            if (panelVacunasGato != null) panelVacunasGato.SetActive(false);
            if (panelVacunasPerro != null) { panelVacunasPerro.SetActive(true); panelVacunasPerro.transform.SetAsLastSibling(); }
        }
        else
        {
            if (textoNombreGato != null) textoNombreGato.text = m.nombre;
            if (imagenPerfilGato != null && !string.IsNullOrEmpty(m.rutaFoto))
            {
                try
                {
                    Texture2D tex = NativeGallery.LoadImageAtPath(m.rutaFoto, 256);
                    if (tex) imagenPerfilGato.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
                catch { }
            }
            if (togglesVacunasGato != null)
            {
                for (int i = 0; i < togglesVacunasGato.Length; i++)
                {
                    if (togglesVacunasGato[i] != null && i < m.estadosVacunas.Count)
                    {
                        togglesVacunasGato[i].isOn = m.estadosVacunas[i];
                        togglesVacunasGato[i].interactable = !m.estadosVacunas[i];
                    }
                }
            }
            if (panelVacunasPerro != null) panelVacunasPerro.SetActive(false);
            if (panelVacunasGato != null) { panelVacunasGato.SetActive(true); panelVacunasGato.transform.SetAsLastSibling(); }
        }

        cargandoVacunas = false;
    }

    public void GuardarEstadoVacunas()
    {
        if (mascotaActiva == null || cargandoVacunas) return;

        Toggle[] togglesActuales = (mascotaActiva.especie == "Perro") ? togglesVacunasPerro : togglesVacunasGato;

        for (int i = 0; i < togglesActuales.Length; i++)
        {
            if (togglesActuales[i] == null) continue;

            while (mascotaActiva.estadosVacunas.Count <= i) { mascotaActiva.estadosVacunas.Add(false); mascotaActiva.fechasVacunas.Add(""); }

            if (togglesActuales[i].isOn != mascotaActiva.estadosVacunas[i])
            {
                indiceVacunaPendiente = i;
                nuevoEstadoVacunaPendiente = togglesActuales[i].isOn;

                cargandoVacunas = true;
                togglesActuales[i].isOn = mascotaActiva.estadosVacunas[i];
                cargandoVacunas = false;

                if (i == 4 && mascotaActiva.especie == "Perro" && nuevoEstadoVacunaPendiente == true)
                {
                    if (panelPreguntaTraqueo != null) { panelPreguntaTraqueo.SetActive(true); panelPreguntaTraqueo.transform.SetAsLastSibling(); }
                }
                else
                {
                    if (textoConfirmacionVacuna != null)
                    {
                        string nombreLimpio = togglesActuales[i].gameObject.name.Replace("toogle", "").Replace("Toggle", "").Trim();
                        string animalTexto = (mascotaActiva.especie == "Perro") ? "perrito" : "gatito";

                        string parrafo1 = "Has marcado que tu " + animalTexto + " ya recibió la " + nombreLimpio + ".";
                        string parrafo2 = "<color=#666666>Recibirás una notificación diaria dos días antes de la fecha recomendada para la siguiente dosis.</color>";
                        string parrafo3 = "<color=#666666>Puedes desactivar estas notificaciones en la sección de notificaciones.</color>";

                        textoConfirmacionVacuna.text = parrafo1 + "\n\n" + parrafo2 + "\n\n" + parrafo3;
                    }

                    if (panelConfirmacionVacuna != null) { panelConfirmacionVacuna.SetActive(true); panelConfirmacionVacuna.transform.SetAsLastSibling(); }
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
            mascotaActiva.notificacionOcultaID = 0;
            mascotaActiva.fechasVacunas[indiceVacunaPendiente] = DateTime.Now.ToString("yyyy-MM-dd");

            if (mascotaActiva.notificacionesPausadas)
            {
                mascotaActiva.notificacionesPausadas = false;
                mascotaActiva.indiceVacunaPausada = -1;
                mascotaActiva.estadoAlertaPausada = 0;
            }
        }

        GuardarEnMemoria();
        ActualizarEstadoNotificacionesUI();

        Toggle[] togglesActuales = (mascotaActiva.especie == "Perro") ? togglesVacunasPerro : togglesVacunasGato;
        if (togglesActuales != null && indiceVacunaPendiente < togglesActuales.Length && togglesActuales[indiceVacunaPendiente] != null)
        {
            cargandoVacunas = true;
            togglesActuales[indiceVacunaPendiente].isOn = nuevoEstadoVacunaPendiente;
            if (nuevoEstadoVacunaPendiente) togglesActuales[indiceVacunaPendiente].interactable = false;
            cargandoVacunas = false;
        }

        indiceVacunaPendiente = -1;
        if (panelConfirmacionVacuna != null) panelConfirmacionVacuna.SetActive(false);
    }

    public void CancelarCambioVacuna()
    {
        if (indiceVacunaPendiente != -1 && mascotaActiva != null)
        {
            Toggle[] togglesActuales = (mascotaActiva.especie == "Perro") ? togglesVacunasPerro : togglesVacunasGato;
            if (togglesActuales != null && indiceVacunaPendiente < togglesActuales.Length && togglesActuales[indiceVacunaPendiente] != null)
            {
                cargandoVacunas = true;
                togglesActuales[indiceVacunaPendiente].isOn = mascotaActiva.estadosVacunas[indiceVacunaPendiente];
                cargandoVacunas = false;
            }
        }
        indiceVacunaPendiente = -1;
        if (panelConfirmacionVacuna != null) panelConfirmacionVacuna.SetActive(false);
        if (panelPreguntaTraqueo != null) panelPreguntaTraqueo.SetActive(false);
    }

    void ActualizarTarjetasVisuales()
    {
        if (contenedorPerfiles == null) return;
        foreach (Transform child in contenedorPerfiles) Destroy(child.gameObject);
        foreach (Mascota m in todasLasMascotas) CrearTarjetaPerfil(m);
    }

    void CrearTarjetaPerfil(Mascota m)
    {
        if (prefabPerfil == null || contenedorPerfiles == null) return;
        GameObject tarjeta = Instantiate(prefabPerfil, contenedorPerfiles);
        Transform txtN = tarjeta.transform.Find("TextoNombre");
        if (txtN != null) txtN.GetComponent<TextMeshProUGUI>().text = m.nombre;

        Transform txtE = tarjeta.transform.Find("TextoEdad");
        if (txtE != null) txtE.GetComponent<TextMeshProUGUI>().text = ObtenerEdadDinamica(m);

        Transform imgF = tarjeta.transform.Find("Foto");
        if (imgF && !string.IsNullOrEmpty(m.rutaFoto))
        {
            try
            {
                Texture2D tex = NativeGallery.LoadImageAtPath(m.rutaFoto, 256);
                if (tex) imgF.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            catch { }
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
                    if (panelConfirmacion != null) { panelConfirmacion.SetActive(true); panelConfirmacion.transform.SetAsLastSibling(); }
                });
            }
        }
    }

    public void ConfirmarEliminacion()
    {
        todasLasMascotas.Remove(mascotaAEliminar);
        GuardarEnMemoria(); ActualizarTarjetasVisuales();
        if (panelConfirmacion != null) panelConfirmacion.SetActive(false);
        if (panelVacunasPerro != null) panelVacunasPerro.SetActive(false);
        if (panelVacunasGato != null) panelVacunasGato.SetActive(false);
        if (panelPerfiles != null) { panelPerfiles.SetActive(true); panelPerfiles.transform.SetAsLastSibling(); }
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
            foreach (Mascota m in todasLasMascotas)
            {
                if (string.IsNullOrEmpty(m.fechaNacimiento)) m.fechaNacimiento = DateTime.Now.ToString("yyyy-MM-dd");
            }
            GuardarEnMemoria(); ActualizarTarjetasVisuales();
        }
    }

    // ================================================================
    // BOTONES HACKER (TESTING CON AddYears() EN LUGAR DE DÍAS)
    // ================================================================
    public void DEBUG_BorrarTodaLaMemoria()
    {
        PlayerPrefs.DeleteAll(); PlayerPrefs.Save(); todasLasMascotas.Clear(); ActualizarTarjetasVisuales();
        Debug.Log("¡Toda la memoria borrada!");
    }

    public void DEBUG_SaltoEnElTiempo_19Dias()
    {
        if (mascotaActiva == null) return;
        for (int i = 0; i < mascotaActiva.fechasVacunas.Count; i++)
        {
            if (!string.IsNullOrEmpty(mascotaActiva.fechasVacunas[i]) && DateTime.TryParse(mascotaActiva.fechasVacunas[i], out DateTime fechaGuardada))
                mascotaActiva.fechasVacunas[i] = fechaGuardada.AddDays(-19).ToString("yyyy-MM-dd");
        }
        if (!string.IsNullOrEmpty(mascotaActiva.fechaNacimiento) && DateTime.TryParse(mascotaActiva.fechaNacimiento, out DateTime fn))
            mascotaActiva.fechaNacimiento = fn.AddDays(-19).ToString("yyyy-MM-dd");

        GuardarEnMemoria(); ActualizarEstadoNotificacionesUI(); ActualizarTarjetasVisuales(); IrAVacunas(mascotaActiva);
    }

    public void DEBUG_SaltoEnElTiempo_1Año()
    {
        if (mascotaActiva == null) return;

        // Ahora usamos AddYears(-1) en lugar de AddDays(-365) para respetar los años bisiestos.
        for (int i = 0; i < mascotaActiva.fechasVacunas.Count; i++)
        {
            if (!string.IsNullOrEmpty(mascotaActiva.fechasVacunas[i]) && DateTime.TryParse(mascotaActiva.fechasVacunas[i], out DateTime fechaGuardada))
                mascotaActiva.fechasVacunas[i] = fechaGuardada.AddYears(-1).ToString("yyyy-MM-dd");
        }
        if (!string.IsNullOrEmpty(mascotaActiva.fechaNacimiento) && DateTime.TryParse(mascotaActiva.fechaNacimiento, out DateTime fn))
            mascotaActiva.fechaNacimiento = fn.AddYears(-1).ToString("yyyy-MM-dd");

        GuardarEnMemoria(); ActualizarEstadoNotificacionesUI(); ActualizarTarjetasVisuales(); IrAVacunas(mascotaActiva);
    }

    public void DEBUG_AdelantarUnDia()
    {
        if (mascotaActiva == null) return;
        for (int i = 0; i < mascotaActiva.fechasVacunas.Count; i++)
        {
            if (!string.IsNullOrEmpty(mascotaActiva.fechasVacunas[i]) && DateTime.TryParse(mascotaActiva.fechasVacunas[i], out DateTime fechaGuardada))
                mascotaActiva.fechasVacunas[i] = fechaGuardada.AddDays(-1).ToString("yyyy-MM-dd");
        }
        if (!string.IsNullOrEmpty(mascotaActiva.fechaNacimiento) && DateTime.TryParse(mascotaActiva.fechaNacimiento, out DateTime fn))
            mascotaActiva.fechaNacimiento = fn.AddDays(-1).ToString("yyyy-MM-dd");

        GuardarEnMemoria(); ActualizarEstadoNotificacionesUI(); ActualizarTarjetasVisuales(); IrAVacunas(mascotaActiva);
    }
}

// ================================================================
// CLASES DE SERIALIZACIÓN
// ================================================================
[Serializable]
public class GrupoAlertasVacuna
{
    public string nombreParaOrganizar;
    public Sprite faltan2Dias;
    public Sprite falta1Dia;
    public Sprite esHoy;
    public Sprite atrasada;
}

[Serializable]
public class Mascota
{
    public string nombre, especie, edad, rutaFoto;
    public string fechaNacimiento;
    public List<bool> estadosVacunas = new List<bool>();
    public List<string> fechasVacunas = new List<string>();

    public bool notificacionesPausadas = false;
    public int notificacionOcultaID = 0;
    public int indiceVacunaPausada = -1;
    public int estadoAlertaPausada = 0;

    public bool traqueoFueNasal = false;
    public bool dosis2Expirada = false;
}

[Serializable]
public class ListaMascotas { public List<Mascota> lista = new List<Mascota>(); }