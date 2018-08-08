namespace Raidfelden.Entities
{
	public interface IFortSighting
	{
		int Id { get; }
		byte Team { get; set; }
		int? FortId { get; set; }
		int? LastModified { get; set; }
		int? Updated { get; set; }
	}
}
