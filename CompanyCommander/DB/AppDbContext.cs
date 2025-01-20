//using Microsoft.EntityFrameworkCore;

using System.ComponentModel;
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

  public enum Fraction {
    Wehr,
    OKW,
    US,
    Britten,
    Russen
  }

  public enum GameEdition {
    [Description("1. Edition - Normal")]
    FirstEditionNormal,
    [Description("1. Edition - Erweitert")]
    FirstEditionPro,
    [Description("1. Edition - Erweitert mit Errata")]
    FirstEditionProWithErrata,
    [Description("2. Edition - Normal")]
    SecondEditionNormal
  }

  public class Game {

    [BsonId]
    public Guid Id { get; set; }
    public GameEdition Edition { get; set; }
    public DateTime Start { get; set; }
    public int VictoryPoints { get; set; }
    public int SavedRound { get; set; }
    public Fraction Fraction { get; set; }
    public string PalyerName{ get; set; }
  }

  public class Stockpile {
    [BsonId]
    public Guid Id { get; set; }
    public int Round { get; set; }
    public int Amount { get; set; }
    public StockpileType Type { get; set; }
    public DateTime Date { get; set; }
    public int InitialAmount { get; set; }
  }

  public class Income {
    public int Round { get; set; }
    public int Amount { get; set; }
    public StockpileType Type { get; set; }
    [BsonId]
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
  }

  public class Fuk {
    [BsonId]
    public Guid Id { get; set; }
    public int Round { get; set; }
    public DateTime Date { get; set; }
  }

  public class AppDbContext {

    public ILiteCollection<Stockpile> Stockpile => Database.GetCollection<Stockpile>();
    public ILiteCollection<Income> Income => Database.GetCollection<Income>();
    public ILiteCollection<Fuk> Fuks => Database.GetCollection<Fuk>();
    public ILiteCollection<Game> Game => Database.GetCollection<Game>();



    public LiteDatabase Database { get; set; }

    public AppDbContext(ILocalStorageService localStorage) {

      _localStorage = localStorage;
    }

    private readonly ILocalStorageService _localStorage;
    public MemoryStream MemoryStream { get; set; }

    public async Task ClearLocalStorageAsync() {
      await _localStorage.ClearAsync();
    }

    public async Task LoadDatabaseAsync() {
      await EnumExtensions.SaveSemaphore.WaitAsync();
      try {
        if (Database == null) {
          var dbContent = await _localStorage.GetItemAsync<string>(nameof(AppDbContext));

          MemoryStream = new MemoryStream();

          if (dbContent != null) {
            var bytes = Convert.FromBase64String(dbContent);
            MemoryStream = new MemoryStream(bytes);
          }

          Database = new LiteDatabase(MemoryStream);
        }
      }
      finally {
        EnumExtensions.SaveSemaphore.Release();
      }
    }

    public async Task SaveDatabaseAsync() {
      await EnumExtensions.SaveSemaphore.WaitAsync();
      try {
        //Database.chec
        Database.Commit();

        //Hie darf parallel kein Schreibzeugfs merh passieen
        Database.Checkpoint(); //TODO: Exception



        MemoryStream.Position = 0;

        var jsonString = Convert.ToBase64String(MemoryStream.ToArray());
        await _localStorage.SetItemAsync(nameof(AppDbContext), jsonString);
      }
      finally {
        EnumExtensions.SaveSemaphore.Release();
      }
    }
  }

  //public class AppDbContext : DbContext {
  //  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  //  public DbSet<Income> Income { get; set; }
  //  public DbSet<Stockpile> Stockpile { get; set; }
  //}
}
