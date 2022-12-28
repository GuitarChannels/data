using GraphQL.Types;
using Infrastructure.API.Models;

namespace Presentation.API.GraphQL.Types
{
	public sealed class ChannelIdentificationType
		: ObjectGraphType<ChannelIdentificationModel>, IGraphQLType
	{
		public ChannelIdentificationType()
		{
			Name = "ChannelIdentification";

			Field(i => i.ChannelID, true);

			Field<DisplayItemType>("Channel");

			Field(i => i.IsGuitarChannel);
			Field(i => i.Source, true);
			Field(i => i.Status);
		}
	}
}
