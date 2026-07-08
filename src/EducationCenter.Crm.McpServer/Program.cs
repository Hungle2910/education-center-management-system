using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EducationCenter.Crm.Application;
using EducationCenter.Crm.Application.CoreData;
using EducationCenter.Crm.Application.Schedules;
using EducationCenter.Crm.Application.Attendance;
using EducationCenter.Crm.Infrastructure;

namespace EducationCenter.Crm.McpServer
{
    internal class Program
    {
        private static IServiceProvider _serviceProvider = null!;

        static async Task Main(string[] args)
        {
            // Set Console encoding to UTF-8 for Vietnamese characters support
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            try
            {
                var basePath = AppContext.BaseDirectory;
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var services = new ServiceCollection();
                services.AddSingleton<IConfiguration>(configuration);
                services.AddApplication();
                services.AddInfrastructure(configuration);

                _serviceProvider = services.BuildServiceProvider();

                await LogAsync("Education Center CRM MCP Server initialized and listening...");

                // Run message loop
                await RunMessageLoopAsync();
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"Critical initialization failure: {ex}");
                Environment.Exit(1);
            }
        }

        private static async Task RunMessageLoopAsync()
        {
            using var reader = new StreamReader(Console.OpenStandardInput(), System.Text.Encoding.UTF8);
            
            while (true)
            {
                var line = await reader.ReadLineAsync();
                if (line == null)
                {
                    await LogAsync("Input stream closed. Exiting.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(line)) continue;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var request = JsonSerializer.Deserialize<JsonRpcMessage>(line);
                        if (request == null) return;

                        await HandleMessageAsync(request);
                    }
                    catch (Exception ex)
                    {
                        await LogErrorAsync($"Error processing line: {ex}");
                    }
                });
            }
        }

        private static async Task HandleMessageAsync(JsonRpcMessage message)
        {
            if (message.Method == "initialize")
            {
                var response = new JsonRpcResponse
                {
                    JsonRpc = "2.0",
                    Id = message.Id,
                    Result = new InitializeResult
                    {
                        ProtocolVersion = "2024-11-05",
                        Capabilities = new ServerCapabilities
                        {
                            Tools = new ToolCapabilities()
                        },
                        ServerInfo = new ImplementationInfo
                        {
                            Name = "education-center-crm-mcp",
                            Version = "1.0.0"
                        }
                    }
                };
                await SendResponseAsync(response);
            }
            else if (message.Method == "notifications/initialized")
            {
                // Accept silently
                await LogAsync("Client confirmed initialization.");
            }
            else if (message.Method == "tools/list")
            {
                var response = new JsonRpcResponse
                {
                    JsonRpc = "2.0",
                    Id = message.Id,
                    Result = new ToolListResult
                    {
                        Tools = new List<McpTool>
                        {
                            new McpTool
                            {
                                Name = "list_students",
                                Description = "Lấy danh sách tất cả học sinh đang theo học tại trung tâm.",
                                InputSchema = GetEmptySchema()
                            },
                            new McpTool
                            {
                                Name = "list_classes",
                                Description = "Lấy danh sách các lớp học hiện tại.",
                                InputSchema = GetEmptySchema()
                            },
                            new McpTool
                            {
                                Name = "list_teachers",
                                Description = "Lấy danh sách giáo viên của trung tâm.",
                                InputSchema = GetEmptySchema()
                            },
                            new McpTool
                            {
                                Name = "list_schedules",
                                Description = "Lấy lịch học và sự kiện trong một khoảng thời gian nhất định (mặc định 30 ngày tới).",
                                InputSchema = GetScheduleSchema()
                            },
                            new McpTool
                            {
                                Name = "get_attendance",
                                Description = "Xem chi tiết thông tin điểm danh của một ca học/buổi học cụ thể theo Occurrence ID.",
                                InputSchema = GetOccurrenceSchema()
                            }
                        }
                    }
                };
                await SendResponseAsync(response);
            }
            else if (message.Method == "tools/call")
            {
                await HandleToolCallAsync(message);
            }
            else
            {
                if (message.Id != null)
                {
                    var response = new JsonRpcResponse
                    {
                        JsonRpc = "2.0",
                        Id = message.Id,
                        Error = new JsonRpcError
                        {
                            Code = -32601,
                            Message = $"Method '{message.Method}' not found."
                        }
                    };
                    await SendResponseAsync(response);
                }
            }
        }

        private static async Task HandleToolCallAsync(JsonRpcMessage message)
        {
            var callParams = message.Params?.Deserialize<ToolCallParams>();
            if (callParams == null || string.IsNullOrEmpty(callParams.Name))
            {
                await SendErrorResponseAsync(message.Id, -32602, "Invalid params. Tool name is required.");
                return;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                object? resultData = null;

                switch (callParams.Name)
                {
                    case "list_students":
                        {
                            var service = scope.ServiceProvider.GetRequiredService<IStudentService>();
                            resultData = await service.GetAllAsync(CancellationToken.None);
                        }
                        break;

                    case "list_classes":
                        {
                            var service = scope.ServiceProvider.GetRequiredService<IClassService>();
                            resultData = await service.GetAllAsync(CancellationToken.None);
                        }
                        break;

                    case "list_teachers":
                        {
                            var service = scope.ServiceProvider.GetRequiredService<ITeacherService>();
                            resultData = await service.GetAllAsync(CancellationToken.None);
                        }
                        break;

                    case "list_schedules":
                        {
                            var service = scope.ServiceProvider.GetRequiredService<IScheduleService>();
                            
                            DateOnly startDate = DateOnly.FromDateTime(DateTime.Today);
                            DateOnly endDate = startDate.AddDays(30);

                            if (callParams.Arguments != null && callParams.Arguments.Value.ValueKind == JsonValueKind.Object)
                            {
                                if (callParams.Arguments.Value.TryGetProperty("startDate", out var startProp) && startProp.ValueKind == JsonValueKind.String)
                                {
                                    DateOnly.TryParse(startProp.GetString(), out startDate);
                                }
                                if (callParams.Arguments.Value.TryGetProperty("endDate", out var endProp) && endProp.ValueKind == JsonValueKind.String)
                                {
                                    DateOnly.TryParse(endProp.GetString(), out endDate);
                                }
                            }

                            resultData = await service.GetCalendarAsync(startDate, endDate, CancellationToken.None);
                        }
                        break;

                    case "get_attendance":
                        {
                            var service = scope.ServiceProvider.GetRequiredService<IAttendanceService>();
                            Guid occurrenceId = Guid.Empty;

                            if (callParams.Arguments != null && callParams.Arguments.Value.ValueKind == JsonValueKind.Object)
                            {
                                if (callParams.Arguments.Value.TryGetProperty("occurrenceId", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                                {
                                    Guid.TryParse(idProp.GetString(), out occurrenceId);
                                }
                            }

                            if (occurrenceId == Guid.Empty)
                            {
                                await SendErrorResponseAsync(message.Id, -32602, "occurrenceId is required.");
                                return;
                            }

                            resultData = await service.GetAttendanceByOccurrenceAsync(occurrenceId, CancellationToken.None);
                        }
                        break;

                    default:
                        await SendErrorResponseAsync(message.Id, -32601, $"Unknown tool: {callParams.Name}");
                        return;
                }

                var toolResult = new ToolCallResult
                {
                    Content = new List<McpContent>
                    {
                        new McpContent
                        {
                            Type = "text",
                            Text = JsonSerializer.Serialize(resultData, new JsonSerializerOptions { WriteIndented = true })
                        }
                    }
                };

                var response = new JsonRpcResponse
                {
                    JsonRpc = "2.0",
                    Id = message.Id,
                    Result = toolResult
                };
                await SendResponseAsync(response);
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"Error executing tool {callParams.Name}: {ex}");
                await SendErrorResponseAsync(message.Id, -32603, $"Internal tool execution error: {ex.Message}");
            }
        }

        private static async Task SendResponseAsync(object response)
        {
            var json = JsonSerializer.Serialize(response);
            await Console.Out.WriteLineAsync(json);
            await Console.Out.FlushAsync();
        }

        private static async Task SendErrorResponseAsync(object? id, int code, string message)
        {
            var response = new JsonRpcResponse
            {
                JsonRpc = "2.0",
                Id = id,
                Error = new JsonRpcError
                {
                    Code = code,
                    Message = message
                }
            };
            await SendResponseAsync(response);
        }

        private static async Task LogAsync(string text)
        {
            await Console.Error.WriteLineAsync($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {text}");
        }

        private static async Task LogErrorAsync(string text)
        {
            await Console.Error.WriteLineAsync($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {text}");
        }

        private static McpSchema GetEmptySchema()
        {
            return new McpSchema
            {
                Type = "object",
                Properties = new Dictionary<string, object>()
            };
        }

        private static McpSchema GetScheduleSchema()
        {
            return new McpSchema
            {
                Type = "object",
                Properties = new Dictionary<string, object>
                {
                    { "startDate", new { type = "string", description = "Ngày bắt đầu tìm kiếm lịch học, định dạng YYYY-MM-DD" } },
                    { "endDate", new { type = "string", description = "Ngày kết thúc tìm kiếm lịch học, định dạng YYYY-MM-DD" } }
                }
            };
        }

        private static McpSchema GetOccurrenceSchema()
        {
            return new McpSchema
            {
                Type = "object",
                Properties = new Dictionary<string, object>
                {
                    { "occurrenceId", new { type = "string", description = "ID của ca học/buổi học cần tra cứu điểm danh (Guid)" } }
                },
                Required = new List<string> { "occurrenceId" }
            };
        }
    }

    #region Mcp Types
    public class JsonRpcMessage
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonPropertyName("id")]
        public object? Id { get; set; }

        [JsonPropertyName("method")]
        public string? Method { get; set; }

        [JsonPropertyName("params")]
        public JsonElement? Params { get; set; }
    }

    public class JsonRpcResponse
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonPropertyName("id")]
        public object? Id { get; set; }

        [JsonPropertyName("result")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Result { get; set; }

        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonRpcError? Error { get; set; }
    }

    public class JsonRpcError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
    }

    public class InitializeResult
    {
        [JsonPropertyName("protocolVersion")]
        public string ProtocolVersion { get; set; } = "2024-11-05";

        [JsonPropertyName("capabilities")]
        public ServerCapabilities Capabilities { get; set; } = null!;

        [JsonPropertyName("serverInfo")]
        public ImplementationInfo ServerInfo { get; set; } = null!;
    }

    public class ServerCapabilities
    {
        [JsonPropertyName("tools")]
        public ToolCapabilities Tools { get; set; } = null!;
    }

    public class ToolCapabilities { }

    public class ImplementationInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("version")]
        public string Version { get; set; } = "";
    }

    public class ToolListResult
    {
        [JsonPropertyName("tools")]
        public List<McpTool> Tools { get; set; } = null!;
    }

    public class McpTool
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("inputSchema")]
        public McpSchema InputSchema { get; set; } = null!;
    }

    public class McpSchema
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "object";

        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = null!;

        [JsonPropertyName("required")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Required { get; set; }
    }

    public class ToolCallParams
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("arguments")]
        public JsonElement? Arguments { get; set; }
    }

    public class ToolCallResult
    {
        [JsonPropertyName("content")]
        public List<McpContent> Content { get; set; } = null!;
    }

    public class McpContent
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";

        [JsonPropertyName("text")]
        public string Text { get; set; } = "";
    }
    #endregion
}
