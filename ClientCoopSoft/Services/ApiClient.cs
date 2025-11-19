using ClientCoopSoft.DTO;
using ClientCoopSoft.DTO.Asistencia;
using ClientCoopSoft.DTO.Capacitaciones;
using ClientCoopSoft.DTO.Contratacion;
using ClientCoopSoft.DTO.Extras;
using ClientCoopSoft.DTO.FormacionAcademica;
using ClientCoopSoft.DTO.Personas;
using ClientCoopSoft.DTO.Trabajadores;
using ClientCoopSoft.DTO.VacacionesPermisos;
using ClientCoopSoft.Models;
using Newtonsoft.Json;
using System.Net.Http;
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

    public async Task<List<SolicitudVacPermiso>?> ObtenerVacacionesPermisosAsync()
    {
        SetBearer();
        var response = await _http.GetAsync("api/VacacionesPermisos/SolicitudesCalendario");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return JsonConvert.DeserializeObject<List<SolicitudVacPermiso>>(raw);

        return null;
    }
    public async Task<List<SolicitudVacPermListarDTO>?> ObtenerListaVacacionesPermisosAsync()
    {
        SetBearer();
        var response = await _http.GetAsync("api/VacacionesPermisos");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
            return JsonConvert.DeserializeObject<List<SolicitudVacPermListarDTO>>(raw);

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
        var response = await _http.GetAsync($"api/Contratos/trabajador/ultimoContrato/{idTrabajador}");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<ContratoDTO> (raw);
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

    public async Task<bool> AprobarSolicitudAsync(int idSolicitud)
    {
        SetBearer();
        var response = await _http.PutAsync(
            $"api/VacacionesPermisos/{idSolicitud}/aprobar",
            null); // sin body

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RechazarSolicitudAsync(int idSolicitud)
    {
        SetBearer();
        var response = await _http.PutAsync(
            $"api/VacacionesPermisos/{idSolicitud}/rechazar",
            null); // sin body

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CrearSolicitudVacPermAsync(SolicitudVacPermCrearDTO dto)
    {
        SetBearer();
        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("api/VacacionesPermisos", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<TipoSolicitud>?> ObtenerClasificadorPorTipoSolicitudAsync()
    {
        SetBearer();
        var response = await _http.GetAsync("api/TipoSolicitud");
        var raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<List<TipoSolicitud>>(raw);
        }
        return null;
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
}

