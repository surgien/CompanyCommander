using CompanyCommander.DB;

namespace CompanyCommander.Model {
  public class RoundInfoModel {
    public DateTime Date { get; set; }
    public int Round { get; set; }
    public int FukCount { get; set; }
    public List<Income>? Income { get; set; }
    public List<Stockpile>? Stockpile { get; set; }
  }
}
