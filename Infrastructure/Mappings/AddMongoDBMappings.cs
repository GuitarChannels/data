using Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Infrastructure.Mappings
{
	public static class MongoDBMappings
	{
		public static void AddMongoDBMappings(this IServiceCollection services)
		{
			// CHANNEL POPULARITY
			BsonClassMap.RegisterClassMap<ChannelPopularity>(cm =>
			{
				cm.MapMember(c => c.Total).SetElementName("total");
			});

			// CHANNEL CUSTOM LINKS
			BsonClassMap.RegisterClassMap<ChannelCustomLink>(cm =>
			{
				cm.MapMember(c => c.Icon).SetElementName("icon");
				cm.MapMember(c => c.Title).SetElementName("title");
				cm.MapMember(c => c.Url).SetElementName("url");
			});

			// DISPLAY ITEM
			BsonClassMap.RegisterClassMap<DisplayItem>(cm =>
			{
				cm.MapIdMember(c => c.Id).SetElementName("_id");
				cm.MapMember(c => c.Description).SetElementName("description");
				cm.MapMember(c => c.Title).SetElementName("title");
				cm.MapMember(c => c.Thumbnail).SetElementName("thumbnail");
			});

			// CHANNEL
			BsonClassMap.RegisterClassMap<Channel>(cm =>
			{
				cm.MapMember(c => c.Views).SetElementName("views");
				cm.MapMember(c => c.PublishedAt).SetElementName("publishedAt");
				cm.MapMember(c => c.LastCrawl).SetElementName("lastCrawl");
				cm.MapMember(c => c.Subscribers).SetElementName("subscribers");
				cm.MapMember(c => c.VideoCount).SetElementName("videoCount");
				cm.MapMember(c => c.Language).SetElementName("language");
				cm.MapMember(c => c.Country).SetElementName("country");
				cm.MapMember(c => c.DetectedLanguage).SetElementName("detectedLanguage");
				cm.MapMember(c => c.LastUploadAt).SetElementName("lastUploadAt");
				cm.MapMember(c => c.SubscribersHidden).SetElementName("subscribersHidden");
				cm.MapMember(c => c.Popularity).SetElementName("popularity");
				cm.MapMember(c => c.Keywords).SetElementName("keywords");
				cm.MapMember(c => c.CustomLinks).SetElementName("customLinks");
				cm.MapMember(c => c.UploadsPlaylistId).SetElementName("uploadsPlaylistId");
			});

			// SEARCH
			BsonClassMap.RegisterClassMap<Search>(cm =>
			{
				cm.MapIdMember(c => c.Id).SetIdGenerator(StringObjectIdGenerator.Instance);

				cm.MapMember(c => c.Query).SetElementName("q");
				cm.MapMember(c => c.Timestamp).SetElementName("time");
			});

			// VIDEO
			BsonClassMap.RegisterClassMap<Video>(cm =>
			{
				cm.MapMember(c => c.Views).SetElementName("views");
				cm.MapMember(c => c.Dislikes).SetElementName("dislikes");
				cm.MapMember(c => c.PublishedAt).SetElementName("publishedAt");
				cm.MapMember(c => c.Likes).SetElementName("likes");
				cm.MapMember(c => c.ChannelId).SetElementName("channel");
				cm.MapMember(c => c.Comments).SetElementName("comments");
				cm.MapMember(c => c.GeoChecked).SetElementName("geoChecked");
				cm.MapMember(c => c.Tags).SetElementName("tags");
			});

			// TAG ID
			BsonClassMap.RegisterClassMap<TagId>(cm =>
			{
				cm.MapMember(c => c.ChannelId).SetElementName("channel");
				cm.MapMember(c => c.Tag).SetElementName("tag");
			});

			// TAG
			BsonClassMap.RegisterClassMap<Tag>(cm =>
			{
				cm.MapIdField(c => c.Id).SetElementName("_id");
				cm.MapMember(c => c.Popularity).SetElementName("value");
			});

			// TOPIC
			BsonClassMap.RegisterClassMap<Topic>(cm =>
			{
				cm.MapIdField(c => c.Id).SetElementName("_id");
				cm.MapMember(c => c.Description).SetElementName("description");
				cm.MapMember(c => c.Language).SetElementName("language");
				cm.MapMember(c => c.Title).SetElementName("title");
				cm.MapMember(c => c.SearchTerms).SetElementName("searchterms");
			});

			// FLAG ID
			BsonClassMap.RegisterClassMap<FlagId>(cm =>
			{
				cm.MapMember(c => c.ChannelId).SetElementName("channel");
				cm.MapMember(c => c.UserId).SetElementName("user");
			});

			// FLAG
			BsonClassMap.RegisterClassMap<Flag>(cm =>
			{
				cm.MapIdField(c => c.Id).SetElementName("_id");
				cm.MapMember(c => c.When).SetElementName("when");
			});

			// SUGGESTION ID
			BsonClassMap.RegisterClassMap<SuggestionId>(cm =>
			{
				cm.MapMember(c => c.ChannelId).SetElementName("channel");
				cm.MapMember(c => c.UserId).SetElementName("user");
			});

			// SUGGESTION
			BsonClassMap.RegisterClassMap<Suggestion>(cm =>
			{
				cm.MapIdField(c => c.Id).SetElementName("_id");
				cm.MapMember(c => c.When).SetElementName("when");
			});

			// SAILING TERM
			BsonClassMap.RegisterClassMap<GuitarTerm>(cm =>
			{
				cm.MapIdField(c => c.Id).SetElementName("_id");
			});

			// SUBSCRIBER ID
			BsonClassMap.RegisterClassMap<SubscriberId>(cm =>
			{
				cm.MapMember(c => c.ChannelId).SetElementName("channel");
				cm.MapMember(c => c.Date).SetElementName("date");
			});

			// SUBSCRIBER
			BsonClassMap.RegisterClassMap<Subscriber>(cm =>
			{
				cm.MapIdField(c => c.Id).SetElementName("_id");
				cm.MapMember(c => c.Date).SetElementName("date");
				cm.MapMember(c => c.Year).SetElementName("year");
				cm.MapMember(c => c.Month).SetElementName("month");
				cm.MapMember(c => c.Day).SetElementName("day");
				cm.MapMember(c => c.Subscribers).SetElementName("subscribers");
			});
		}
	}
}
