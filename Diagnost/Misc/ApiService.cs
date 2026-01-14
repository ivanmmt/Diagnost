using Diagnost.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Diagnost.Misc
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api-diagnost.runasp.net";

        public ApiService()
        {
            if (OperatingSystem.IsBrowser())
            {
                var browserHandler = new BrowserCookieHandler();

                _httpClient = new HttpClient(browserHandler)
                {
                    BaseAddress = new Uri(BaseUrl)
                };
            }
            else
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = new CookieContainer();
                handler.UseCookies = true;
                handler.UseDefaultCredentials = false;

                _httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(BaseUrl)
                };
            }
        }

        // ==============
        // /api/Result
        // ==============
        // POST /api/Result
        // Returns result ID on success, null on failure
        public async Task<long?> CreateResult(string AccessCode, DiagnosticResult CurrentResult)
        {
            ResultRequest req = new ResultRequest
            {
                AccessCode = AccessCode,
                StudentFullName = $"{CurrentResult.StudentLastName} {CurrentResult.StudentFirstName} {CurrentResult.StudentPapaName}",
                SportType = CurrentResult.SportType,
                SportQualification = CurrentResult.SportQualification,
                Group = CurrentResult.Group,
                Gender = CurrentResult.Gender
            };

            try
            {
                HttpResponseMessage? response = await _httpClient.PostAsJsonAsync("api/Result", req, AppJsonContext.Default.ResultRequest);
                if (response.IsSuccessStatusCode)
                {
                    ResultResponse? result = await response.Content.ReadFromJsonAsync(AppJsonContext.Default.ResultResponse);
                    return result?.Id;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Misc > ApiService > CreateResult: {ex.Message}");
            }
            return null;
        }

        // GET /api/Result
        // Returns (results, null) on success, (null, errorMessage) on failure
        public async Task<(List<ResultResponse>?, string?)> GetResults()
        {
            try
            {
                // 1. Делаем запрос
                HttpResponseMessage? response = await _httpClient.GetAsync("api/Result");
        
                // 2. Читаем сырой текст (строку)
                string rawJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // 3. Пробуем превратить строку в объекты
                    // ВАЖНО: Используем AppJsonContext с новыми настройками
                    List<ResultResponse>? results = System.Text.Json.JsonSerializer.Deserialize(
                        rawJson, 
                        AppJsonContext.Default.ListResultResponse
                    );

                    // 4. ПРОВЕРКА НА НУЛИ (Метод "Таран")
                    if (results != null && results.Count > 0)
                    {
                        var first = results[0];
                        // Если значение 0, но в тексте JSON есть "342", значит парсинг сломан
                        if (first.PZMRLatet == 0 && rawJson.Contains("342")) 
                        {
                            // БРОСАЕМ ОШИБКУ, ЧТОБЫ ТЫ УВИДЕЛ ЕЕ НА ЭКРАНЕ
                            throw new Exception($"ПАРСИНГ НЕ РАБОТАЕТ! \nJSON: {rawJson}"); 
                        }
                    }

                    return (results, null);
                }
                else 
                {
                    // Если сервер вернул ошибку, показываем её
                    throw new Exception($"ОШИБКА СЕРВЕРА: {response.StatusCode} \nТекст: {rawJson}");
                }
            }
            catch (Exception ex)
            {
                // Вернем ошибку, чтобы она показалась в UI или остановила отладчик
                return (null, ex.Message);
            }
        }

        // PUT /api/Result/PZMR
        // Returns true on success, false on failure
        public async Task<bool> PutPZMR(PZMRResultRequest req)
        {
            try
            {
                HttpResponseMessage? response = await _httpClient.PutAsJsonAsync("api/Result/PZMR", req, AppJsonContext.Default.PZMRResultRequest);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Misc > ApiService > PutPZMR: {ex.Message}");
            }
            return false;
        }

        // PUT /api/Result/PV2_3
        // Returns true on success, false on failure
        public async Task<bool> PutPV2_3(PV2_3ResultRequest req)
        {
            try
            {
                HttpResponseMessage? response = await _httpClient.PutAsJsonAsync("api/Result/PV2_3", req, AppJsonContext.Default.PV2_3ResultRequest);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Misc > ApiService > PutPV2_3: {ex.Message}");
            }
            return false;
        }

        // PUT /api/Result/UFP
        // Returns true on success, false on failure
        public async Task<bool> PutUFP(UFPResultRequest req)
        {
            try
            {
                HttpResponseMessage? response = await _httpClient.PutAsJsonAsync("api/Result/UFP", req, AppJsonContext.Default.UFPResultRequest);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Misc > ApiService > PutUFP: {ex.Message}");
            }
            return false;
        }
        
        // DELETE /api/Result/{id}
        public async Task<bool> DeleteResult(long id)
        {
            try
            {
                // Обрати внимание: тут URL "api/Result", а не "api/admin/users"
                HttpResponseMessage response = await _httpClient.DeleteAsync($"api/Result/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] DeleteResult: {ex.Message}");
                return false;
            }
        }

        // Ensures that a result exists for the current session, creating one if necessary
        // To-Do: add error handling
        public async Task VerifyResultExists(DiagnosticResult currentResult)
        {
            if (SessionContext.ResultId == null && SessionContext.AccessCode != null)
            {
                long? id = await SessionContext.ApiService.CreateResult(SessionContext.AccessCode, currentResult);
                // if (id == null) todo: add error handling
                SessionContext.ResultId = id;
            }
        }

        // ===============
        // /api/AccessCode
        // ===============
        // POST api/AccessCode
        // Returns access code string on success, null on failure
        public async Task<AccessCodeResponse?> CreateAccessCode()
        {
            try
            {
                HttpResponseMessage? response = await _httpClient.PostAsync("api/AccessCode", null);
                if (response.IsSuccessStatusCode)
                {
                    AccessCodeResponse? accessCode = await response.Content.ReadFromJsonAsync(AppJsonContext.Default.AccessCodeResponse);
                    return accessCode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Misc > ApiService > CreateAccessCode: {ex.Message}");
            }
            return null;
        }

        // GET /api/AccessCode
        // Returns list of access codes on success, null on failure
        public async Task<List<AccessCodeResponse>?> GetAllAccessCodes()
        {
            try
            {
                HttpResponseMessage? response = await _httpClient.GetAsync("api/AccessCode");
                if (response.IsSuccessStatusCode)
                {
                    List<AccessCodeResponse>? accessCodes = await response.Content.ReadFromJsonAsync(AppJsonContext.Default.ListAccessCodeResponse);
                    return accessCodes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Misc > ApiService > GetAllAccessCodes: {ex.Message}");
            }
            return null;
        }

        // GET /api/AccessCode/{code}
        // Returns access code info on success, null on failure
        public async Task<AccessCodeResponse?> GetAccessCode(string code)
        {
            try
            {
                HttpResponseMessage? response = await _httpClient.GetAsync($"api/AccessCode/{code}");
                if (response.IsSuccessStatusCode)
                {
                    AccessCodeResponse? accessCodes = await response.Content.ReadFromJsonAsync(AppJsonContext.Default.AccessCodeResponse);
                    return accessCodes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Misc > ApiService > GetAllAccessCodes: {ex.Message}");
            }
            return null;
        }

        // PUT /api/AccessCode/{code}
        // Returns true on success, false on failure
        public async Task<bool> RevokeAccessCode(string code)
        {
            try
            {
                HttpResponseMessage? response = await _httpClient.PutAsync($"api/AccessCode/{code}", null);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Misc > ApiService > RevokeAccessCode: {ex.Message}");
            }
            return false;
        }

        // DELETE /api/AccessCode/{code}
        // Returns true on success, false on failure
        public async Task<bool> DeleteAccessCode(string code)
        {
            try
            {
                HttpResponseMessage? response = await _httpClient.DeleteAsync($"api/AccessCode/{code}");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Misc > ApiService > DeleteAccessCode: {ex.Message}");
            }
            return false;
        }

        // GET /api/AccessCode/verify/{code}
        // Returns true if access code is valid, false otherwise
        public async Task<bool> VerifyAccessCode(string? accessCode)
        {
            try
            {
                HttpResponseMessage? response = await _httpClient.GetAsync($"api/AccessCode/verify/{accessCode}");
                if (response.IsSuccessStatusCode)
                {
                    if (await response.Content.ReadAsStringAsync() == "true")
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Misc > ApiService > VerifyAccessCode: {ex.Message}");
            }
            return false;
        }

        // ===============
        // /api/auth
        // ===============
        // POST /api/auth/login
        // If method returns null - success, else returns error message
        public async Task<string?> Login(string email, string password)
        {
            try
            {
                LoginRequest request = new LoginRequest { Email = email, Password = password };
                HttpResponseMessage? response = await _httpClient.PostAsJsonAsync("api/auth/login", request, AppJsonContext.Default.LoginRequest);

                if (response.IsSuccessStatusCode)
                {
                    return null; // Успех
                }
            }
            catch (Exception ex)
            {
                return $"[Error] Login: {ex.Message}";
            }
            return "Невірний логін або пароль";
        }

        // POST /api/auth/logout
        public async Task Logout()
        {
            try
            {
                await _httpClient.PostAsync("api/auth/logout", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Misc > ApiService > Logout: {ex.Message}");
            }
        }

        // ===========
        // /api/admin
        // ===========
        // POST /api/admin/register
        // Returns RegisterDto on success, null on failure
        public async Task<RegisterResponse?> RegisterAdmin(string email, string password)
        {
            try
            {
                RegisterRequest request = new RegisterRequest { Email = email, Password = password };
                HttpResponseMessage? response = await _httpClient.PostAsJsonAsync("api/admin/register", request, AppJsonContext.Default.RegisterRequest);
                if (response.IsSuccessStatusCode)
                {
                    RegisterResponse? registerResponse = await response.Content.ReadFromJsonAsync(AppJsonContext.Default.RegisterResponse);
                    return registerResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Misc > ApiService > RegisterAdmin: {ex.Message}");
            }
            return null;
        }

        // GET /api/admin/users
        // Returns list of RegisterDto on success, null on failure
        public async Task<List<RegisterResponse>?> GetAllAdmins()
        {
            try
            {
                HttpResponseMessage? response = await _httpClient.GetAsync("api/admin/users");
                if (response.IsSuccessStatusCode)
                {
                    List<RegisterResponse>? admins = await response.Content.ReadFromJsonAsync(AppJsonContext.Default.ListRegisterResponse);
                    return admins;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Misc > ApiService > GetAllAdmins: {ex.Message}");
            }
            return null;
        }

        // DELETE /api/admin/users/{id}
        // Returns true on success, false on failure
        public async Task<bool> DeleteAdmin(Guid id)
        {
            try
            {
                HttpResponseMessage? response = await _httpClient.DeleteAsync($"api/admin/users/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Misc > ApiService > DeleteAdmin: {ex.Message}");
            }
            return false;
        }
    }
}
