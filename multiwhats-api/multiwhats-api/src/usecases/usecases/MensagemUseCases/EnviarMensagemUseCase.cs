using System.Text;
using System.Text.Json;
using multiwhats_api.src.data.dtos.Requests;
using multiwhats_api.src.usecases.interfaces.MensagemInterfaces;

namespace multiwhats_api.src.usecases.usecases.MensagemUseCases;

public class EnviarMensagemUseCase : IEnviarMensagemUseCase
{
    private readonly HttpClient _httpClient;

    public EnviarMensagemUseCase(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> Execute(EnviarMensagemRequest req)
    {
        try
        {
            var payloadNode = new
            {
                numero = req.Numero,
                mensagem = req.Conteudo
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payloadNode),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("http://localhost:3000/api/enviar", jsonContent);

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[ASP.NET] Resposta do Node.js -> Status: {response.StatusCode} | Corpo: {responseBody}");

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao integrar com a API do WhatsApp: {ex.Message}");
            return false;
        }
    }
}