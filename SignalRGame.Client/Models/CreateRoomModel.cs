using System.ComponentModel.DataAnnotations;

namespace SignalRGame.Client.Models
{
	public class CreateRoomModel
	{
		[Required(ErrorMessage = "Player name is required.")]
		public string PlayerName { get; set; }

		[Required(ErrorMessage = "Room name is required.")]
		public string RoomName { get; set; }
	}

}
