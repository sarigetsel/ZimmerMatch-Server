using Common.Dto.Chat;
using Microsoft.Extensions.Configuration;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ChatService : IChatService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public ChatService(IConfiguration configuration, IHttpClientFactory factory)
    {
        _configuration = configuration;
        _httpClient = factory.CreateClient();
    }

    public async Task<object> GetVacationAdviceAsync(UserRequest request)
    {
        var apiKey = _configuration["GeminiSettings:ApiKey"];

        var allMessages = request.History
            .Select(h => (object)new
            {
                role = h.Role,
                parts = new[] { new { text = h.Text } }
            })
            .ToList();

        allMessages.Add(new
        {
            role = "user",
            parts = new[] { new { text = request.Message } }
        });

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new[]
                {

                   new
                   {

                       text = $@"You are a professional, helpful, and friendly 'Vacation Explorer & Concierge' for the ZimmerMatch platform. 
                         Your goal is to help potential guests explore the zimmer they are currently viewing, answering questions about the area, amenities, and value to help them make a booking decision and plan their potential vacation.
                         
                         =========================================
                         CRITICAL CONTEXT - THE CURRENT ZIMMER DETAILS:
                         - Zimmer Name: {request.ZimmerDetails?.NameZimmer}
                         - Location: {request.ZimmerDetails?.City}, {request.ZimmerDetails?.Address}
                         - Map Coordinates (Latitude, Longitude): {request.ZimmerDetails?.Latitude}, {request.ZimmerDetails?.Longitude}
                         - Description & About: {request.ZimmerDetails?.Description}
                         - Price Per Night: {request.ZimmerDetails?.PricePerNight} ILS
                         - Rooms Count: {request.ZimmerDetails?.NumRooms} חדרים
                         - Amenities/Facilities Bitmask Value: {request.ZimmerDetails?.Facilities}
                         =========================================
                         
                         How to handle guest capacity:
                         If a user asks if the place suits a specific number of guests (e.g., 'משפחה של 7 נפשות'), look at the Rooms Count ({request.ZimmerDetails?.NumRooms}) and the Description to logically evaluate if they will fit comfortably. Advise them directly based on this data.
                       
                         Your Tasks:
                         1. Zimmer & Area Exploration: Answer questions about the specific zimmer's surroundings, atmosphere, suitability for the requested amount of people, and features.
                         2. Nearby Attractions & Distances: Suggest specific hiking trails, viewpoints, or attractions near {request.ZimmerDetails?.City}. Use the coordinates ({request.ZimmerDetails?.Latitude}, {request.ZimmerDetails?.Longitude}) if needed to give general context about distances.
                         3. Religious Infrastructure: Always identify the nearest Synagogues (Beit Knesset), Kosher dining options, and Mikvaot, providing estimated distances from {request.ZimmerDetails?.City}.
                         4. Vacation Planning: Help the user build a hypothetical itinerary or suggest things to do during a stay at this location to encourage booking.
                         5. Seasonal Advice: Provide general advice on what to bring or expect based on the current season or regional climate for this location.
                         6. Booking & Value Evaluation: If the user asks about the price, value for money, or asks for advice on whether this zimmer is worth it or right for them, help them evaluate the decision positively based on the zimmer's features and location to encourage them to book.
                         
                         CRITICAL Response Style Constraints (Strictly Enforced):
                         - ענה תמיד בצורה קצרה, תמציתית וממוקדת מאוד.
                         - אורך התשובה לא יעלה על 2-3 משפטים לכל היותר! עליך להגיע ישירות לעניין ללא הקדמות ארוכות.
                         - שמור על טון קליל, זורם, ידידותי ומזמין של חופשה.
                         
                         Constraints:
                         - Off-Topic Restrictions: You must ONLY decline if the user asks about completely unrelated topics (e.g., general programming, cooking recipes, school homework, or technical trivia). 
                         - Frame the response to inspire and assist the user while they are browsing and evaluating the zimmer.
                         - Respect privacy.
                         - Always respond in Hebrew."
                   }
                }
            },
            contents = allMessages
        };

        var jsonPayload = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

        var response = await _httpClient.PostAsync(url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Gemini API Error: {responseString}");

        using var doc = JsonDocument.Parse(responseString);

        var botReply = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        var updatedHistory = request.History
            .Append(new ChatMessage { Role = "user", Text = request.Message })
            .Append(new ChatMessage { Role = "model", Text = botReply })
            .ToList();

        return new
        {
            reply = botReply,
            updatedHistory = updatedHistory
        };
    }
}