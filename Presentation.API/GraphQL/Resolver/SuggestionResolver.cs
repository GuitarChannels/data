using System;
using System.Collections.Generic;
using AutoMapper;
using Core.DTO;
using Core.DTO.UseCaseRequests;
using Core.Interfaces.Repositories;
using Core.Interfaces.UseCases;
using GraphQL.Types;
using Infrastructure.API.Models;
using Microsoft.Toolkit.Extensions;
using Presentation.API.GraphQL.Types;

namespace Presentation.API.GraphQL.Resolver
{
	public class SuggestionResolver : IResolver, ISuggestionResolver
	{
		private readonly IChannelSuggestionsUseCase _channelSuggestionsUseCase;
		private readonly ISuggestionRepository _suggestionRepository;
		private readonly IMapper _mapper;

		public SuggestionResolver(
			IChannelSuggestionsUseCase channelSuggestionsUseCase,
			ISuggestionRepository suggestionRepository,
			IMapper mapper
		)
		{
			_suggestionRepository = suggestionRepository ?? throw new ArgumentNullException("suggestionRepository");
			_channelSuggestionsUseCase = channelSuggestionsUseCase ?? throw new ArgumentNullException("channelSuggestionsUseCase");
			_mapper = mapper ?? throw new ArgumentNullException("mapper");
		}

		public void ResolveQuery(GraphQlQuery graphQlQuery)
		{
			// IDENTIFY CHANNELS: take a list of url hints and identify guitar channels from them
			graphQlQuery.FieldAsync<ListGraphType<ChannelIdentificationType>>(
				"channelSuggestions",
				arguments: new QueryArguments(
					new QueryArgument<NonNullGraphType<ListGraphType<StringGraphType>>> { Name = "channelIds" },
					new QueryArgument<StringGraphType> { Name = "source" }
				),
				resolve: async context =>
				{
					// read user context dictionary
					var userContext = (GraphQlUserContext)context.UserContext;
					var userId = userContext.GetUserId();

					// require user to be authenticated
					if (userId == null) return null;

					var result = await _channelSuggestionsUseCase.Handle(
						new ChannelSuggestionsRequest(
							context.GetArgument<List<string>>("channelIds"),
							userId,
							context.GetArgument<string>("source")
						)
					);

					var truncatedSuggestions =
						TruncateChannelIdentificationDescription(result.Suggestions);

					// map entity to model
					return _mapper.Map<List<ChannelIdentificationModel>>(truncatedSuggestions);
				}
			);
		}

		public void ResolveMutation(GraphQlMutation graphQlMutation)
		{
			// SUGGEST CHANNEL
			graphQlMutation.FieldAsync<BooleanGraphType>(
				"suggestChannel",
				arguments: new QueryArguments(
					new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "channelId" }
				),
				resolve: async context =>
				{
					try
					{
						// read user context dictionary
						var userContext = (GraphQlUserContext)context.UserContext;
						var userId = userContext.GetUserId();

						// require user to be authenticated
						if (userId == null) return false;

						await _suggestionRepository.AddSuggestion(
							context.GetArgument<string>("channelId"),
							userId
						);

						return true;
					}
					catch
					{
						return false;
					}
				});
		}

		private IReadOnlyCollection<ChannelIdentificationDto> TruncateChannelIdentificationDescription(
			IEnumerable<ChannelIdentificationDto> suggestions)
		{
			var truncatedSuggestions = new List<ChannelIdentificationDto>();

			foreach (var suggestion in suggestions)
			{
				var channelIdentification = suggestion with
				{
					Channel = suggestion.Channel with
					{
						Description = suggestion.Channel.Description.Truncate(300, true)
					}
				};

				truncatedSuggestions.Add(channelIdentification);
			}

			return truncatedSuggestions;
		}
	}
}
