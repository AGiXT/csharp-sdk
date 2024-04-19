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

        public async Task<List<string>> GetProvidersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/provider");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                return result["providers"];
            }
            catch (Exception e)
            {
                return new List<string> { HandleError(e) };
            }
        }

        public async Task<List<string>> GetProvidersByServiceAsync(string service)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/providers/service/{service}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                return result["providers"];
            }
            catch (Exception e)
            {
                return new List<string> { HandleError(e) };
            }
        }

        public async Task<Dictionary<string, object>> GetProviderSettingsAsync(string providerName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/provider/{providerName}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["settings"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<List<string>> GetEmbedProvidersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/embedding_providers");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                return result["providers"];
            }
            catch (Exception e)
            {
                return new List<string> { HandleError(e) };
            }
        }

        public async Task<Dictionary<string, object>> GetEmbeddersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/embedders");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["embedders"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<Dictionary<string, object>> AddAgentAsync(string agentName, Dictionary<string, object> settings = null)
        {
            try
            {
                var requestBody = new
                {
                    agent_name = agentName,
                    settings = settings ?? new Dictionary<string, object>()
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/api/agent", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<Dictionary<string, object>> ImportAgentAsync(string agentName, Dictionary<string, object> settings = null, Dictionary<string, object> commands = null)
        {
            try
            {
                var requestBody = new
                {
                    agent_name = agentName,
                    settings = settings ?? new Dictionary<string, object>(),
                    commands = commands ?? new Dictionary<string, object>()
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/api/agent/import", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<string> RenameAgentAsync(string agentName, string newName)
        {
            try
            {
                var requestBody = new
                {
                    new_name = newName
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"{_baseUri}/api/agent/{agentName}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<string>(responseJson);
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> UpdateAgentSettingsAsync(string agentName, Dictionary<string, object> settings)
        {
            try
            {
                var requestBody = new
                {
                    settings = settings,
                    agent_name = agentName
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/api/agent/{agentName}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> UpdateAgentCommandsAsync(string agentName, Dictionary<string, object> commands)
        {
            try
            {
                var requestBody = new
                {
                    commands = commands,
                    agent_name = agentName
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/api/agent/{agentName}/commands", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> DeleteAgentAsync(string agentName)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUri}/api/agent/{agentName}");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
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
                var response = await _httpClient.GetAsync($"{_baseUri}/api/agent");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(json);
                return result["agents"];
            }
            catch (Exception e)
            {
                return new List<Dictionary<string, object>> { new Dictionary<string, object> { { "error", HandleError(e) } } };
            }
        }

        public async Task<Dictionary<string, object>> GetAgentConfigAsync(string agentName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/agent/{agentName}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["agent"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<List<string>> GetConversationsAsync(string agentName = "")
        {
            var url = string.IsNullOrEmpty(agentName)
                ? $"{_baseUri}/api/conversations"
                : $"{_baseUri}/api/{agentName}/conversations";

            try
            {
                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                return result["conversations"];
            }
            catch (Exception e)
            {
                return new List<string> { HandleError(e) };
            }
        }

        public async Task<List<Dictionary<string, object>>> GetConversationAsync(string agentName, string conversationName, int limit = 100, int page = 1)
        {
            try
            {
                var requestBody = new
                {
                    conversation_name = conversationName,
                    agent_name = agentName,
                    limit = limit,
                    page = page
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.GetAsync($"{_baseUri}/api/conversation");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(responseJson);
                return result["conversation_history"];
            }
            catch (Exception e)
            {
                return new List<Dictionary<string, object>> { new Dictionary<string, object> { { "error", HandleError(e) } } };
            }
        }

        public async Task<List<Dictionary<string, object>>> NewConversationAsync(string agentName, string conversationName, List<Dictionary<string, object>> conversationContent = null)
        {
            try
            {
                var requestBody = new
                {
                    conversation_name = conversationName,
                    agent_name = agentName,
                    conversation_content = conversationContent ?? new List<Dictionary<string, object>>()
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/api/conversation", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(responseJson);
                return result["conversation_history"];
            }
            catch (Exception e)
            {
                return new List<Dictionary<string, object>> { new Dictionary<string, object> { { "error", HandleError(e) } } };
            }
        }

        public async Task<string> DeleteConversationAsync(string agentName, string conversationName)
        {
            try
            {
                var requestBody = new
                {
                    conversation_name = conversationName,
                    agent_name = agentName
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.DeleteAsync($"{_baseUri}/api/conversation");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> DeleteConversationMessageAsync(string agentName, string conversationName, string message)
        {
            try
            {
                var requestBody = new
                {
                    message = message,
                    agent_name = agentName,
                    conversation_name = conversationName
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.DeleteAsync($"{_baseUri}/api/conversation/message");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> UpdateConversationMessageAsync(string agentName, string conversationName, string message, string newMessage)
        {
            try
            {
                var requestBody = new
                {
                    message = message,
                    new_message = newMessage,
                    agent_name = agentName,
                    conversation_name = conversationName
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/api/conversation/message", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> PromptAgentAsync(string agentName, string promptName, Dictionary<string, object> promptArgs)
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
                var response = await _httpClient.PostAsync($"{_baseUri}/api/agent/{agentName}/prompt", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["response"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> InstructAsync(string agentName, string userInput, string conversation)
        {
            return await PromptAgentAsync(agentName, "instruct", new Dictionary<string, object>
            {
                { "user_input", userInput },
                { "disable_memory", true },
                { "conversation_name", conversation }
            });
        }

        public async Task<string> ChatAsync(string agentName, string userInput, string conversation, int contextResults = 4)
        {
            return await PromptAgentAsync(agentName, "Chat", new Dictionary<string, object>
            {
                { "user_input", userInput },
                { "context_results", contextResults },
                { "conversation_name", conversation },
                { "disable_memory", true }
            });
        }

        public async Task<string> SmartInstructAsync(string agentName, string userInput, string conversation)
        {
            return await RunChainAsync(
                "Smart Instruct",
                userInput,
                agentName,
                false,
                1,
                new Dictionary<string, object>
                {
                    { "conversation_name", conversation },
                    { "disable_memory", true }
                }
            );
        }

        public async Task<string> SmartChatAsync(string agentName, string userInput, string conversation)
        {
            return await RunChainAsync(
                "Smart Chat",
                userInput,
                agentName,
                false,
                1,
                new Dictionary<string, object>
                {
                    { "conversation_name", conversation },
                    { "disable_memory", true }
                }
            );
        }

        public async Task<Dictionary<string, Dictionary<string, bool>>> GetCommandsAsync(string agentName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/agent/{agentName}/command");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, bool>>>>(json);
                return result["commands"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, Dictionary<string, bool>> { { "error", new Dictionary<string, bool> { { HandleError(e), false } } } };
            }
        }

        public async Task<string> ToggleCommandAsync(string agentName, string commandName, bool enable)
        {
            try
            {
                var requestBody = new
                {
                    command_name = commandName,
                    enable = enable
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"{_baseUri}/api/agent/{agentName}/command", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> ExecuteCommandAsync(string agentName, string commandName, Dictionary<string, object> commandArgs, string conversationName = "AGiXT Terminal Command Execution")
        {
            try
            {
                var requestBody = new
                {
                    command_name = commandName,
                    command_args = commandArgs,
                    conversation_name = conversationName
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/api/agent/{agentName}/command", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["response"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<List<string>> GetChainsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/chain");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<string>>(json);
            }
            catch (Exception e)
            {
                return new List<string> { HandleError(e) };
            }
        }

        public async Task<Dictionary<string, object>> GetChainAsync(string chainName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/chain/{chainName}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["chain"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<Dictionary<string, object>> GetChainResponsesAsync(string chainName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/chain/{chainName}/responses");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["chain"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<List<string>> GetChainArgsAsync(string chainName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/chain/{chainName}/args");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                return result["chain_args"];
            }
            catch (Exception e)
            {
                return new List<string> { HandleError(e) };
            }
        }

        public async Task<string> RunChainAsync(string chainName, string userInput, string agentName = "", bool allResponses = false, int fromStep = 1, Dictionary<string, object> chainArgs = null)
        {
            try
            {
                var requestBody = new
                {
                    prompt = userInput,
                    agent_override = agentName,
                    all_responses = allResponses,
                    from_step = fromStep,
                    chain_args = chainArgs ?? new Dictionary<string, object>()
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/api/chain/{chainName}/run", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<string>(responseJson);
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> RunChainStepAsync(string chainName, int stepNumber, string userInput, string agentName = null, Dictionary<string, object> chainArgs = null)
        {
            try
            {
                var requestBody = new
                {
                    prompt = userInput,
                    agent_override = agentName,
                    chain_args = chainArgs ?? new Dictionary<string, object>()
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/api/chain/{chainName}/run/step/{stepNumber}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<string>(responseJson);
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> AddChainAsync(string chainName)
        {
            try
            {
                var requestBody = new
                {
                    chain_name = chainName
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/api/chain", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> ImportChainAsync(string chainName, Dictionary<string, object> steps)
        {
            try
            {
                var requestBody = new
                {
                    chain_name = chainName,
                    steps = steps
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/api/chain/import", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> RenameChainAsync(string chainName, string newName)
        {
            try
            {
                var requestBody = new
                {
                    new_name = newName
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/api/chain/{chainName}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> DeleteChainAsync(string chainName)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUri}/api/chain/{chainName}");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> AddStepAsync(string chainName, int stepNumber, string agentName, string promptType, Dictionary<string, object> prompt)
        {
            try
            {
                var requestBody = new
                {
                    step_number = stepNumber,
                    agent_name = agentName,
                    prompt_type = promptType,
                    prompt = prompt
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/api/chain/{chainName}/step", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> UpdateStepAsync(string chainName, int stepNumber, string agentName, string promptType, Dictionary<string, object> prompt)
        {
            try
            {
                var requestBody = new
                {
                    step_number = stepNumber,
                    agent_name = agentName,
                    prompt_type = promptType,
                    prompt = prompt
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/api/chain/{chainName}/step/{stepNumber}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> MoveStepAsync(string chainName, int oldStepNumber, int newStepNumber)
        {
            try
            {
                var requestBody = new
                {
                    old_step_number = oldStepNumber,
                    new_step_number = newStepNumber
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"{_baseUri}/api/chain/{chainName}/step/move", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> DeleteStepAsync(string chainName, int stepNumber)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUri}/api/chain/{chainName}/step/{stepNumber}");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> AddPromptAsync(string promptName, string prompt, string promptCategory = "Default")
        {
            try
            {
                var requestBody = new
                {
                    prompt_name = promptName,
                    prompt = prompt
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/api/prompt/{promptCategory}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<Dictionary<string, object>> GetPromptAsync(string promptName, string promptCategory = "Default")
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/prompt/{promptCategory}/{promptName}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["prompt"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<List<string>> GetPromptsAsync(string promptCategory = "Default")
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/prompt/{promptCategory}");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                return result["prompts"];
            }
            catch (Exception e)
            {
                return new List<string> { HandleError(e) };
            }
        }

        public async Task<List<string>> GetPromptCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/prompt/categories");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                return result["prompt_categories"];
            }
            catch (Exception e)
            {
                return new List<string> { HandleError(e) };
            }
        }

        public async Task<Dictionary<string, object>> GetPromptArgsAsync(string promptName, string promptCategory = "Default")
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/prompt/{promptCategory}/{promptName}/args");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["prompt_args"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<string> DeletePromptAsync(string promptName, string promptCategory = "Default")
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUri}/api/prompt/{promptCategory}/{promptName}");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> UpdatePromptAsync(string promptName, string prompt, string promptCategory = "Default")
        {
            try
            {
                var requestBody = new
                {
                    prompt = prompt,
                    prompt_name = promptName,
                    prompt_category = promptCategory
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUri}/api/prompt/{promptCategory}/{promptName}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> RenamePromptAsync(string promptName, string newName, string promptCategory = "Default")
        {
            try
            {
                var requestBody = new
                {
                    prompt_name = newName
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"{_baseUri}/api/prompt/{promptCategory}/{promptName}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<Dictionary<string, object>> GetExtensionSettingsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/extensions/settings");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["extension_settings"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<List<string>> GetExtensionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/extensions");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                return result["extensions"];
            }
            catch (Exception e)
            {
                return new List<string> { HandleError(e) };
            }
        }

        public async Task<Dictionary<string, object>> GetCommandArgsAsync(string commandName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/extensions/{commandName}/args");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["command_args"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<Dictionary<string, object>> GetEmbeddersDetailsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/embedders");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
                return result["embedders"];
            }
            catch (Exception e)
            {
                return new Dictionary<string, object> { { "error", HandleError(e) } };
            }
        }

        public async Task<string> LearnTextAsync(string agentName, string userInput, string text, int collectionNumber = 0)
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
                var response = await _httpClient.PostAsync($"{_baseUri}/api/agent/{agentName}/learn/text", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> LearnUrlAsync(string agentName, string url, int collectionNumber = 0)
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
                var response = await _httpClient.PostAsync($"{_baseUri}/api/agent/{agentName}/learn/url", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> LearnFileAsync(string agentName, string fileName, string fileContent, int collectionNumber = 0)
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
                var response = await _httpClient.PostAsync($"{_baseUri}/api/agent/{agentName}/learn/file", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> LearnGitHubRepoAsync(string agentName, string githubRepo, string githubUser = null, string githubToken = null, string githubBranch = "main", bool useAgentSettings = false, int collectionNumber = 0)
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
                var response = await _httpClient.PostAsync($"{_baseUri}/api/agent/{agentName}/learn/github", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> LearnArxivAsync(string agentName, string query = null, string arxivIds = null, int maxResults = 5, int collectionNumber = 0)
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
                var response = await _httpClient.PostAsync($"{_baseUri}/api/agent/{agentName}/learn/arxiv", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> AgentReaderAsync(string agentName, string readerName, Dictionary<string, object> data, int collectionNumber = 0)
        {
            if (!data.ContainsKey("collection_number"))
            {
                data["collection_number"] = collectionNumber;
            }

            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/api/agent/{agentName}/reader/{readerName}", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> WipeAgentMemoriesAsync(string agentName, int collectionNumber = 0)
        {
            var url = collectionNumber == 0
                ? $"{_baseUri}/api/agent/{agentName}/memory"
                : $"{_baseUri}/api/agent/{agentName}/memory/{collectionNumber}";

            try
            {
                var response = await _httpClient.DeleteAsync(url);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> DeleteAgentMemoryAsync(string agentName, string memoryId, int collectionNumber = 0)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUri}/api/agent/{agentName}/memory/{collectionNumber}/{memoryId}");
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<List<Dictionary<string, object>>> GetAgentMemoriesAsync(string agentName, string userInput, int limit = 5, float minRelevanceScore = 0.0f, int collectionNumber = 0)
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
                var response = await _httpClient.PostAsync($"{_baseUri}/api/agent/{agentName}/memory/{collectionNumber}/query", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(responseJson);
                return result["memories"];
            }
            catch (Exception e)
            {
                return new List<Dictionary<string, object>> { new Dictionary<string, object> { { "error", HandleError(e) } } };
            }
        }

        public async Task<List<Dictionary<string, object>>> ExportAgentMemoriesAsync(string agentName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUri}/api/agent/{agentName}/memory/export");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(json);
                return result["memories"];
            }
            catch (Exception e)
            {
                return new List<Dictionary<string, object>> { new Dictionary<string, object> { { "error", HandleError(e) } } };
            }
        }

        public async Task<string> ImportAgentMemoriesAsync(string agentName, List<Dictionary<string, object>> memories)
        {
            try
            {
                var requestBody = new
                {
                    memories = memories
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/api/agent/{agentName}/memory/import", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> CreateDatasetAsync(string agentName, string datasetName, int batchSize = 4)
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
                var response = await _httpClient.PostAsync($"{_baseUri}/api/agent/{agentName}/memory/dataset", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> TrainAsync(string agentName = "AGiXT", string datasetName = "dataset", string model = "unsloth/mistral-7b-v0.2", int maxSeqLength = 16384, string huggingfaceOutputPath = "JoshXT/finetuned-mistral-7b-v0.2", bool privateRepo = true)
        {
            try
            {
                var requestBody = new
                {
                    model = model,
                    max_seq_length = maxSeqLength,
                    huggingface_output_path = huggingfaceOutputPath,
                    private_repo = privateRepo
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUri}/api/agent/{agentName}/memory/dataset/{datasetName}/finetune", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
                return result["message"];
            }
            catch (Exception e)
            {
                return HandleError(e);
            }
        }

        public async Task<string> TextToSpeechAsync(string agentName, string text, string conversationName)
        {
            return await ExecuteCommandAsync(
                agentName,
                "Text to Speech",
                new Dictionary<string, object>
                {
                    { "text", text }
                },
                conversationName
            );
        }
    }
}