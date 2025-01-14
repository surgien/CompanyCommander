using BlazorBootstrap;
using CompanyCommander.DB;
using CompanyCommander.Model;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GameService {
  private readonly AppDbContext db;

  public GameService(AppDbContext dbContext) {
    db = dbContext;
  }

  public async Task<(IncomeModel currentIncome, IncomeModel currentCount, Game currentGame, List<int> vps)>
    InitializeGameAsync(GameEdition edition, int targetVpPick = 15) {
    var currentCount = new IncomeModel();
    var currentIncome = new IncomeModel();
    List<int> vps;

    if (edition == GameEdition.SecondEditionNormal) {
      currentCount.Manpower = 4;
      currentCount.Ammo = 2;
      currentCount.Fuel = 3;
      vps = new List<int> { 18, 21, 24, 27 };
    }
    else {
      currentCount.Manpower = 4;
      currentCount.Ammo = 4;
      currentCount.Fuel = 4;
      vps = new List<int> { 13, 15, 18 };
    }

    currentIncome.Manpower = 1;
    currentIncome.Ammo = 1;
    currentIncome.Fuel = 1;
    currentIncome.VictoryPoints = 0;

    currentCount.VictoryPoints = 0;
    var currentRound = 1;
    var currentGame = new Game() {
      Edition = GameEdition.FirstEditionProWithErrata,
      Start = DateTime.Now,
      VictoryPoints = targetVpPick
    };
    await NewRoundAsync(currentIncome, currentCount, currentRound, currentGame);

    db.Game.Insert(currentGame);
    await db.SaveDatabaseAsync();

    return (currentIncome, currentCount, currentGame, vps);
  }

  public (IncomeModel currentIncome, IncomeModel currentCount, Game currentGame, int currentRound)
    LoadGame() {
    var currentRound = db.Income.Max(x => x.Round);
    var currentIncome = new IncomeModel {
      Manpower = db.Income.FindOne(x => x.Type == StockpileType.Manpower && x.Round == currentRound).Amount,
      Ammo = db.Income.FindOne(x => x.Type == StockpileType.Ammo && x.Round == currentRound).Amount,
      Fuel = db.Income.FindOne(x => x.Type == StockpileType.Fuel && x.Round == currentRound).Amount,
      VictoryPoints = db.Income.FindOne(x => x.Type == StockpileType.VictoryPoints && x.Round == currentRound).Amount
    };

    var currentCount = new IncomeModel {
      Manpower = db.Stockpile.FindOne(x => x.Type == StockpileType.Manpower && x.Round == currentRound).Amount,
      Ammo = db.Stockpile.FindOne(x => x.Type == StockpileType.Ammo && x.Round == currentRound).Amount,
      Fuel = db.Stockpile.FindOne(x => x.Type == StockpileType.Fuel && x.Round == currentRound).Amount,
      VictoryPoints = db.Stockpile.FindOne(x => x.Type == StockpileType.VictoryPoints && x.Round == currentRound).Amount
    };

    var currentGame = db.Game.FindAll().SingleOrDefault();
    return (currentIncome, currentCount, currentGame, currentRound);
  }

  private async Task NewRoundAsync(IncomeModel currentIncome, IncomeModel currentCount, int currentRound, Game currentGame) {
    db.Income.Insert(new Income { Amount = currentIncome.Manpower, Type = StockpileType.Manpower, Round = currentRound, Date = DateTime.Now });
    db.Income.Insert(new Income { Amount = currentIncome.Ammo, Type = StockpileType.Ammo, Round = currentRound, Date = DateTime.Now });
    db.Income.Insert(new Income { Amount = currentIncome.Fuel, Type = StockpileType.Fuel, Round = currentRound, Date = DateTime.Now });
    db.Income.Insert(new Income { Amount = currentIncome.VictoryPoints, Type = StockpileType.VictoryPoints, Round = currentRound, Date = DateTime.Now });

    db.Stockpile.Insert(new Stockpile { InitialAmount = currentCount.Manpower, Amount = currentCount.Manpower, Type = StockpileType.Manpower, Round = currentRound, Date = DateTime.Now });
    db.Stockpile.Insert(new Stockpile { InitialAmount = currentCount.Ammo, Amount = currentCount.Ammo, Type = StockpileType.Ammo, Round = currentRound, Date = DateTime.Now });
    db.Stockpile.Insert(new Stockpile { InitialAmount = currentCount.Fuel, Amount = currentCount.Fuel, Type = StockpileType.Fuel, Round = currentRound, Date = DateTime.Now });
    db.Stockpile.Insert(new Stockpile { InitialAmount = currentCount.VictoryPoints, Amount = currentCount.VictoryPoints, Type = StockpileType.VictoryPoints, Round = currentRound, Date = DateTime.Now });

    var backend = new CompanyCommander.Backend.BackendDataContext("https://solarsphereapi-gybwcpf8ade9chbj.germanywestcentral-01.azurewebsites.net/", new HttpClient());
    //var backend = new CompanyCommander.Backend.BackendDataContext("https://localhost:7027/", new HttpClient());

    await backend.CollectIncomeAsync(new CompanyCommander.Backend.Round() {
      ClientId = currentGame.Id,
      Start = currentGame.Start,
      RoundNr = currentRound,
      Income = new CompanyCommander.Backend.Income() {
        Manpower = currentIncome.Manpower,
        Ammo = currentIncome.Ammo,
        Fuel = currentIncome.Fuel,
        VictoryPoints = currentIncome.VictoryPoints
      },
      Stockpile = new CompanyCommander.Backend.Income() {
        Manpower = currentCount.Manpower,
        Ammo = currentCount.Ammo,
        Fuel = currentCount.Fuel,
        VictoryPoints = currentCount.VictoryPoints
      }
    });
  }

  public List<string> GetChangedIncome(int currentRound, IncomeModel currentIncome) {
    var incomeManpower = db.Income.FindOne(x => x.Type == StockpileType.Manpower && x.Round == currentRound).Amount;
    var incomeAmmo = db.Income.FindOne(x => x.Type == StockpileType.Ammo && x.Round == currentRound).Amount;
    var incomeFuel = db.Income.FindOne(x => x.Type == StockpileType.Fuel && x.Round == currentRound).Amount;
    var incomeVP = db.Income.FindOne(x => x.Type == StockpileType.VictoryPoints && x.Round == currentRound).Amount;
    var changed = new List<string>();

    if (incomeManpower != currentIncome.Manpower) {
      changed.Add("Manpower: " + WithPlus(currentIncome.Manpower - incomeManpower));
    }
    if (incomeAmmo != currentIncome.Ammo) {
      changed.Add("Ammo: " + WithPlus(currentIncome.Ammo - incomeAmmo));
    }
    if (incomeFuel != currentIncome.Fuel) {
      changed.Add("Fuel: " + WithPlus(currentIncome.Fuel - incomeFuel));
    }
    if (incomeVP != currentIncome.VictoryPoints) {
      changed.Add("VPs: " + WithPlus(currentIncome.VictoryPoints - incomeVP));
    }

    return changed;
  }
  private string WithPlus(int val) {
    if (val > 0) return "+" + val;
    else if (val < 0) return val.ToString();
    else return "0";
  }
  public List<int> GetInitVps(GameEdition edition) {
    if (edition == GameEdition.SecondEditionNormal) {
      return new List<int>
  { 18, 21, 24, 27 };
    }
    else {
      return new List<int>
      { 13, 15, 18 };
    }
  }

  public async Task ProcessNextRoundAsync(Game currentGame, IncomeModel currentIncome, IncomeModel currentCount, int currentRound, Modal modal) {
    currentRound++;
    currentCount.Manpower += currentIncome.Manpower;
    currentCount.Ammo += currentIncome.Ammo;
    currentCount.Fuel += currentIncome.Fuel;
    currentCount.VictoryPoints += currentIncome.VictoryPoints;

    if (currentGame != null && currentCount.VictoryPoints >= currentGame.VictoryPoints) {
      await modal.ShowAsync();
    }
    await NewRoundAsync(currentIncome, currentCount, currentRound, currentGame);
    await db.SaveDatabaseAsync();
  }

  public async Task UpdateStockpileAsync(StockpileType type, int currentRound, int amount) {
    var stock = db.Stockpile.FindOne(x => x.Type == type && x.Round == currentRound);
    stock.Amount = amount;
    db.Stockpile.Update(stock);
    await db.SaveDatabaseAsync();
  }

  public async Task UpdateIncomeAsync(StockpileType type, int currentRound, int amount) {
    var income = db.Income.FindOne(x => x.Type == type && x.Round == currentRound);
    income.Amount = amount;
    db.Income.Update(income);
    await db.SaveDatabaseAsync();
  }

  public async Task ClearDatabaseAsync() {
    await db.ClearLocalStorageAsync();
    await db.LoadDatabaseAsync();
  }
}
