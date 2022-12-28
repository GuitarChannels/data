using System;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
	public static class YoutubeAPIServices
	{
		private static Random rnd = new Random();

		public static void AddYoutubeAPI(this IServiceCollection services)
		{
			string[] googleApiKeys = Environment.GetEnvironmentVariable("GOOGLE_API_KEYS").Split(",");

			// youtube api
			services.AddTransient(s => new YouTubeService(new BaseClientService.Initializer
			{
				ApiKey = googleApiKeys[rnd.Next(googleApiKeys.Length)],
				ApplicationName = "Guitar-Channels"
			}));
		}
	}
}