﻿using System;
using System.Collections.Generic;
using AutoMapper;
using Core.DTO.UseCaseRequests;
using Core.Interfaces.Repositories;
using Core.Interfaces.UseCases;
using GraphQL.Types;
using Infrastructure.API.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Extensions;
using Presentation.API.GraphQL.Types;

namespace Presentation.API.GraphQL.Resolver
{
    public class SuggestionResolver : IResolver, ISuggestionResolver
    {
        private readonly IChannelSuggestionsUseCase _channelSuggestionsUseCase;
        private readonly ISuggestionRepository _suggestionRepository;
        private readonly ILogger<SuggestionResolver> _logger;
        private readonly IMapper _mapper;

        public SuggestionResolver(
            ILogger<SuggestionResolver> logger,
            IChannelSuggestionsUseCase channelSuggestionsUseCase,
            ISuggestionRepository suggestionRepository,
            IMapper mapper
        )
        {
            _suggestionRepository = suggestionRepository ?? throw new ArgumentNullException("suggestionRepository");
            _channelSuggestionsUseCase = channelSuggestionsUseCase ?? throw new ArgumentNullException("channelSuggestionsUseCase");
            _logger = logger;
            _mapper = mapper ?? throw new ArgumentNullException("mapper");
        }

        public void ResolveQuery(GraphQLQuery graphQLQuery)
        {
            // IDENTIFY CHANNELS: take a list of url hints and identify sailing channels from them
            graphQLQuery.FieldAsync<ListGraphType<ChannelIdentificationType>>(
                "channelSuggestions",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<ListGraphType<StringGraphType>>> { Name = "channelIds" },
                    new QueryArgument<StringGraphType> { Name = "source" }
                ),
                resolve: async (context) =>
                {
                    // read user context dictionary
                    var userContext = (GraphQLUserContext)context.UserContext;
                    string userId = userContext.GetUserId();

                    // require user to be authenticated
                    if (userId == null) return null;

                    var result = await _channelSuggestionsUseCase.Handle(
                        new ChannelSuggestionsRequest(
                            context.GetArgument<List<string>>("channelIds"),
                            userId,
                            context.GetArgument<string>("source")
                        )
                    );

                    // truncate description
                    foreach (var channel in result.Suggestions)
                    {
                        channel.Channel.Description = channel.Channel.Description.Truncate(300, ellipsis: true);
                    }

                    // map entity to model
                    return _mapper.Map<List<ChannelIdentificationModel>>(result.Suggestions);
                }
            );
        }

        public void ResolveMutation(GraphQLMutation graphQLMutation)
        {
            // SUGGEST CHANNEL
            graphQLMutation.FieldAsync<BooleanGraphType>(
                "suggestChannel",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "channelId" }
                ),
                resolve: async (context) =>
                {
                    try
                    {
                        // read user context dictionary
                        var userContext = (GraphQLUserContext) context.UserContext;
                        string userId = userContext.GetUserId();

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
    }
}
