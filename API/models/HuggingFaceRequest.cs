namespace API.models
{
    namespace HuggingFaceApiDemo.Models
    {
        public record HuggingFaceRequest(string Model, string Prompt);
    }
    public class ChatRequest
    {
        public List<ChatMessage> Messages { get; set; } = new();
        public string Model { get; set; } = string.Empty;
        public bool Stream { get; set; } = false;
    }

    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
