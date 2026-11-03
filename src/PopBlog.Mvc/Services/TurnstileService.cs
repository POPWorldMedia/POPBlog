using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PopBlog.Mvc.Services
{
	public interface ITurnstileService
	{
		Task<TurnstileResponse> VerifyToken(string token, string ip);
	}

	public class TurnstileService : ITurnstileService
	{
		private readonly IConfig _config;
		private const string VerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
		private static readonly HttpClient Client = new();

		public TurnstileService(IConfig config)
		{
			_config = config;
		}

		public async Task<TurnstileResponse> VerifyToken(string token, string ip)
		{
			if (string.IsNullOrEmpty(token))
				return new TurnstileResponse { IsSuccess = false, ErrorCodes = new[] { "missing-input-response" } };
			var values = new Dictionary<string, string>
			{
				{"secret", _config.TurnstileSecretKey},
				{"response", token},
				{"remoteip", ip}
			};
			try
			{
				var httpResult = await Client.PostAsync(VerifyUrl, new FormUrlEncodedContent(values));
				var content = await httpResult.Content.ReadAsStringAsync();
				var verifyResult = JsonSerializer.Deserialize<TurnstileResponse>(content);
				verifyResult.ErrorCodes ??= Array.Empty<string>();
				return verifyResult;
			}
			catch (Exception exc) when (exc is HttpRequestException or JsonException or TaskCanceledException)
			{
				return new TurnstileResponse { IsSuccess = false, ErrorCodes = new[] { exc.GetType().Name } };
			}
		}
	}

	public class TurnstileResponse
	{
		[JsonPropertyName("success")]
		public bool IsSuccess { get; set; }
		[JsonPropertyName("challenge_ts")]
		public DateTime? ChallengeTimeStamp { get; set; }
		[JsonPropertyName("hostname")]
		public string HostName { get; set; }
		[JsonPropertyName("error-codes")]
		public string[] ErrorCodes { get; set; }
		[JsonPropertyName("action")]
		public string Action { get; set; }
	}
}
