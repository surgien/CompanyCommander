namespace CompanyCommander.Model {
  public class Customer {
    public Customer(int id, string name) {
      CustomerName = name;
    }
    public string CustomerName { get; set; }
  }
}
