﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DTO;
using Core.DTO.UseCaseRequests;
using Core.DTO.UseCaseResponses;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.UseCases;

namespace Core.UseCases
{
    public class IdentifySailingChannelUseCase : IIdentifySailingChannelUseCase
    {
        private readonly IExtractYouTubeChannelIDUseCase _extractYouTubeChannelIdUseCase;
        private readonly IYouTubeChannelDetailUseCase _youTubeChannelDetailUseCase;
        private readonly ISuggestionRepository _suggestionRepository;

        public IdentifySailingChannelUseCase(
            IExtractYouTubeChannelIDUseCase extractYouTubeChannelIdUseCase,
            IYouTubeChannelDetailUseCase youTubeChannelDetailUseCase,
            ISuggestionRepository suggestionRepository
        )
        {
            _extractYouTubeChannelIdUseCase = extractYouTubeChannelIdUseCase ?? throw new ArgumentNullException(nameof(extractYouTubeChannelIdUseCase));
            _youTubeChannelDetailUseCase = youTubeChannelDetailUseCase ?? throw new ArgumentNullException(nameof(youTubeChannelDetailUseCase));
            _suggestionRepository = suggestionRepository ?? throw new ArgumentNullException(nameof(suggestionRepository));
        }

        public async Task<IdentifySailingChannelResponse> Handle(IdentifySailingChannelRequest message)
        {
            // guard clause
            if (string.IsNullOrWhiteSpace(message.PossibleYouTubeChannelUrl))
            {
                return new IdentifySailingChannelResponse(
                    ChannelIdentificationStatus.NotValid
                );
            }

            // try to identify a channel id from the URL
            var channelIdResponse = await _extractYouTubeChannelIdUseCase.Handle(
                new ExtractYouTubeChannelIdRequest(message.PossibleYouTubeChannelUrl)
            );

            // could not identity a channel
            if (string.IsNullOrWhiteSpace(channelIdResponse.ChannelId))
            {
                return new IdentifySailingChannelResponse(
                    ChannelIdentificationStatus.NotValid
                );
            }

            // fetch details for youtube channel
            var channelDetails = await _youTubeChannelDetailUseCase.Handle(
                new YouTubeChannelDetailRequest(
                    new List<string> { channelIdResponse.ChannelId }
                )
            );

            if (channelDetails.IdentifiedChannels?.Count == 0)
            {
                return new IdentifySailingChannelResponse(
                    ChannelIdentificationStatus.NotValid
                );
            }

            ChannelIdentificationDto channelDetail = channelDetails.IdentifiedChannels.First();

            // we discovered a newly listed channel
            if (channelDetail.Source.ToLower() == "yt")
            {
                // check if channel has been suggested before
                var suggestions = await _suggestionRepository.GetAny(
                    new List<string> { channelIdResponse.ChannelId }
                );

                // a suggestion does already exist for this channel
                if (suggestions.Count > 0)
                {
                    return new IdentifySailingChannelResponse(
                        ChannelIdentificationStatus.AlreadySuggested
                    );
                }

                // we found a novel channel at this point!
                return new IdentifySailingChannelResponse(
                    ChannelIdentificationStatus.Novel,
                    channelDetail
                );
            }

            // this channel must already be listed
            return new IdentifySailingChannelResponse(
                ChannelIdentificationStatus.AlreadyListed,
                channelDetail
            );
        }
    }
}
