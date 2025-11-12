using ClientCoopSoft.DTO;
using ClientCoopSoft.DTO.Personas;
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
}
