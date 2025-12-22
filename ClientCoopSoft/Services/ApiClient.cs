using ClientCoopSoft.DTO;
using ClientCoopSoft.DTO.Asistencia;
using ClientCoopSoft.DTO.Capacitaciones;
using ClientCoopSoft.DTO.Contratacion;
using ClientCoopSoft.DTO.Extras;
using ClientCoopSoft.DTO.Faltas;
using ClientCoopSoft.DTO.FormacionAcademica;
using ClientCoopSoft.DTO.Historicos;
using ClientCoopSoft.DTO.Huellas;
using ClientCoopSoft.DTO.Licencias;
using ClientCoopSoft.DTO.Personas;
using ClientCoopSoft.DTO.Planillas;
using ClientCoopSoft.DTO.Trabajadores;
using ClientCoopSoft.DTO.Vacaciones;
using ClientCoopSoft.Models;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Windows;

public class ApiClient
{
    private readonly HttpClient _http;
    public string JwtToken { get; private set; } = string.Empty;

    public ApiClient(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    // LOGIN
    public async Task<AuthResponse?> LoginAsync(string username, string password)
    {
        var payload = new { nombreUsuario = username, Password = password };
        var resp = await _http.PostAsJsonAsync("api/auth/login", payload);
        var raw = await resp.Content.ReadAsStringAsync();
        System.Diagnostics.Debug.WriteLine($"Login response: {resp.StatusCode} - {raw}");

        if (!resp.IsSuccessStatusCode) return null;

        var auth = JsonConvert.DeserializeObject<AuthResponse>(raw);
        if (auth != null && !string.IsNullOrWhiteSpace(auth.Token))
        {
            JwtToken = auth.Token;
            SetBearer(); // importantísimo
            System.Diagnostics.Debug.WriteLine("Token set in ApiClient: " + JwtToken);
        }
        return auth;
    }

    // OBTENER HUELLA
    public async Task<string?> ObtenerHuellaXmlAsync(int idPersona)
    {
        try
        {
            var response = await _http.GetAsync($"api/personas/{idPersona}/huella");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

        return null;
    }

    // SETEAR JWT BEARER
    public void SetBearer()
    {
        if (!string.IsNullOrWhiteSpace(JwtToken))
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", JwtToken);
        else
            _http.DefaultRequestHeaders.Authorization = null;
    }

    #region USUARIOS
    // OBTENER USUARIOS
    public async Task<List<Usuario>?> GetUsuariosAsync()
    {
        SetBearer();
        var request = new HttpRequestMessage(HttpMethod.Get, "api/usuarios");
        var response = await _http.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return JsonConvert.DeserializeObject<List<Usuario>>(raw);
        return null;
    }
    // CREAR USUARIOS
    public async Task<bool> CrearUsuarioAsync(UsuarioCrearDTO dto)
    {
        try
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("api/usuarios", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    // ACTUALIZAR USUARIOS
    public async Task<bool> ActualizarUsuarioAsync(int id, UsuarioEditarDTO dto)
    {
        SetBearer();
        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _http.PutAsync($"api/usuarios/{id}", content);
        return response.IsSuccessStatusCode;
    }
    // ELIMINAR USUARIOS
    public async Task<bool> EliminarUsuarioAsync(int idUsuario)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/usuarios/{idUsuario}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    #endregion

    public async Task<List<Persona>?> ObtenerPersonasAsync()
    {
        SetBearer();
        var request = new HttpRequestMessage(HttpMethod.Get, "api/personas");
        var response = await _http.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<Persona>>(raw);
        }
        return null;
    }

    public async Task<List<Cargo>?> ObtenerCargosAsync()
    {
        SetBearer();
        var request = new HttpRequestMessage(HttpMethod.Get, "api/cargos");
        var response = await _http.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<Cargo>>(raw);
        }
        return null;
    }
    public async Task<List<Trabajador>?> ObtenerTrabajadoresAsync()
    {
        SetBearer();
        var request = new HttpRequestMessage(HttpMethod.Get, "api/trabajadores");
        var resp = await _http.SendAsync(request);
        var raw = await resp.Content.ReadAsStringAsync();

        if (resp.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<Trabajador>>(raw);
        }
        return null;
    }
    
    public async Task<bool> CrearPersonaAsync(PersonaCrearDTO dto)
    {
        try
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("api/personas", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CrearTrabajadorAsync(TrabajadorCrearDTO dto)
    {
        try
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("api/trabajadores", content);
            return response.IsSuccessStatusCode;
        }
        catch 
        {
            return false;
        }
    }



    public async Task<bool> EditarPersonaAsync(int id, Persona dto)
    {
        SetBearer();
        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _http.PutAsync($"api/personas/{id}", content);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<bool> EliminarPersonaAsync(int idPersona)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/personas/{idPersona}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    public async Task<Persona?> ObtenerPersonaAsync(int idPersona)
    {
        SetBearer();
        var response = await _http.GetAsync($"api/personas/{idPersona}");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return JsonConvert.DeserializeObject<Persona>(raw);
        return null;
    }

    public async Task<List<Rol>?> ObtenerRolesAsync()
    {
        SetBearer();
        var request = new HttpRequestMessage(HttpMethod.Get, "api/roles");
        var response = await _http.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<Rol>>(raw);
        }
        return null;
    }
    public async Task<List<bool>?> ObtenerGenerosAsync()
    {
        SetBearer();
        var request = new HttpRequestMessage(HttpMethod.Get, "api/generos");
        var response = await _http.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<bool>>(raw);
        }
        return null;
    }
    public async Task<List<Nacionalidad>?> ObtenerNacionalidadesAsync()
    {
        SetBearer();
        var response = await _http.GetAsync("api/nacionalidades");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<Nacionalidad>>(raw);
        }
        return null;
    }
    public async Task<List<PeriodoPago>?> ObtenerPeriodosPagoAsync()
    {
        SetBearer();
        var response = await _http.GetAsync("api/PeriodosPago");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<PeriodoPago>>(raw);
        }
        return null;
    }
    public async Task<List<TipoContrato>?> ObtenerTipoContratosAsync()
    {
        SetBearer();
        var response = await _http.GetAsync("api/TipoContratos");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<TipoContrato>>(raw);
        }
        return null;
    }

    public async Task<List<SolicitudVacacion>?> ObtenerVacacionesAsync()
    {
        SetBearer();
        var response = await _http.GetAsync("api/Vacaciones/SolicitudesCalendario");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return JsonConvert.DeserializeObject<List<SolicitudVacacion>>(raw);

        return null;
    }
    public async Task<List<SolicitudVacListarDTO>?> ObtenerListaVacacionesAsync()
    {
        SetBearer();
        var response = await _http.GetAsync("api/Vacaciones");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return JsonConvert.DeserializeObject<List<SolicitudVacListarDTO>>(raw);

        return null;
    }



    public async Task<bool> EditarTrabajadorAsync(int idTrabajador, TrabajadorEditarDTO dto)
    {
        SetBearer();

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PutAsync($"api/trabajadores/{idTrabajador}", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<Trabajador?> ObtenerTrabajadorAsync(int idTrabajador)
    {
        SetBearer();
        var response = await _http.GetAsync($"api/trabajadores/{idTrabajador}");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return JsonConvert.DeserializeObject<Trabajador>(raw);
        return null;
    }

    public async Task<bool> EliminarTrabajadorAsync(int idTrabajador)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/trabajadores/{idTrabajador}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<AsistenciaListarDTO>?> ObtenerListaAsistencias()
    {
        SetBearer();
        var response = await _http.GetAsync("api/asistencias");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<AsistenciaListarDTO>>(raw);
        }
           

        return null;
    }
    public async Task<List<ListarFaltasDTO>?> ObtenerListaFaltas()
    {
        SetBearer();
        var response = await _http.GetAsync("api/Faltas");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<ListarFaltasDTO>>(raw);
        }
           

        return null;
    }

    #region FORMACION ACADEMICA

    // OBTENER FORMACIONES POR TRABAJADOR (para las cards)
    public async Task<List<FormacionAcademicaResumenDTO>?>
        ObtenerFormacionesPorTrabajadorAsync(int idTrabajador)
    {
        SetBearer();
        var response = await _http.GetAsync($"api/FormacionesAcademicas/trabajador/{idTrabajador}");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<FormacionAcademicaResumenDTO>>(raw);
        }

        return null;
    }

    public async Task<List<CapacitacionResumenDTO>?> ObtenerCapacitacionesPorTrabajadorAsync(int idTrabajador)
    {
        SetBearer();
        var response = await _http.GetAsync($"api/Capacitaciones/trabajador/{idTrabajador}");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<CapacitacionResumenDTO>>(raw);
        }
        return null;
    }
    public async Task<ContratoDTO?> ObtenerContratoUltimoPorTrabajadorAsync(int idTrabajador)
    {
        SetBearer();
        var response = await _http.GetAsync($"api/Contratos/trabajador/ultimo/{idTrabajador}");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<ContratoDTO>(raw);
        }
        return null;
    }



    // OBTENER UNA FORMACION POR ID (para editar en el modal)
    public async Task<FormacionAcademicaEditarDTO?> ObtenerFormacionPorIdAsync(int idFormacion)
    {
        SetBearer();
        var response = await _http.GetAsync($"api/FormacionesAcademicas/{idFormacion}");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<FormacionAcademicaEditarDTO>(raw);
        }

        return null;
    }

    public async Task<CapacitacionEditarDTO?> ObtenerCapacitacionPorIdAsync(int idCapacitacion)
    {
        SetBearer();
        var response = await _http.GetAsync($"api/Capacitaciones/{idCapacitacion}");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<CapacitacionEditarDTO>(raw);
        }

        return null;
    }

    public async Task<ContratoActualizarDTO?> ObtenerContratoPorIdAsync(int idContrato)
    {
        SetBearer();
        var response = await _http.GetAsync($"api/Contratos/{idContrato}");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<ContratoActualizarDTO>(raw);
        }

        return null;
    }

    // CREAR FORMACION ACADEMICA (desde el modal, cuando IdFormacion = 0)
    public async Task<FormacionAcademicaResumenDTO?> CrearFormacionAcademicaAsync(FormacionAcademicaCrearDTO dto)
    {
        SetBearer();

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("api/FormacionesAcademicas", content);
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            // el backend devuelve el resumen (id, titulo, institucion, anio, etc.)
            return JsonConvert.DeserializeObject<FormacionAcademicaResumenDTO>(raw);
        }

        return null;
    }
    public async Task<CapacitacionResumenDTO?> CrearCapacitacionAsync(CapacitacionCrearDTO dto)
    {
        SetBearer();

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("api/Capacitaciones", content);
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<CapacitacionResumenDTO>(raw);
        }

        return null;
    }

    // ACTUALIZAR FORMACION ACADEMICA
    public async Task<bool> ActualizarCapacitacionAsync(int idCapacitacion, CapacitacionEditarDTO dto)
    {
        SetBearer();

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PutAsync($"api/Capacitaciones/{idCapacitacion}", content);
        return response.IsSuccessStatusCode;
    }
    public async Task<bool> ActualizarFormacionAcademicaAsync(int idFormacion, FormacionAcademicaEditarDTO dto)
    {
        SetBearer();

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PutAsync($"api/FormacionesAcademicas/{idFormacion}", content);
        return response.IsSuccessStatusCode;
    }
    public async Task<bool> ActualizarContratoAsync(int idContrato, ContratoActualizarDTO dto)
    {
        SetBearer();

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PutAsync($"api/Contratos/{idContrato}", content);
        return response.IsSuccessStatusCode;
    }

    // ELIMINAR FORMACION ACADEMICA
    public async Task<bool> EliminarCapacitacionAsync(int idCapacitacion)
    {
        SetBearer();
        try
        {
            var response = await _http.DeleteAsync($"api/Capacitaciones/{idCapacitacion}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    public async Task<bool> EliminarFormacionAcademicaAsync(int idFormacion)
    {
        SetBearer();
        try
        {
            var response = await _http.DeleteAsync($"api/FormacionesAcademicas/{idFormacion}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    public async Task<(bool ok, string? error)> AprobarSolicitudAsync(int idSolicitud)
    {
        SetBearer();
        var resp = await _http.PutAsync($"api/Vacaciones/{idSolicitud}/aprobar", null);

        if (resp.IsSuccessStatusCode)
            return (true, null);

        var contenido = await resp.Content.ReadAsStringAsync();
        return (false, contenido);
    }

    public async Task<ResumenVacacionesDTO?> ObtenerResumenVacacionesAsync(int idTrabajador)
    {
        SetBearer();

        var response = await _http.GetAsync($"api/Vacaciones/Resumen/{idTrabajador}");

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ResumenVacacionesDTO>(json);
    }

    public async Task<(bool ok, string? error)> RechazarSolicitudAsync(int idSolicitud)
    {
        SetBearer();
        var resp = await _http.PutAsync($"api/Vacaciones/{idSolicitud}/rechazar", null);

        if (resp.IsSuccessStatusCode)
            return (true, null);

        var contenido = await resp.Content.ReadAsStringAsync();
        return (false, contenido);
    }

    public async Task<(bool ok, string? error)> CrearSolicitudVacPermAsync(SolicitudVacCrearDTO dto)
    {
        SetBearer();

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("api/Vacaciones", content);

        if (response.IsSuccessStatusCode)
            return (true, null);

        // Intentamos capturar mensaje del backend
        var contenido = await response.Content.ReadAsStringAsync();
        return (false, contenido);
    }

    public async Task<bool> LogoutAsync(int idUsuario)
    {
        var dto = new LogoutSolicitudDTO { IdUsuario = idUsuario };

        var response = await _http.PostAsJsonAsync("api/auth/logout", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<LogsAccesoDTO>?> ObtenerListaLogsAcceso()
    {
        SetBearer();
        var response = await _http.GetAsync("api/LogsAcceso");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<LogsAccesoDTO>>(raw);
        }


        return null;
    }


    public async Task<List<TipoLicencia>?> ObtenerTiposLicenciaAsync()
    {
        SetBearer();
        var response = await _http.GetAsync("api/TipoLicencia"); // ajusta si tu endpoint se llama distinto
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<TipoLicencia>>(raw);
        }
        return null;
    }

    public async Task<(bool ok, string? error)> CrearLicenciaAsync(LicenciaCrearDTO dto)
    {
        SetBearer();

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("api/Licencias", content);

        if (response.IsSuccessStatusCode)
            return (true, null);

        var contenido = await response.Content.ReadAsStringAsync();
        return (false, contenido);
    }
    public async Task<List<LicenciaListarDTO>?> ObtenerLicenciasAsync()
    {
        SetBearer();
        var response = await _http.GetAsync("api/Licencias");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<LicenciaListarDTO>>(raw);
        }

        return null;
    }

    public async Task<(bool ok, string? error)> AprobarLicenciaAsync(int idLicencia)
    {
        SetBearer();
        var response = await _http.PutAsync($"api/Licencias/{idLicencia}/aprobar", null);

        if (response.IsSuccessStatusCode)
            return (true, null);

        var contenido = await response.Content.ReadAsStringAsync();
        return (false, contenido);
    }

    public async Task<(bool ok, string? error)> RechazarLicenciaAsync(int idLicencia)
    {
        SetBearer();
        var response = await _http.PutAsync($"api/Licencias/{idLicencia}/rechazar", null);

        if (response.IsSuccessStatusCode)
            return (true, null);

        var contenido = await response.Content.ReadAsStringAsync();
        return (false, contenido);
    }


    // APARTADO DE PLANILLAS
    // Crear planilla de sueldos y salarios (encabezado)
    public async Task<PlanillaResumenModel?> CrearPlanillaSueldosAsync(int gestion, int mes)
    {
        try
        {
            SetBearer();

            var dto = new
            {
                idTipoPlanilla = 31, // Clasificador: TipoPlanilla -> PlanillaSueldosSalarios
                gestion,
                mes,
                periodoDesde = new DateTime(gestion, mes, 1),
                periodoHasta = new DateTime(gestion, mes, DateTime.DaysInMonth(gestion, mes))
            };

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("api/PlanillaSueldosSalarios", content);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            return JsonConvert.DeserializeObject<PlanillaResumenModel>(raw);
        }
        catch
        {
            return null;
        }
    }

    // Generar Trabajador_Planilla para una planilla dada
    public async Task<bool> GenerarTrabajadoresPlanillaAsync(int idPlanilla)
    {
        try
        {
            SetBearer();
            var response = await _http.PostAsync($"api/PlanillaSueldosSalarios/{idPlanilla}/generar-trabajadores", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // Calcular planilla (llenar Trabajador_Planilla_Valor)
    public async Task<bool> CalcularPlanillaSueldosAsync(int idPlanilla)
    {
        try
        {
            SetBearer();
            var response = await _http.PostAsync($"api/PlanillaSueldosSalarios/{idPlanilla}/calcular", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // Obtener filas de la planilla (tipo Excel)
    public async Task<List<PlanillaSueldosFilaModel>?> ObtenerDatosPlanillaSueldosAsync(int idPlanilla)
    {
        try
        {
            SetBearer();
            var response = await _http.GetAsync($"api/PlanillaSueldosSalarios/{idPlanilla}");
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            return JsonConvert.DeserializeObject<List<PlanillaSueldosFilaModel>>(raw);
        }
        catch
        {
            return null;
        }
    }

    // Cerrar planilla (opcional)
    public async Task<bool> CerrarPlanillaSueldosAsync(int idPlanilla)
    {
        try
        {
            SetBearer();
            var response = await _http.PutAsync($"api/PlanillaSueldosSalarios/{idPlanilla}/cerrar", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool ok, byte[]? archivo, string? error)>
    DescargarJustificativoLicenciaAsync(int idLicencia)
    {
        try
        {
            var response = await _http.GetAsync($"api/licencias/{idLicencia}/archivo");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, null, error);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();

            return (true, bytes, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }
    public async Task<PlanillaResumenModel?> ObtenerResumenPlanillaSueldosAsync(int idPlanilla)
    {
        try
        {
            SetBearer();
            var response = await _http.GetAsync($"api/PlanillaSueldosSalarios/resumen/{idPlanilla}");

            if (!response.IsSuccessStatusCode)
                return null;

            var raw = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PlanillaResumenModel>(raw);
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<PlanillaAportesFilaModel>?> ObtenerDatosPlanillaAportesAsync(int idPlanilla)
    {
        try
        {
            SetBearer();
            var response = await _http.GetAsync($"api/PlanillaAportesPatronales/{idPlanilla}");
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            return JsonConvert.DeserializeObject<List<PlanillaAportesFilaModel>>(raw);
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<BoletaPagoModel>?> ObtenerBoletasPagoAsync(int idTrabajador)
    {
        try
        {
            SetBearer();
            var response = await _http.GetAsync($"api/BoletasPago/trabajador/{idTrabajador}");
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            return JsonConvert.DeserializeObject<List<BoletaPagoModel>>(raw);
        }
        catch
        {
            return null;
        }
    }

    public async Task<byte[]?> ObtenerBoletaPdfAsync(int idTrabajador, int idPlanilla)
    {
        try
        {
            SetBearer();
            var response = await _http.GetAsync(
                $"api/BoletasPago/trabajador/{idTrabajador}/planilla/{idPlanilla}/pdf");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<byte[]?> DescargarArchivoJustificativoFaltaAsync(int idFalta)
    {
        var response = await _http.GetAsync($"api/Faltas/{idFalta}/justificativo");
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<bool> SubirArchivoJustificativoFaltaAsync(int idFalta, string filePath)
    {
        using var form = new MultipartFormDataContent();
        var bytes = await File.ReadAllBytesAsync(filePath);
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

        form.Add(content, "Archivo", Path.GetFileName(filePath));

        var response = await _http.PostAsync($"api/Faltas/{idFalta}/justificativo", form);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> EliminarFaltaAsync(int idFalta)
    {
        var response = await _http.DeleteAsync($"api/Faltas/{idFalta}");
        return response.IsSuccessStatusCode;
    }


    // HISTORIALES
    public async Task<List<HistoricoPersonaListarDTO>?> ObtenerHistorialPersonasAsync()
    {
        SetBearer();
        var request = new HttpRequestMessage(HttpMethod.Get, "api/historicos/historicoPersonas");
        var response = await _http.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<HistoricoPersonaListarDTO>>(raw);
        }
        return null;
    }
    public async Task<List<HistoricoUsuarioListarDTO>?> ObtenerHistorialUsuariosAsync()
    {
        SetBearer();
        var request = new HttpRequestMessage(HttpMethod.Get, "api/historicos/historicoUsuarios");
        var response = await _http.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<HistoricoUsuarioListarDTO>>(raw);
        }
        return null;
    }
    public async Task<List<HistoricoTrabajadorListarDTO>?> ObtenerHistorialTrabajadoresAsync()
    {
        SetBearer();
        var request = new HttpRequestMessage(HttpMethod.Get, "api/historicos/historicoTrabajadores");
        var response = await _http.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<HistoricoTrabajadorListarDTO>>(raw);
        }
        return null;
    }
    public async Task<List<HistoricoFaltaListarDTO>?> ObtenerHistorialFaltasAsync()
    {
        SetBearer();
        var request = new HttpRequestMessage(HttpMethod.Get, "api/historicos/historicoFaltas");
        var response = await _http.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<HistoricoFaltaListarDTO>>(raw);
        }
        return null;
    }

    public async Task<bool> ActualizarRcIvaAsync(int idTrabajadorPlanilla, decimal montoRcIva)
    {
        try
        {
            var dto = new RcIvaActualizarDTO
            {
                MontoRcIva = montoRcIva
            };

            var response = await _http.PutAsJsonAsync(
                $"api/PlanillaSueldosSalarios/trabajador-planilla/{idTrabajadorPlanilla}/rc-iva",
                dto);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al actualizar RC-IVA: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    public async Task<bool> ActualizarOtrosDescAsync(int idTrabajadorPlanilla, decimal montoOtrosDesc)
    {
        var dto = new OtrosDescActualizarDTO
        {
            MontoOtrosDesc = montoOtrosDesc
        };

        var response = await _http.PutAsJsonAsync(
            $"api/PlanillaSueldosSalarios/trabajador-planilla/{idTrabajadorPlanilla}/otros-desc",
            dto);

        return response.IsSuccessStatusCode;
    }


    // VACACIONES
    public async Task<(bool ok, string? error)> EliminarSolicitudVacacionAsync(int idVacacion)
    {
        var response = await _http.DeleteAsync($"api/Vacaciones/{idVacacion}");

        if (response.IsSuccessStatusCode)
            return (true, null);

        var contenido = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(contenido) ? response.ReasonPhrase : contenido);
    }

    // LICENCIAS
    public async Task<(bool ok, string? error)> EliminarLicenciaAsync(int idLicencia)
    {
        var response = await _http.DeleteAsync($"api/Licencias/{idLicencia}");

        if (response.IsSuccessStatusCode)
            return (true, null);

        var contenido = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(contenido) ? response.ReasonPhrase : contenido);
    }
    public async Task<PlanillaResumenModel?> BuscarPlanillaSueldosPorPeriodoAsync(int gestion, int mes)
    {
        try
        {
            SetBearer();
            var response = await _http.GetAsync(
                $"api/PlanillaSueldosSalarios/buscar?gestion={gestion}&mes={mes}");

            if (!response.IsSuccessStatusCode)
                return null;

            var raw = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PlanillaResumenModel>(raw);
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool ok, string? error)> ActualizarSolicitudVacacionAsync(
    int idVacacion,
    SolicitudVacEditarDTO dto)
    {
        SetBearer();

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PutAsync($"api/Vacaciones/{idVacacion}", content);

        if (response.IsSuccessStatusCode)
            return (true, null);

        var contenido = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(contenido) ? response.ReasonPhrase : contenido);
    }


    public async Task<int?> CrearPersonaYObtenerIdAsync(PersonaCrearDTO dto)
    {
        try
        {
            SetBearer();
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("api/personas", content);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            // El backend devuelve un DTO con IdPersona; lo deserializamos a Persona
            var personaCreada = JsonConvert.DeserializeObject<Persona>(raw);
            return personaCreada?.IdPersona;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> RegistrarHuellaAsync(int idPersona, int indiceDedo, string templateXml)
    {
        try
        {
            SetBearer();

            var dto = new HuellaDTO
            {
                IdPersona = idPersona,
                IndiceDedo = indiceDedo,
                TemplateXml = templateXml
            };

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("api/huellas/registrar", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<HuellaRespuestaDTO>?> ObtenerHuellasPersonaAsync(int idPersona)
    {
        SetBearer();
        var resp = await _http.GetAsync($"api/huellas/obtener/{idPersona}");

        if (resp.StatusCode == HttpStatusCode.NotFound)
            return new List<HuellaRespuestaDTO>();

        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<HuellaRespuestaDTO>>(json);
    }


    public async Task<bool> CrearContratoAsync(ContratoCrearDTO dto)
    {
        SetBearer();

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("api/Contratos", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> GenerarInasistenciasAsync(DateTime desde, DateTime hasta)
    {
        SetBearer();

        // POST api/Faltas/generar-inasistencias?desde=2025-03-01&hasta=2025-03-31
        string url = $"api/Faltas/generar-inasistencias?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";

        var response = await _http.PostAsync(url, null);
        return response.IsSuccessStatusCode;
    }


}

