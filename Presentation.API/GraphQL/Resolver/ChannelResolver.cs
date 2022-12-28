using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Core.DTO.UseCaseRequests;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.UseCases;
using GraphQL.Types;
using Infrastructure.API.Models;
using Microsoft.Toolkit.Extensions;
using Presentation.API.GraphQL.Types;

namespace Presentation.API.GraphQL.Resolver
{
	public sealed class ChannelResolver
		: BaseResolver, IResolver, IChannelResolver
	{
		private readonly IChannelRepository _channelRepository;
		private readonly IIdentifyGuitarChannelUseCase _identifyGuitarChannelUseCase;
		private readonly IChannelPublishPredictionRepository _channelPublishPredictionRepository;
		private readonly IMapper _mapper;

		public ChannelResolver(
			IChannelRepository channelRepository,
			IIdentifyGuitarChannelUseCase identifyGuitarChannelUseCase,
			IMapper mapper,
			IChannelPublishPredictionRepository channelPublishPredictionRepository)
		{
			_channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
			_identifyGuitarChannelUseCase = identifyGuitarChannelUseCase ?? throw new ArgumentNullException(nameof(identifyGuitarChannelUseCase));
			_mapper = mapper ?? throw new ArgumentNullException("mapper");
			_channelPublishPredictionRepository = channelPublishPredictionRepository ?? throw new ArgumentNullException(nameof(channelPublishPredictionRepository));
		}

		/// <summary>
		/// Resolves all queries on guest accesses
		/// </summary>
		/// <param name="graphQlQuery"></param>
		public void ResolveQuery(GraphQlQuery graphQlQuery)
		{
			// GUEST ACCESSES: list of all guest access entries
			graphQlQuery.FieldAsync<ListGraphType<ChannelType>>(
				"channels",
				arguments: new QueryArguments(
					new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "sortBy" },
					new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "skip" },
					new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "take" },
					new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "language" }
				),
				resolve: async context =>
				{
					var channels = await _channelRepository.GetAll(
						(ChannelSorting)Enum.Parse(typeof(ChannelSorting), context.GetArgument<string>("sortBy")),
						context.GetArgument<int>("skip"),
						context.GetArgument<int>("take"),
						context.GetArgument<string>("language")
					);

					var channelsWithTruncatedDescription =
						TruncateChannelDescriptions(channels);

					// map entity to model
					return _mapper.Map<List<ChannelModel>>(channelsWithTruncatedDescription);
				}
			);

			// CHANNEL: retrieve single channel
			graphQlQuery.FieldAsync<ChannelType>(
				"channel",
				arguments: new QueryArguments(
					new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "id" }
				),
				resolve: async context =>
				{
					var channel = await _channelRepository.Get(context.GetArgument<string>("id"));

					// map entity to model
					return _mapper.Map<ChannelModel>(channel);
				}
			);

			// CHANNEL COUNT TOTAL
			graphQlQuery.FieldAsync<IntGraphType>(
				"channelCountTotal",
				resolve: async context =>
				{
					var channelCount = await _channelRepository.Count();

					// map entity to model
					return channelCount;
				}
			);

			// IDENTIFY CHANNELS: take a list of url hints and identify guitar channels from them
			graphQlQuery.FieldAsync<ChannelIdentificationType>(
				"identifyChannel",
				arguments: new QueryArguments(
					new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "channelUrlHint" }
				),
				resolve: async context =>
				{
					var result = await _identifyGuitarChannelUseCase.Handle(
						new IdentifyGuitarChannelRequest(
							context.GetArgument<string>("channelUrlHint")
						)
					);

					var identifiedChannel = result.IdentifiedChannel;

					// truncate description
					if (result.IdentifiedChannel != null && result.IdentifiedChannel.Channel != null)
					{
						identifiedChannel = identifiedChannel with
						{
							Channel = identifiedChannel.Channel with
							{
								Description = identifiedChannel.Channel.Description.Truncate(300, true)
							}
						};
					}

					// map entity to model
					return _mapper.Map<ChannelIdentificationModel>(identifiedChannel);
				}
			);

			// CHANNEL PUBLISH PREDICTION
			graphQlQuery.FieldAsync<ListGraphType<PublishSchedulePredictionType>>(
				"channelPublishPrediction",
				arguments: new QueryArguments(
					new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "channelId" },
					new QueryArgument<BooleanGraphType> { Name = "filterBelowAverage", DefaultValue = false },
					new QueryArgument<FloatGraphType> { Name = "minGradient", DefaultValue = 0.7f }
				),
				resolve: async context =>
				{
					var channelId = context.GetArgument<string>("channelId");
					var filterBelowAverage = context.GetArgument<bool>("filterBelowAverage");
					var minGradient = context.GetArgument<double>("minGradient");

					var predictionResult = await _channelPublishPredictionRepository.Get(channelId);

					if (predictionResult is null || predictionResult.Gradient <= minGradient)
					{
						return null;
					}

					var predictionItems = predictionResult.PredictionItems;

					if (filterBelowAverage)
					{
						predictionItems = predictionItems.Where(i => i.DeviationFromAverage > 0);
					}

					return _mapper.Map<List<PublishSchedulePredictionModel>>(predictionItems);
				});

			// CHANNEL PUBLISH PREDICTIONS
			graphQlQuery.FieldAsync<ListGraphType<PublishPredictionProgrammingItemType>>(
				"channelPublishPredictions",
				arguments: new QueryArguments(
					new QueryArgument<FloatGraphType> { Name = "minGradient", DefaultValue = 0.7f }
				),
				resolve: async context =>
				{
					var minGradient = context.GetArgument<double>("minGradient");

					var predictionResultsPerChannel =
						await _channelPublishPredictionRepository.Get();

					predictionResultsPerChannel = predictionResultsPerChannel
						.Where(p => p.Gradient >= minGradient)
						.ToList();

					var programmingLookup = new Dictionary<Weekstamp, List<Channel>>();

					foreach (var publishPrediction in predictionResultsPerChannel)
					{
						var topPredictionResult = publishPrediction.PredictionItems.FirstOrDefault();
						if (topPredictionResult is null) continue;

						var predictionWeekstamp = (Weekstamp)topPredictionResult;

						if (!programmingLookup.ContainsKey(predictionWeekstamp))
						{
							programmingLookup.Add(predictionWeekstamp, new List<Channel>());
						}

						programmingLookup[predictionWeekstamp].Add(new Channel
						{
							Id = publishPrediction.Id,
							Title = publishPrediction.Title
						});
					}

					return programmingLookup
						.Select(programmingLookupItem =>
							new PublishPredictionProgrammingItemModel(
								programmingLookupItem.Key.DayOfTheWeek,
								programmingLookupItem.Key.HourOfTheDay,
								_mapper.Map<List<ChannelModel>>(programmingLookupItem.Value)))
						.ToList();
				});
		}

		/// <summary>
		/// Resolves all mutations on guest accesses.
		/// </summary>
		/// <param name="graphQlMutation"></param>
		public void ResolveMutation(GraphQlMutation graphQlMutation)
		{
		}
	}
}
