using HealthcareAI.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Diagnostics;
using ModelContextProtocol.Client;

namespace HealthcareAI.Services
{
    public class SemanticKernelService
    {
        private readonly Kernel _kernel;

        private readonly ChatHistoryStore _chatHistoryStore;

        private readonly LongTermMemoryStore _longTermMemoryStore;

        private readonly RagPlugin _ragPlugin;

        private readonly QuestionRouter _questionRouter;

        private readonly AgentRequestContext _requestContext;

        private readonly IAgentAuthorizationService _agentAuthorizationService;

        private readonly ILogger<SemanticKernelService> _logger;

        private readonly ILogger<HealthcarePlugin> _healthcarePluginLogger;

        private McpClient? _mcpClient;
        private IList<McpClientTool> _mcpTools = [];

        /// <summary>
        /// Semantic Kernel acts as the agent/orchestrator, and Ollama provides the local LLM that the agent uses for reasoning and tool/function calling.
        /// </summary>
        /// <param name="patientService"></param>
        /// <param name="appointmentService"></param>
        /// <param name="chatHistoryStore"></param>
        /// <param name="longTermMemoryStore"></param>

        public SemanticKernelService(IPatientService patientService, IAppointmentService appointmentService,
            ChatHistoryStore chatHistoryStore, LongTermMemoryStore longTermMemoryStore, RagPlugin ragPlugin, QuestionRouter questionRouter,
            AgentRequestContext requestContext, IAgentAuthorizationService agentAuthorizationService, ILogger<SemanticKernelService> logger, 
            ILogger<HealthcarePlugin> healthcarePluginLogger)
        {
            _chatHistoryStore = chatHistoryStore;
            _longTermMemoryStore = longTermMemoryStore;
            _ragPlugin = ragPlugin;
            _questionRouter = questionRouter;
            _requestContext = requestContext;
            _agentAuthorizationService = agentAuthorizationService;
            _logger = logger;
            _healthcarePluginLogger = healthcarePluginLogger;

            var builder = Kernel.CreateBuilder();

            builder.AddOllamaChatCompletion(
                modelId: "qwen2.5:3b",
                endpoint: new Uri("http://localhost:11434"));

            _kernel = builder.Build();

            //// Semantic Kernel will have two tool groups:Healthcare and Memory. 
            //// Healthcare group will have functions from HealthcarePlugin and Memory group will have functions from MemoryPlugin.

            _kernel.Plugins.AddFromObject(new HealthcarePlugin(patientService, appointmentService, requestContext, agentAuthorizationService, healthcarePluginLogger), "Healthcare");
            _kernel.Plugins.AddFromObject(ragPlugin,"RAG");

            ////foreach (var function in _kernel.Plugins["Healthcare"])
            ////{
            ////    Console.WriteLine($"Registered Function: {function.Name}");
            ////}

            ////foreach (var function in _kernel.Plugins["RAG"])
            ////{
            ////    Console.WriteLine($"Registered Function: {function.Name}");
            ////}
        }

        /// <summary>
        /// HealthcareAI agent, memory, and function calling.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="userId"></param>
        /// <param name="question"></param>
        /// <returns></returns>

        public async Task<string> AskAsync(string conversationId, string userId, string question)
        {
            await InitializeMcpAsync();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation(
                            "Agent request started. UserId={UserId}, ConversationId={ConversationId}",
                            userId,
                            conversationId);

            string intent = _questionRouter.GetIntent(question);

            _logger.LogInformation("Agent intent detected. UserId: {UserId}, Intent: {Intent}",
                                    userId,
                                    intent);

            bool requiresRag = intent == "Rag" || intent == "MemoryAndRag";

            var chatService = _kernel.GetRequiredService<IChatCompletionService>();

            string memoryResult = string.Empty;
            string ragResult = string.Empty;

            var preferredLanguage = await _longTermMemoryStore.GetMemoryAsync(userId, "preferred_language");
            var accessLevel = await _longTermMemoryStore.GetMemoryAsync(userId, "access_level");

            _requestContext.UserId = userId;
            _requestContext.AccessLevel = accessLevel ?? string.Empty;

            //if (requiresRag)
            //{
            //    var ragResult = await _ragPlugin.SearchHospitalPolicyAsync(question);

            //    return ragResult;
            //}

            // If the question requires RAG, we will first retrieve relevant hospital policy information and then use that information to answer the user's question.
            // If the question does not require RAG, we will use the Semantic Kernel to answer the question directly.
            // This is a simple deterministic routing layer that decides whether to use RAG or not based on the user's question.
            // The RAG retrieval logic is in the RagPlugin class, which is registered as a Semantic Kernel plugin.
            // The Semantic Kernel will call the SearchHospitalPolicyAsync function in the RagPlugin to retrieve relevant hospital policy information.
            // The retrieved information will then be used to answer the user's question.
            // This is a simple example of how to use RAG with Semantic Kernel and Ollama.
            // The RAG retrieval logic can be improved by using a more sophisticated method to determine whether to use RAG or not, such as using a machine learning model to classify the user's question.
            // The RAG retrieval logic can also be improved by using a more sophisticated method to retrieve relevant hospital policy information, such as using a vector database to store and retrieve document embeddings.
            // The RAG retrieval logic can also be improved by using a more sophisticated method to answer the user's question, such as using a more advanced LLM or using a more advanced prompt engineering technique.
            // RAG + LLM

            ////if (requiresRag)
            ////{
            ////    var ragResult = await _ragPlugin.SearchHospitalPolicyAsync(question);

            ////    var ragPrompt = $"""
            ////                    Answer the user's question using only the hospital policy information below.

            ////                    Hospital Policy:
            ////                    {ragResult}

            ////                    User Question:
            ////                    {question}

            ////                    Rules:
            ////                    - Use only the provided hospital policy.
            ////                    - Do not invent information.
            ////                    - Give a clear and concise answer.
            ////                    - If the policy does not contain the answer, say that the policy does not provide this information.
            ////                    """;

            ////    var ragAnswer = await chatService.GetChatMessageContentAsync(
            ////        ragPrompt);

            ////    return ragAnswer.Content ?? string.Empty;
            ////}


            if (intent == "Memory" || intent == "MemoryAndRag")
            {
                if (!string.IsNullOrEmpty(preferredLanguage))
                {
                    memoryResult =
                        $"User's preferred language is {preferredLanguage}.";
                }
            }

            if (intent == "Rag" || intent == "MemoryAndRag")
            {
                if (string.IsNullOrEmpty(accessLevel))
                {
                    return "You are not authorized to access hospital policy information.";
                }

                var ragStopwatch = System.Diagnostics.Stopwatch.StartNew();

                _logger.LogInformation(
                                        "RAG search started. UserId: {UserId}, AccessLevel: {AccessLevel}",
                                        userId,
                                        accessLevel);

                ragResult = await _ragPlugin.SearchHospitalPolicyAsync(question,accessLevel);

                ragStopwatch.Stop();

                _logger.LogInformation("RAG search completed. DurationMs: {DurationMs}",ragStopwatch.ElapsedMilliseconds);

            }

            if (intent == "MemoryAndRag")
            {
                            var combinedPrompt = $"""
                    Answer the user's question using the information provided below.

                    Memory information:
                    {memoryResult}

                    Hospital policy information:
                    {ragResult}

                    User question:
                    {question}

                    Rules:
                    - Use the memory information for personal preferences.
                    - Use the hospital policy information for hospital-related questions.
                    - Do not invent information.
                    - Give one clear and concise answer.
                    """;

                var finalResponse =
                    await chatService.GetChatMessageContentAsync(combinedPrompt);

                return finalResponse.Content ?? string.Empty;
            }

            ////var ragResult = await _ragPlugin.SearchHospitalPolicyAsync("What should I do if I cancel my appointment late?");

            ////Console.WriteLine("RAG TEST:");
            ////Console.WriteLine(ragResult);

            _kernel.Plugins.AddFromObject(new MemoryPlugin(_longTermMemoryStore, userId), "Memory");

            //// The model can decide whether to call an available function/tool.

            var settings = new PromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            };

            var history = _chatHistoryStore.GetOrCreateChat(conversationId);

            // var preferredLanguage = await _longTermMemoryStore.GetMemoryAsync(userId, "PreferredLanguage");

            var memoryContext = string.Empty;

            if (!string.IsNullOrEmpty(preferredLanguage))
            {
                memoryContext = $"User's preferred language is {preferredLanguage}.";
            }


            history.AddSystemMessage($""""
                You are HealthcareAI, a healthcare information assistant.

                Rules:
                - Provide general health information only.
                - Do not diagnose diseases.
                - Do not prescribe medicines or dosages.
                - For serious or emergency symptoms, recommend consulting a qualified healthcare professional.
                - When the user asks to create an appointment using a patient's name, use the CreateAppointmentForPatient tool.
                - Do not ask the user to confirm the patient name if the patient name is already provided.
                - Answer clearly and simply.

                 Long-Term Memory Rules:
            
                - Use the Memory tool when you need to retrieve information previously saved about the current user.
                - When the user asks about a preference or personal information that may have been stored in memory, use GetUserMemoryAsync before answering.
                - For example, if the user asks "What is my preferred language?", retrieve the "preferred_language" memory.
                - When the user provides a useful non-sensitive preference or personal information, use SaveUserMemoryAsync to remember it.
                - Do not save every user message.
                - Do not save temporary information.
                - Do not save sensitive healthcare information.

                Long-Term Memory:{memoryContext}

                RAG Rules:
                - Use the SearchHospitalPolicyAsync tool when the user asks about hospital policies, appointment rules, cancellation rules, arrival requirements, or other information that should come from hospital documents.
                - Use the retrieved policy information to answer the user.
                - Do not invent hospital policy information.
                """"
             );

            ////history.AddUserMessage(question);

            if(intent == "Rag")
            {
                ////        history.AddUserMessage($"""
                ////Use the following hospital policy information to answer the user's question.

                ////Hospital policy information:
                ////{ragResult}

                ////User question:
                ////{question}

                ////Answer only using the hospital policy information provided above.
                ////""");

                history.AddUserMessage($"""
                        Answer the user's question using ONLY the hospital policy information below.

                        Hospital policy information:
                        {ragResult}

                        User question:
                        {question}

                        Rules:
                        - Do not add information that is not stated in the policy.
                        - Do not assume or interpret rules differently from the policy.
                        - When the policy gives a time limit, apply that rule to the user's specific time.
                        - Example: if the policy requires cancellation at least 24 hours before the appointment, any cancellation less than 24 hours before the appointment requires contacting hospital support.
                        - If the policy says an action is required, clearly state that action.
                        - If the policy does not answer the question, say that the hospital policy information does not provide the answer.
                        """);
            }
            else
            {
                history.AddUserMessage(question);
            }

            var response = await chatService.GetChatMessageContentAsync(
                history,
                executionSettings: settings,
                kernel: _kernel
                );

            stopwatch.Stop();

            _logger.LogInformation(
                "Agent request completed. UserId: {UserId}, DurationMs: {DurationMs}",
                userId,
                stopwatch.ElapsedMilliseconds);

            return response.Content ?? string.Empty;
        }

        /// <summary>
        /// create a separate RAG method
        /// </summary>
        /// <param name="question"></param>
        /// <param name="context"></param>
        /// <returns></returns>

        public async Task<string> AskRagAsync(string question, string context)
        {
            var chatService = _kernel.GetRequiredService<IChatCompletionService>();

            var history = new ChatHistory();

            history.AddSystemMessage("""
                            You are a healthcare policy assistant.

                            Follow these rules strictly:

                              1. Answer the user's question using ONLY the provided context.
                              2. Do not use your own knowledge or assumptions.
                              3. Do not invent or guess information.
                              4. If the answer is not available in the context, say:
                                 "I don't have enough information in the provided policy."
                              5. Keep the answer clear and concise.
                              6. If the context contains multiple relevant points, combine them accurately.
                            """);

            history.AddUserMessage($"""
                                    Context:
                                    {context}

                                    Question:
                                    {question}
                                    """);

            var response = await chatService.GetChatMessageContentAsync(
                history,
                kernel: _kernel);

            return response.Content ?? string.Empty;
        }

        /// <summary>
        /// RAG Conversation Flow
        /// Now we will make RAG remember the current conversation.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="question"></param>
        /// <param name="context"></param>
        /// <returns></returns>

        public async Task<string> AskRagConversationAsync(
                                                            string conversationId,
                                                            string question,
                                                            string context)
        {
            var chatService =
                _kernel.GetRequiredService<IChatCompletionService>();

            //This created a new conversation every time.

            var history =
                _chatHistoryStore.GetOrCreateChat(conversationId);

            if (history.Count == 0)
            {
                history.AddSystemMessage("""
                                        You are a healthcare policy assistant.

                                        Follow these rules strictly:

                                        1. Answer using ONLY the provided RAG context.
                                        2. Do not use your own knowledge or assumptions.
                                        3. Do not invent or guess information.
                                        4. If the answer is not available in the context, say:
                                           "I don't have enough information in the provided policy."
                                        5. Keep answers clear and concise.
                                        """);
            }

            history.AddUserMessage($"""
                                    RAG Context:
                                    {context}

                                    User Question:
                                    {question}
                                    """);

            var response =
                await chatService.GetChatMessageContentAsync(
                    history,
                    kernel: _kernel);

            return response.Content ?? string.Empty;
        }

        private async Task InitializeMcpAsync()
        {
            if (_mcpClient != null)
                return;

            var clientTransport = new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Endpoint = new Uri("http://localhost:3001")
                });

            _mcpClient = await McpClient.CreateAsync(clientTransport);

            _mcpTools = await _mcpClient.ListToolsAsync();

            var kernelFunctions = _mcpTools
                .Select(tool => tool.AsKernelFunction())
                .ToList();

            _kernel.Plugins.AddFromFunctions(
                "MCP",
                kernelFunctions);

            _logger.LogInformation(
                "MCP tools registered in Semantic Kernel: {ToolCount}",
                _mcpTools.Count);

            foreach (var tool in _mcpTools)
            {
                _logger.LogInformation(
                    "MCP Kernel function registered: {ToolName}",
                    tool.Name);
            }
        }
    }
}
