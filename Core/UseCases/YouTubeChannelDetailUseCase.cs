using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DTO;
using Core.DTO.UseCaseRequests;
using Core.DTO.UseCaseResponses;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Interfaces.UseCases;

namespace Core.UseCases
{
	public class YouTubeChannelDetailUseCase
		: IYouTubeChannelDetailUseCase
	{
		private readonly IChannelRepository _channelRepository;
		private readonly IYouTubeDataService _youtubeDataService;
		private readonly IGuitarTermRepository _guitarTermRepository;

		public YouTubeChannelDetailUseCase(
			IYouTubeDataService youtubeDataService,
			IChannelRepository channelRepository,
			IGuitarTermRepository guitarTermRepository
		)
		{
			_youtubeDataService = youtubeDataService ?? throw new ArgumentNullException(nameof(youtubeDataService));
			_channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
			_guitarTermRepository = guitarTermRepository ?? throw new ArgumentNullException(nameof(guitarTermRepository));
		}

		public async Task<YouTubeChannelDetailResponse> Handle(YouTubeChannelDetailRequest message)
		{
			var identifiedChannels = new List<ChannelIdentificationDto>();

			var guitarTerms = await _guitarTermRepository.GetAll();
			var storedChannels = await _channelRepository.GetAll(message.ChannelIdsToCheck);

			foreach (var storedChannel in storedChannels)
			{
				identifiedChannels.Add(new ChannelIdentificationDto
				{
					ChannelId = storedChannel.Id,
					Channel = storedChannel,
					IsGuitarChannel = CheckGuitarTerms(guitarTerms, storedChannel),
					Source = "db"
				});
			}

			var storedChannelIds = storedChannels.Select(c => c.Id);
			var ytFetchChannelIds = message.ChannelIdsToCheck.Except(storedChannelIds).ToList();

			var ytChannels = await _youtubeDataService.GetChannelDetails(ytFetchChannelIds);

			foreach (var ytChannel in ytChannels)
			{
				identifiedChannels.Add(new ChannelIdentificationDto
				{
					ChannelId = ytChannel.Id,
					Channel = ytChannel,
					IsGuitarChannel = CheckGuitarTerms(guitarTerms, ytChannel),
					Source = "yt"
				});
			}

			var response = new YouTubeChannelDetailResponse(identifiedChannels);
			return response;
		}

		/// <summary>
		/// Checks if a channel contains guitar terms and is therefore a guitar channel
		/// </summary>
		/// <param name="terms"></param>
		/// <param name="channel"></param>
		/// <returns></returns>
		private bool CheckGuitarTerms(IReadOnlyCollection<GuitarTerm> terms, DisplayItem channel)
		{
			string body = channel.Title.ToLower() + " " + channel.Description.ToLower();

			foreach (var term in terms)
			{
				if (body.Contains(term.Id))
				{
					return true;
				}
			}

			return false;
		}
	}
}
