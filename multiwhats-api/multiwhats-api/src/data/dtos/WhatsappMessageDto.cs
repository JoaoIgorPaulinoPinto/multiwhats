namespace multiwhats_api.src.data.dtos
{
    public class WhatsappMessageDto
    {
        public string From { get; set; }
        public string Body { get; set; }
        public long Timestamp { get; set; }
        public string NotifyName { get; set; }
    }
}
