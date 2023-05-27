namespace Capstone.ResultsAndResponses.SortParams
{
	public enum SortDirection { Ascending = 1, Descending = -1 }

	public class SortParams
	{
		public string? SortByField { get; set; }
		public SortDirection? SortDirection { get; set; }
	}
}
