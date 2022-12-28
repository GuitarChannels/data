using Core.Entities;
using Core.Enums;

namespace Core.DTO
{
	public record ChannelIdentificationDto
	{
		public string ChannelId { get; init; }
		public string Source { get; init; }
		public bool IsGuitarChannel { get; init; }
		public DisplayItem Channel { get; init; }
		public ChannelIdentificationStatus Status { get; init; }
	}
}
