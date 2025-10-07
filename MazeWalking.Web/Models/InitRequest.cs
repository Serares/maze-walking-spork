namespace MazeWalking.Web.Models
{
    public record InitRequest(string PlayerName, int RowsColumns, string? PlayerId = null);
}
