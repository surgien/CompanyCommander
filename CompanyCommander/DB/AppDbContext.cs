//using Microsoft.EntityFrameworkCore;

using System.Runtime.CompilerServices;
using Blazored.LocalStorage;
using LiteDB;

namespace CompanyCommander.DB {

  //HÄÄÄÄÄÄÄ????
  //public enum StockpileType {
  //  Food,
  //  Ammo,
  //  Fuel
  //}

  public enum StockpileType {
    Manpower,
    Ammo,
    Fuel,
    VictoryPoints //er auch ...
  }

  public class Stockpile {
    public int Round { get; set; }
    public int Amount { get; set; }
    public StockpileType Type { get; set; }
    [BsonId]
    public ObjectId Id { get; set; }
  }

  public class Income {
    public int Round { get; set; }
    public int Amount { get; set; }
    public StockpileType Type { get; set; }
    [BsonId]
    public ObjectId Id { get; set; }
  }

  public class AppDbContext {

    public ILiteCollection<Stockpile> Stockpile => Database.GetCollection<Stockpile>();
    public ILiteCollection<Income> Income => Database.GetCollection<Income>();



    public LiteDatabase Database { get; set; }

    public AppDbContext(ILocalStorageService localStorage) {

      _localStorage = localStorage;
    }

    private readonly ILocalStorageService _localStorage;
    private MemoryStream _memoryStream;

    public async Task LoadDatabaseAsync() {
      var dbContent = await _localStorage.GetItemAsync<string>(nameof(AppDbContext));

      _memoryStream = new MemoryStream();

      if (dbContent != null) {
        var bytes = Convert.FromBase64String(dbContent);
        _memoryStream = new MemoryStream(bytes);
      }

      Database = new LiteDatabase(_memoryStream);
    }

    public async Task SaveDatabaseAsync() {
      Database.Commit();
      Database.Checkpoint();

      _memoryStream.Position = 0;

      var jsonString = Convert.ToBase64String(_memoryStream.ToArray());
      await _localStorage.SetItemAsync(nameof(AppDbContext), jsonString);
    }
  }

  //public class AppDbContext : DbContext {
  //  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  //  public DbSet<Income> Income { get; set; }
  //  public DbSet<Stockpile> Stockpile { get; set; }
  //}
}
