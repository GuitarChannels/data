using Core.Enums;

namespace Core.DTO.UseCaseResponses
{
	public record IdentifyGuitarChannelResponse
	{
		public ChannelIdentificationStatus Status { get; init; }
		public ChannelIdentificationDto IdentifiedChannel { get; init; }

		public IdentifyGuitarChannelResponse(
			ChannelIdentificationStatus status
		)
		{
			Status = status;
			IdentifiedChannel = new ChannelIdentificationDto() with { Status = status };
		}

		public IdentifyGuitarChannelResponse(
			ChannelIdentificationStatus status,
			ChannelIdentificationDto identifiedChannel
		)
		{
			Status = status;
			IdentifiedChannel = identifiedChannel with { Status = status };
		}
	}
}
