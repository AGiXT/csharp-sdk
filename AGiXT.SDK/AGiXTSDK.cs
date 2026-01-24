using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AGiXTSDK
{
    public class AGiXTSDK
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUri;

        public AGiXTSDK(string baseUri = null, string apiKey = null)
        {
            _baseUri = baseUri ?? "http://localhost:7437";
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", apiKey.Replace("Bearer ", "").Replace("bearer ", ""));
            }

            if (_baseUri.EndsWith("/"))
            {
                _baseUri = _baseUri.Substring(0, _baseUri.Length - 1);
            }
        }

        private string HandleError(Exception e)
        {
            Console.WriteLine($"Error: {e}");
            return "Unable to retrieve data.";
        }

        // ─────────────────────────────────────────────────────────────
        // Auth Methods
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Login with username/password authentication.
        /// </summary>
        /// <param name="username">Username or email address</param>
        /// <param name="password">User's password</param>
        /// <param name="mfaToken">Optional TOTP code if MFA is enabled</param>
        /// <returns>Login response with token on success</returns>
        public async Task<Dictionary<string, object>> LoginAsync(string username, string password, string mfaToken = null)
        {
            try
            {
                var requestBody = new Dictionary<string, object>
                {
                    { "username", username },
                    { "password", password }
                };
                if (!string.IsNullOrEmpty(mfaToken))
                {
                    requestBody["mfa_token"] = mfaToken;
                }
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/login", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
                if (response.IsSuccessStatusCode && result.ContainsKey("token"))
                {
                    _httpClient.DefaultRequestHeaders.Remove("Authorization");
                    _httpClient.DefaultRequestHeaders.Add("Authorization", result["token"].ToString());
                }
                return result;
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        /// <summary>
        /// Legacy login with magic link (email + OTP token).
        /// Maintained for backward compatibility.
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="otp">TOTP code from authenticator app</param>
        /// <returns>Token string on success</returns>
        public async Task<string> LoginMagicLinkAsync(string email, string otp)
        {
            try
            {
                var requestBody = new { email = email, token = otp };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/login/magic-link", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        /// <summary>
        /// Register a new user with username/password authentication.
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's password</param>
        /// <param name="confirmPassword">Password confirmation</param>
        /// <param name="firstName">User's first name (optional)</param>
        /// <param name="lastName">User's last name (optional)</param>
        /// <param name="username">Desired username (optional)</param>
        /// <param name="organizationName">Company/organization name (optional)</param>
        /// <returns>Response with user_id, username, token on success</returns>
        public async Task<Dictionary<string, object>> RegisterUserAsync(
            string email,
            string password,
            string confirmPassword,
            string firstName = "",
            string lastName = "",
            string username = null,
            string organizationName = null)
        {
            try
            {
                var requestBody = new Dictionary<string, object>
                {
                    { "email", email },
                    { "password", password },
                    { "confirm_password", confirmPassword },
                    { "first_name", firstName },
                    { "last_name", lastName }
                };
                if (!string.IsNullOrEmpty(username))
                {
                    requestBody["username"] = username;
                }
                if (!string.IsNullOrEmpty(organizationName))
                {
                    requestBody["organization_name"] = organizationName;
                }

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/user", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
                if (response.IsSuccessStatusCode && result.ContainsKey("token"))
                {
                    _httpClient.DefaultRequestHeaders.Remove("Authorization");
                    _httpClient.DefaultRequestHeaders.Add("Authorization", result["token"].ToString());
                }
                return result;
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        /// <summary>
        /// Get MFA setup information including QR code URI.
        /// </summary>
        /// <returns>MFA setup info with provisioning_uri, secret, and mfa_enabled status</returns>
        public async Task<Dictionary<string, object>> GetMfaSetupAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/user/mfa/setup");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        /// <summary>
        /// Enable MFA for the current user.
        /// </summary>
        /// <param name="mfaToken">TOTP code from authenticator app to verify setup</param>
        /// <returns>Response with success message</returns>
        public async Task<Dictionary<string, object>> EnableMfaAsync(string mfaToken)
        {
            try
            {
                var requestBody = new { mfa_token = mfaToken };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/user/mfa/enable", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        /// <summary>
        /// Disable MFA for the current user.
        /// </summary>
        /// <param name="password">User's password (optional)</param>
        /// <param name="mfaToken">Current TOTP code (optional)</param>
        /// <returns>Response with success message</returns>
        public async Task<Dictionary<string, object>> DisableMfaAsync(string password = null, string mfaToken = null)
        {
            try
            {
                var requestBody = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(password))
                {
                    requestBody["password"] = password;
                }
                if (!string.IsNullOrEmpty(mfaToken))
                {
                    requestBody["mfa_token"] = mfaToken;
                }
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/user/mfa/disable", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        /// <summary>
        /// Change the current user's password.
        /// </summary>
        /// <param name="currentPassword">Current password</param>
        /// <param name="newPassword">New password</param>
        /// <param name="confirmPassword">New password confirmation</param>
        /// <returns>Response with success message</returns>
        public async Task<Dictionary<string, object>> ChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword)
        {
            try
            {
                var requestBody = new
                {
                    current_password = currentPassword,
                    new_password = newPassword,
                    confirm_password = confirmPassword
                };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/user/password/change", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        /// <summary>
        /// Set a password for users who don't have one (e.g., social login users).
        /// </summary>
        /// <param name="newPassword">New password</param>
        /// <param name="confirmPassword">New password confirmation</param>
        /// <returns>Response with success message</returns>
        public async Task<Dictionary<string, object>> SetPasswordAsync(string newPassword, string confirmPassword)
        {
            try
            {
                var requestBody = new
                {
                    new_password = newPassword,
                    confirm_password = confirmPassword
                };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/user/password/set", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/user/exists?email={Uri.EscapeDataString(email)}");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<bool>(json);
            }
            catch (Exception e)
            {
                HandleError(e);
                return false;
            }
        }

        public async Task<Dictionary<string, object>> UpdateUserAsync(Dictionary<string, object> updates)
        {
            try
            {
                var json = JsonSerializer.Serialize(updates);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/v1/user", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<Dictionary<string, object>> GetUserAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/user");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Provider Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<List<object>> GetProvidersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/providers");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<object>>(json);
            }
            catch (Exception e)
            {
                return new List<object> { HandleError(e) };
            }
        }

        public async Task<Dictionary<string, object>> GetEmbeddersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/embedders");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["embedders"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Agent Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<Dictionary<string, object>> AddAgentAsync(string agentName, Dictionary<string, object> settings = null, Dictionary<string, bool> commands = null, List<string> trainingUrls = null)
        {
            try
            {
                var requestBody = new
                {
                    agent_name = agentName,
                    settings = settings ?? new Dictionary<string, object>(),
                    commands = commands ?? new Dictionary<string, bool>(),
                    training_urls = trainingUrls ?? new List<string>()
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<Dictionary<string, object>> ImportAgentAsync(string agentName, Dictionary<string, object> settings = null, Dictionary<string, bool> commands = null)
        {
            try
            {
                var requestBody = new
                {
                    agent_name = agentName,
                    settings = settings ?? new Dictionary<string, object>(),
                    commands = commands ?? new Dictionary<string, bool>()
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/import", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<string> RenameAgentAsync(string agentId, string newName)
        {
            try
            {
                var requestBody = new { new_name = newName };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"{_baseUri}/v1/agent/{agentId}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> UpdateAgentSettingsAsync(string agentId, Dictionary<string, object> settings, string agentName = "")
        {
            try
            {
                var requestBody = new
                {
                    agent_name = agentName,
                    settings = settings,
                    commands = new Dictionary<string, bool>(),
                    training_urls = new List<string>()
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/v1/agent/{agentId}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> UpdateAgentCommandsAsync(string agentId, Dictionary<string, bool> commands)
        {
            try
            {
                var requestBody = new { commands = commands };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/v1/agent/{agentId}/commands", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> DeleteAgentAsync(string agentId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUri}/v1/agent/{agentId}");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<List<Dictionary<string, object>>> GetAgentsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/agent");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(json);
                return result["agents"];
            }
            catch (Exception e)
            {
                return new List<Dictionary<string, object>> { new Dictionary<string, object> { { "error", HandleError(e) } } };
            }
        }

        public async Task<Dictionary<string, object>> GetAgentConfigAsync(string agentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/agent/{agentId}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["agent"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<string> GetAgentIdByNameAsync(string agentName)
        {
            try
            {
                var agents = await GetAgentsAsync();
                foreach (var agent in agents)
                {
                    if (agent.ContainsKey("name") && agent["name"]?.ToString() == agentName)
                    {
                        return agent["id"]?.ToString();
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Conversation Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<List<object>> GetConversationsAsync(string agentId = "")
        {
            var url = string.IsNullOrEmpty(agentId)
                ? $"{_baseUri}/v1/conversations"
                : $"{_baseUri}/v1/conversations?agent_id={agentId}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<object>>(json);
            }
            catch (Exception e)
            {
                return new List<object> { HandleError(e) };
            }
        }

        public async Task<List<Dictionary<string, object>>> GetConversationAsync(string conversationId, int limit = 100, int page = 1)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/conversation/{conversationId}?limit={limit}&page={page}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(json);
                return result["conversation_history"];
            }
            catch (Exception e)
            {
                return new List<Dictionary<string, object>> { new Dictionary<string, object> { { "error", HandleError(e) } } };
            }
        }

        public async Task<Dictionary<string, object>> ForkConversationAsync(string conversationId, string messageId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/conversation/fork/{conversationId}/{messageId}", null);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<Dictionary<string, object>> NewConversationAsync(string agentId, string conversationName, List<Dictionary<string, object>> conversationContent = null)
        {
            try
            {
                var requestBody = new
                {
                    conversation_name = conversationName,
                    agent_id = agentId,
                    conversation_content = conversationContent ?? new List<Dictionary<string, object>>()
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/conversation", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<Dictionary<string, object>> RenameConversationAsync(string conversationId, string newName = "-")
        {
            try
            {
                var requestBody = new { new_conversation_name = newName };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/v1/conversation/{conversationId}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<string> DeleteConversationAsync(string conversationId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUri}/v1/conversation/{conversationId}");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> DeleteConversationMessageAsync(string conversationId, string messageId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUri}/v1/conversation/{conversationId}/message/{messageId}");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> UpdateConversationMessageAsync(string conversationId, string messageId, string newMessage)
        {
            try
            {
                var requestBody = new { new_message = newMessage };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/v1/conversation/{conversationId}/message/{messageId}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> NewConversationMessageAsync(string role, string message, string conversationId)
        {
            try
            {
                var requestBody = new { role = role, message = message };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/conversation/{conversationId}/message", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> GetConversationIdByNameAsync(string conversationName)
        {
            try
            {
                var conversations = await GetConversationsAsync();
                foreach (var conv in conversations)
                {
                    if (conv is JsonElement elem && elem.TryGetProperty("name", out var nameProp) && nameProp.GetString() == conversationName)
                    {
                        if (elem.TryGetProperty("id", out var idProp))
                        {
                            return idProp.GetString();
                        }
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Agent Prompt Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<string> PromptAgentAsync(string agentId, string promptName, Dictionary<string, object> promptArgs)
        {
            try
            {
                var requestBody = new
                {
                    prompt_name = promptName,
                    prompt_args = promptArgs
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/{agentId}/prompt", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("response") ? result["response"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> InstructAsync(string agentId, string userInput, string conversationId)
        {
            return await PromptAgentAsync(agentId, "instruct", new Dictionary<string, object>
            {
                { "user_input", userInput },
                { "disable_memory", true },
                { "conversation_name", conversationId }
            });
        }

        public async Task<string> ChatAsync(string agentId, string userInput, string conversationId, int contextResults = 4)
        {
            return await PromptAgentAsync(agentId, "Chat", new Dictionary<string, object>
            {
                { "user_input", userInput },
                { "context_results", contextResults },
                { "conversation_name", conversationId },
                { "disable_memory", true }
            });
        }

        public async Task<string> SmartInstructAsync(string agentId, string userInput, string conversationId)
        {
            return await RunChainAsync(
                chainName: "Smart Instruct",
                userInput: userInput,
                agentId: agentId,
                allResponses: false,
                fromStep: 1,
                chainArgs: new Dictionary<string, object>
                {
                    { "conversation_name", conversationId },
                    { "disable_memory", true }
                }
            );
        }

        public async Task<string> SmartChatAsync(string agentId, string userInput, string conversationId)
        {
            return await RunChainAsync(
                chainName: "Smart Chat",
                userInput: userInput,
                agentId: agentId,
                allResponses: false,
                fromStep: 1,
                chainArgs: new Dictionary<string, object>
                {
                    { "conversation_name", conversationId },
                    { "disable_memory", true }
                }
            );
        }

        // ─────────────────────────────────────────────────────────────
        // Command Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<Dictionary<string, object>> GetCommandsAsync(string agentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/agent/{agentId}/command");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["commands"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<string> ToggleCommandAsync(string agentId, string commandName, bool enable)
        {
            try
            {
                var requestBody = new { command_name = commandName, enable = enable };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"{_baseUri}/v1/agent/{agentId}/command", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> ExecuteCommandAsync(string agentId, string commandName, Dictionary<string, object> commandArgs, string conversationId = "")
        {
            try
            {
                var requestBody = new
                {
                    command_name = commandName,
                    command_args = commandArgs,
                    conversation_name = conversationId
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/{agentId}/command", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("response") ? result["response"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Chain Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<List<object>> GetChainsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/chains");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<object>>(json);
            }
            catch (Exception e)
            {
                return new List<object> { HandleError(e) };
            }
        }

        public async Task<Dictionary<string, object>> GetChainAsync(string chainId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/chain/{chainId}");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<Dictionary<string, object>> GetChainResponsesAsync(string chainId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/chain/{chainId}/responses");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["chain"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<List<string>> GetChainArgsAsync(string chainId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/chain/{chainId}/args");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<string>>(json);
            }
            catch (Exception e)
            {
                return new List<string> { HandleError(e) };
            }
        }

        public async Task<string> RunChainAsync(string chainId = "", string chainName = "", string userInput = "", string agentId = "", bool allResponses = false, int fromStep = 1, Dictionary<string, object> chainArgs = null)
        {
            try
            {
                var requestBody = new
                {
                    prompt = userInput,
                    agent_override = agentId,
                    all_responses = allResponses,
                    from_step = fromStep,
                    chain_args = chainArgs ?? new Dictionary<string, object>()
                };

                var endpoint = !string.IsNullOrEmpty(chainId) ? chainId : chainName;
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/chain/{endpoint}/run", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> RunChainStepAsync(string chainId, int stepNumber, string userInput, string agentId = "", Dictionary<string, object> chainArgs = null)
        {
            try
            {
                var requestBody = new
                {
                    prompt = userInput,
                    agent_override = agentId,
                    chain_args = chainArgs ?? new Dictionary<string, object>()
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/chain/{chainId}/run/step/{stepNumber}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<Dictionary<string, object>> AddChainAsync(string chainName)
        {
            try
            {
                var requestBody = new { chain_name = chainName };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/chain", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<string> ImportChainAsync(string chainName, object steps)
        {
            try
            {
                var requestBody = new { chain_name = chainName, steps = steps };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/chain/import", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> RenameChainAsync(string chainId, string newName)
        {
            try
            {
                var requestBody = new { new_name = newName };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/v1/chain/{chainId}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> DeleteChainAsync(string chainId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUri}/v1/chain/{chainId}");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> AddStepAsync(string chainId, int stepNumber, string agentId, string promptType, object prompt)
        {
            try
            {
                var requestBody = new
                {
                    step_number = stepNumber,
                    agent_id = agentId,
                    prompt_type = promptType,
                    prompt = prompt
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/chain/{chainId}/step", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> UpdateStepAsync(string chainId, int stepNumber, string agentId, string promptType, object prompt)
        {
            try
            {
                var requestBody = new
                {
                    step_number = stepNumber,
                    agent_id = agentId,
                    prompt_type = promptType,
                    prompt = prompt
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/v1/chain/{chainId}/step/{stepNumber}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> MoveStepAsync(string chainId, int oldStepNumber, int newStepNumber)
        {
            try
            {
                var requestBody = new { old_step_number = oldStepNumber, new_step_number = newStepNumber };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"{_baseUri}/v1/chain/{chainId}/step/move", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> DeleteStepAsync(string chainId, int stepNumber)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUri}/v1/chain/{chainId}/step/{stepNumber}");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> GetChainIdByNameAsync(string chainName)
        {
            try
            {
                var chains = await GetChainsAsync();
                foreach (var chain in chains)
                {
                    if (chain is JsonElement elem && elem.TryGetProperty("name", out var nameProp) && nameProp.GetString() == chainName)
                    {
                        if (elem.TryGetProperty("id", out var idProp))
                        {
                            return idProp.GetString();
                        }
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Prompt Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<Dictionary<string, object>> AddPromptAsync(string promptName, string prompt, string promptCategory = "Default")
        {
            try
            {
                var requestBody = new
                {
                    prompt_name = promptName,
                    prompt = prompt,
                    prompt_category = promptCategory
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/prompt", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<Dictionary<string, object>> GetPromptAsync(string promptId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/prompt/{promptId}");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<List<object>> GetPromptsAsync(string promptCategory = "Default")
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/prompts?prompt_category={Uri.EscapeDataString(promptCategory)}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<object>>>(json);
                return result["prompts"];
            }
            catch (Exception e)
            {
                return new List<object> { HandleError(e) };
            }
        }

        public async Task<Dictionary<string, object>> GetAllPromptsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/prompt/all");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<List<object>> GetPromptCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/prompt/categories");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<object>>>(json);
                return result["categories"];
            }
            catch (Exception e)
            {
                return new List<object> { HandleError(e) };
            }
        }

        public async Task<List<object>> GetPromptsByCategoryIdAsync(string categoryId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/prompt/category/{categoryId}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<object>>>(json);
                return result["prompts"];
            }
            catch (Exception e)
            {
                return new List<object> { HandleError(e) };
            }
        }

        public async Task<Dictionary<string, object>> GetPromptArgsAsync(string promptId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/prompt/{promptId}/args");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["prompt_args"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<string> DeletePromptAsync(string promptId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUri}/v1/prompt/{promptId}");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> UpdatePromptAsync(string promptId, string prompt)
        {
            try
            {
                var requestBody = new { prompt = prompt };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/v1/prompt/{promptId}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> RenamePromptAsync(string promptId, string newName)
        {
            try
            {
                var requestBody = new { prompt_name = newName };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"{_baseUri}/v1/prompt/{promptId}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Extension Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<Dictionary<string, object>> GetExtensionSettingsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/extensions/settings");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["extension_settings"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<List<object>> GetExtensionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/extensions");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<object>>(json);
            }
            catch (Exception e)
            {
                return new List<object> { HandleError(e) };
            }
        }

        public async Task<List<object>> GetAgentExtensionsAsync(string agentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/agent/{agentId}/extensions");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<object>>>(json);
                return result["extensions"];
            }
            catch (Exception e)
            {
                return new List<object> { HandleError(e) };
            }
        }

        public async Task<Dictionary<string, object>> GetCommandArgsAsync(string commandName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/extensions/{commandName}/args");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["command_args"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Memory Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<string> LearnTextAsync(string agentId, string userInput, string text, string collectionNumber = "0")
        {
            try
            {
                var requestBody = new
                {
                    user_input = userInput,
                    text = text,
                    collection_number = collectionNumber
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/{agentId}/learn/text", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> LearnUrlAsync(string agentId, string url, string collectionNumber = "0")
        {
            try
            {
                var requestBody = new
                {
                    url = url,
                    collection_number = collectionNumber
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/{agentId}/learn/url", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> LearnFileAsync(string agentId, string fileName, string fileContent, string collectionNumber = "0")
        {
            try
            {
                var requestBody = new
                {
                    file_name = fileName,
                    file_content = fileContent,
                    collection_number = collectionNumber
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/{agentId}/learn/file", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> LearnGitHubRepoAsync(string agentId, string githubRepo, string githubUser = null, string githubToken = null, string githubBranch = "main", bool useAgentSettings = false, string collectionNumber = "0")
        {
            try
            {
                var requestBody = new
                {
                    github_repo = githubRepo,
                    github_user = githubUser,
                    github_token = githubToken,
                    github_branch = githubBranch,
                    collection_number = collectionNumber,
                    use_agent_settings = useAgentSettings
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/{agentId}/learn/github", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> LearnArxivAsync(string agentId, string query = "", string arxivIds = "", int maxResults = 5, string collectionNumber = "0")
        {
            try
            {
                var requestBody = new
                {
                    query = query,
                    arxiv_ids = arxivIds,
                    max_results = maxResults,
                    collection_number = collectionNumber
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/{agentId}/learn/arxiv", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> AgentReaderAsync(string agentId, string readerName, Dictionary<string, object> data, string collectionNumber = "0")
        {
            if (!data.ContainsKey("collection_number"))
            {
                data["collection_number"] = collectionNumber;
            }

            try
            {
                var requestBody = new { data = data };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/{agentId}/reader/{readerName}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> WipeAgentMemoriesAsync(string agentId, string collectionNumber = "0")
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUri}/v1/agent/{agentId}/memory/{collectionNumber}");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> DeleteAgentMemoryAsync(string agentId, string memoryId, string collectionNumber = "0")
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUri}/v1/agent/{agentId}/memory/{collectionNumber}/{memoryId}");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<List<Dictionary<string, object>>> GetAgentMemoriesAsync(string agentId, string userInput, int limit = 5, float minRelevanceScore = 0.0f, string collectionNumber = "0")
        {
            try
            {
                var requestBody = new
                {
                    user_input = userInput,
                    limit = limit,
                    min_relevance_score = minRelevanceScore
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/{agentId}/memory/{collectionNumber}/query", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(responseJson);
                return result["memories"];
            }
            catch (Exception e)
            {
                return new List<Dictionary<string, object>> { new Dictionary<string, object> { { "error", HandleError(e) } } };
            }
        }

        public async Task<List<Dictionary<string, object>>> ExportAgentMemoriesAsync(string agentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/agent/{agentId}/memory/export");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(json);
                return result["memories"];
            }
            catch (Exception e)
            {
                return new List<Dictionary<string, object>> { new Dictionary<string, object> { { "error", HandleError(e) } } };
            }
        }

        public async Task<string> ImportAgentMemoriesAsync(string agentId, List<Dictionary<string, object>> memories)
        {
            try
            {
                var requestBody = new { memories = memories };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/{agentId}/memory/import", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> CreateDatasetAsync(string agentId, string datasetName, int batchSize = 4)
        {
            try
            {
                var requestBody = new
                {
                    dataset_name = datasetName,
                    batch_size = batchSize
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/{agentId}/memory/dataset", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<List<string>> GetBrowsedLinksAsync(string agentId, string collectionNumber = "0")
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/agent/{agentId}/browsed_links/{collectionNumber}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                return result["links"];
            }
            catch (Exception e)
            {
                return new List<string> { HandleError(e) };
            }
        }

        public async Task<string> DeleteBrowsedLinkAsync(string agentId, string link, string collectionNumber = "0")
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUri}/v1/agent/{agentId}/browsed_links");
                var requestBody = new { link = link, collection_number = collectionNumber };
                request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.SendAsync(request);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<Dictionary<string, object>> GetMemoriesExternalSourcesAsync(string agentId, string collectionNumber)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/agent/{agentId}/memory/external_sources/{collectionNumber}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["external_sources"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<string> DeleteMemoryExternalSourceAsync(string agentId, string source, string collectionNumber)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUri}/v1/agent/{agentId}/memory/external_source");
                var requestBody = new { external_source = source, collection_number = collectionNumber };
                request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.SendAsync(request);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Persona Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<string> GetPersonaAsync(string agentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/agent/{agentId}/persona");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return result.ContainsKey("message") ? result["message"] : json;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> UpdatePersonaAsync(string agentId, string persona)
        {
            try
            {
                var requestBody = new { persona = persona };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/v1/agent/{agentId}/persona", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Feedback Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<string> PositiveFeedbackAsync(string agentId, string message, string userInput, string feedback, string conversationId = "")
        {
            return await ProvideFeedbackAsync(agentId, message, userInput, feedback, true, conversationId);
        }

        public async Task<string> NegativeFeedbackAsync(string agentId, string message, string userInput, string feedback, string conversationId = "")
        {
            return await ProvideFeedbackAsync(agentId, message, userInput, feedback, false, conversationId);
        }

        private async Task<string> ProvideFeedbackAsync(string agentId, string message, string userInput, string feedback, bool positive, string conversationId)
        {
            try
            {
                var requestBody = new
                {
                    user_input = userInput,
                    message = message,
                    feedback = feedback,
                    positive = positive,
                    conversation_name = conversationId
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/{agentId}/feedback", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("message") ? result["message"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Text-to-Speech Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<string> TextToSpeechAsync(string agentId, string text)
        {
            try
            {
                var requestBody = new { text = text };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/{agentId}/text_to_speech", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("url") ? result["url"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Task Planning Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<string> PlanTaskAsync(string agentId, string userInput, bool websearch = false, int websearchDepth = 3, string conversationId = "", bool logUserInput = true, bool logOutput = true, bool enableNewCommand = true)
        {
            try
            {
                var requestBody = new
                {
                    user_input = userInput,
                    websearch = websearch,
                    websearch_depth = websearchDepth,
                    conversation_name = conversationId,
                    log_user_input = logUserInput,
                    log_output = logOutput,
                    enable_new_command = enableNewCommand
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/{agentId}/plan/task", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result.ContainsKey("response") ? result["response"] : responseJson;
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Company Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<List<object>> GetCompaniesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/companies");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<object>>(json);
            }
            catch (Exception e)
            {
                return new List<object> { HandleError(e) };
            }
        }

        public async Task<Dictionary<string, object>> CreateCompanyAsync(string name, string agentName, string parentCompanyId = null)
        {
            try
            {
                var requestBody = new
                {
                    name = name,
                    agent_name = agentName,
                    parent_company_id = parentCompanyId
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/companies", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<Dictionary<string, object>> UpdateCompanyAsync(string companyId, string name)
        {
            try
            {
                var requestBody = new { name = name };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/v1/companies/{companyId}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<Dictionary<string, object>> DeleteCompanyAsync(string companyId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUri}/v1/companies/{companyId}");
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<Dictionary<string, object>> DeleteUserFromCompanyAsync(string companyId, string userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUri}/v1/companies/{companyId}/users/{userId}");
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Invitation Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<List<object>> GetInvitationsAsync(string companyId = null)
        {
            try
            {
                var url = string.IsNullOrEmpty(companyId)
                    ? $"{_baseUri}/v1/invitations"
                    : $"{_baseUri}/v1/invitations/{companyId}";
                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<object>>>(json);
                return result["invitations"];
            }
            catch (Exception e)
            {
                return new List<object> { HandleError(e) };
            }
        }

        // ─────────────────────────────────────────────────────────────
        // OAuth2 Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<List<object>> GetOauth2ProvidersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/oauth2");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<object>>(json);
            }
            catch (Exception e)
            {
                return new List<object> { HandleError(e) };
            }
        }

        public async Task<List<string>> GetUserOauth2ConnectionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/v1/user/oauth2");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<string>>(json);
            }
            catch (Exception e)
            {
                return new List<string> { HandleError(e) };
            }
        }

        public async Task<Dictionary<string, object>> Oauth2LoginAsync(string provider, string code, string referrer = null)
        {
            try
            {
                var requestBody = new { code = code, referrer = referrer };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/oauth2/{provider}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Training Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<Dictionary<string, object>> TrainAsync(string agentId, string datasetName = "dataset", string model = "unsloth/mistral-7b-v0.2", int maxSeqLength = 16384, string huggingfaceOutputPath = "JoshXT/finetuned-mistral-7b-v0.2", bool privateRepo = true)
        {
            try
            {
                var requestBody = new
                {
                    dataset_name = datasetName,
                    model = model,
                    max_seq_length = maxSeqLength,
                    huggingface_output_path = huggingfaceOutputPath,
                    private_repo = privateRepo
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/agent/{agentId}/train", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Audio Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<Dictionary<string, object>> TranscribeAudioAsync(string file, string model, string language = null, string prompt = null, string responseFormat = "json", float temperature = 0.0f)
        {
            try
            {
                var requestBody = new
                {
                    file = file,
                    model = model,
                    language = language,
                    prompt = prompt,
                    response_format = responseFormat,
                    temperature = temperature
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/audio/transcriptions", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<Dictionary<string, object>> TranslateAudioAsync(string file, string model, string prompt = null, string responseFormat = "json", float temperature = 0.0f)
        {
            try
            {
                var requestBody = new
                {
                    file = file,
                    model = model,
                    prompt = prompt,
                    response_format = responseFormat,
                    temperature = temperature
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/audio/translations", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Image Generation Methods
        // ─────────────────────────────────────────────────────────────

        public async Task<Dictionary<string, object>> GenerateImageAsync(string prompt, string model = "dall-e-3", int n = 1, string size = "1024x1024", string responseFormat = "url")
        {
            try
            {
                var requestBody = new
                {
                    prompt = prompt,
                    model = model,
                    n = n,
                    size = size,
                    response_format = responseFormat
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/v1/images/generations", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }
    }
}
