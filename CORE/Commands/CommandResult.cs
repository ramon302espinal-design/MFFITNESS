namespace CORE.Commands
{
    public sealed class CommandResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public object? Payload { get; init; }

        public static CommandResult Ok(string message = "Operación completada.", object? payload = null) =>
            new() { Success = true, Message = message, Payload = payload };

        public static CommandResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
