using BlazorBootstrap;
using CompanyCommander.DB;
using CompanyCommander.Model;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GameService {
  private readonly AppDbContext _db;

  public GameService(AppDbContext dbContext) {
    _db = dbContext;
  }

  public async Task BuyOneAsync(StockpileType type, IncomeModel currentCount, int currentRound) {
    var stock = _db.Stockpile.FindOne(x => x.Type == type && x.Round == currentRound);

    switch (type) {
      case StockpileType.Manpower:
        if (currentCount.Manpower > 0)
          currentCount.Manpower--;
        break;
      case StockpileType.Ammo:
        if (currentCount.Ammo > 0)
          currentCount.Ammo--;
        break;
      case StockpileType.Fuel:
        if (currentCount.Fuel > 0)
          currentCount.Fuel--;
        break;
    }
    if (stock.Amount > 0) {
      stock.Amount--;
      _db.Stockpile.Update(stock);
      await _db.SaveDatabaseAsync();
    }
  }

  public async Task UndoOneAsync(StockpileType type, IncomeModel currentCount, int currentRound) {
    var stock = _db.Stockpile.FindOne(x => x.Type == type && x.Round == currentRound);

    switch (type) {
      case StockpileType.Manpower:
        if (currentCount.Manpower < stock.InitialAmount)
          currentCount.Manpower++;
        break;
      case StockpileType.Ammo:
        if (currentCount.Ammo < stock.InitialAmount)
          currentCount.Ammo++;
        break;
      case StockpileType.Fuel:
        if (currentCount.Fuel < stock.InitialAmount)
          currentCount.Fuel++;
        break;
    }

    if (stock.Amount < stock.InitialAmount) {
      stock.Amount++;
      _db.Stockpile.Update(stock);
      await _db.SaveDatabaseAsync();
    }
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

    _db.Game.DeleteAll();
    _db.Game.Insert(currentGame);
    await _db.SaveDatabaseAsync();

    return (currentIncome, currentCount, currentGame, vps);
  }

  public async Task DeleteAllAsync() {
    _db.Income.DeleteAll();
    _db.Stockpile.DeleteAll();
    _db.Fuks.DeleteAll();
    _db.Game.DeleteAll();
    await _db.SaveDatabaseAsync();
  }

  public (IncomeModel currentIncome, IncomeModel currentCount, Game currentGame, int currentRound)
    LoadGame() {
    var currentRound = _db.Income.Max(x => x.Round);
    var currentIncome = new IncomeModel {
      Manpower = _db.Income.FindOne(x => x.Type == StockpileType.Manpower && x.Round == currentRound).Amount,
      Ammo = _db.Income.FindOne(x => x.Type == StockpileType.Ammo && x.Round == currentRound).Amount,
      Fuel = _db.Income.FindOne(x => x.Type == StockpileType.Fuel && x.Round == currentRound).Amount,
      VictoryPoints = _db.Income.FindOne(x => x.Type == StockpileType.VictoryPoints && x.Round == currentRound).Amount
    };

    var currentCount = new IncomeModel {
      Manpower = _db.Stockpile.FindOne(x => x.Type == StockpileType.Manpower && x.Round == currentRound).Amount,
      Ammo = _db.Stockpile.FindOne(x => x.Type == StockpileType.Ammo && x.Round == currentRound).Amount,
      Fuel = _db.Stockpile.FindOne(x => x.Type == StockpileType.Fuel && x.Round == currentRound).Amount,
      VictoryPoints = _db.Stockpile.FindOne(x => x.Type == StockpileType.VictoryPoints && x.Round == currentRound).Amount
    };

    var games = _db.Game.FindAll().ToList();

    var currentGame = games.SingleOrDefault();
    return (currentIncome, currentCount, currentGame, currentRound);
  }

  private async Task NewRoundAsync(IncomeModel currentIncome, IncomeModel currentCount, int currentRound, Game currentGame) {
    _db.Income.Insert(new Income { Amount = currentIncome.Manpower, Type = StockpileType.Manpower, Round = currentRound, Date = DateTime.Now });
    _db.Income.Insert(new Income { Amount = currentIncome.Ammo, Type = StockpileType.Ammo, Round = currentRound, Date = DateTime.Now });
    _db.Income.Insert(new Income { Amount = currentIncome.Fuel, Type = StockpileType.Fuel, Round = currentRound, Date = DateTime.Now });
    _db.Income.Insert(new Income { Amount = currentIncome.VictoryPoints, Type = StockpileType.VictoryPoints, Round = currentRound, Date = DateTime.Now });

    _db.Stockpile.Insert(new Stockpile { InitialAmount = currentCount.Manpower, Amount = currentCount.Manpower, Type = StockpileType.Manpower, Round = currentRound, Date = DateTime.Now });
    _db.Stockpile.Insert(new Stockpile { InitialAmount = currentCount.Ammo, Amount = currentCount.Ammo, Type = StockpileType.Ammo, Round = currentRound, Date = DateTime.Now });
    _db.Stockpile.Insert(new Stockpile { InitialAmount = currentCount.Fuel, Amount = currentCount.Fuel, Type = StockpileType.Fuel, Round = currentRound, Date = DateTime.Now });
    _db.Stockpile.Insert(new Stockpile { InitialAmount = currentCount.VictoryPoints, Amount = currentCount.VictoryPoints, Type = StockpileType.VictoryPoints, Round = currentRound, Date = DateTime.Now });

    //currentGame.SavedRound = currentRound;
    //_db.Game.Update(currentGame);

    //await SaveBackendAsync(currentIncome, currentCount, currentRound, currentGame);
  }

  public async Task SaveBackendAsync(int currentRound, Game currentGame) {

    for (int i = currentGame.SavedRound; i < currentRound; i++) {
      var inc = _db.Income.Find(x => x.Round == currentRound);
      var stock = _db.Stockpile.Find(x => x.Round == currentRound);
      var currentIncome = new IncomeModel();
      var currentCount = new IncomeModel();

      foreach (var item in inc) {
        switch (item.Type) {
          case StockpileType.Manpower:
            currentIncome.Manpower = item.Amount;
            break;
          case StockpileType.Ammo:
            currentIncome.Ammo = item.Amount;
            break;
          case StockpileType.Fuel:
            currentIncome.Fuel = item.Amount;
            break;
          case StockpileType.VictoryPoints:
            currentIncome.VictoryPoints = item.Amount;
            break;
        }
      }

      foreach (var item in stock) {
        switch (item.Type) {
          case StockpileType.Manpower:
            currentCount.Manpower = item.Amount;
            break;
          case StockpileType.Ammo:
            currentCount.Ammo = item.Amount;
            break;
          case StockpileType.Fuel:
            currentCount.Fuel = item.Amount;
            break;
          case StockpileType.VictoryPoints:
            currentCount.VictoryPoints = item.Amount;
            break;
        }
      }

      await SaveBackendAsync(currentIncome, currentCount, i, currentGame);
      currentGame.SavedRound = currentRound;
    }
    _db.Game.Update(currentGame);
    await _db.SaveDatabaseAsync();
  }

  private async Task SaveBackendAsync(IncomeModel currentIncome, IncomeModel currentCount, int currentRound, Game currentGame) {
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
    var incomeManpower = _db.Income.FindOne(x => x.Type == StockpileType.Manpower && x.Round == currentRound).Amount;
    var incomeAmmo = _db.Income.FindOne(x => x.Type == StockpileType.Ammo && x.Round == currentRound).Amount;
    var incomeFuel = _db.Income.FindOne(x => x.Type == StockpileType.Fuel && x.Round == currentRound).Amount;
    var incomeVP = _db.Income.FindOne(x => x.Type == StockpileType.VictoryPoints && x.Round == currentRound).Amount;
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
      await SaveBackendAsync(currentRound, currentGame);
    }
    await NewRoundAsync(currentIncome, currentCount, currentRound, currentGame);
    await _db.SaveDatabaseAsync();
  }

  public async Task GainVictoryPointAsync(int currentRound, IncomeModel currentCount) {
    var stock = _db.Stockpile.FindOne(x => x.Type == StockpileType.VictoryPoints && x.Round == currentRound);

    stock.Amount++;
    currentCount.VictoryPoints++;

    _db.Stockpile.Update(stock);
    _db.Fuks.Insert(new Fuk() { Date = DateTime.Now, Round = currentRound });
    await _db.SaveDatabaseAsync();
  }

  public async Task ClearDatabaseAsync() {
    await _db.ClearLocalStorageAsync();
    await _db.LoadDatabaseAsync();
  }

  public async Task<(List<GameEdition> editions, bool isRunning)> LoadDatabaseAsync() {
    await _db.LoadDatabaseAsync();

    return (((GameEdition[])Enum.GetValues(typeof(GameEdition))).ToList(), _db.Game.Count() == 0);
  }

  public void IncrementCount(StockpileType type, IncomeModel currentIncome) {
    switch (type) {
      case StockpileType.Manpower:
        currentIncome.Manpower++;
        break;
      case StockpileType.Ammo:
        currentIncome.Ammo++;
        break;
      case StockpileType.Fuel:
        currentIncome.Fuel++;
        break;
      case StockpileType.VictoryPoints:
        currentIncome.VictoryPoints++;
        break;
    }
  }

  public void DecrementCount(StockpileType type, IncomeModel currentIncome) {
    switch (type) {
      case StockpileType.Manpower:
        if (currentIncome.Manpower > 0)
          currentIncome.Manpower--;
        break;
      case StockpileType.Ammo:
        if (currentIncome.Ammo > 0)
          currentIncome.Ammo--;
        break;
      case StockpileType.Fuel:
        if (currentIncome.Fuel > 0)
          currentIncome.Fuel--;
        break;
      case StockpileType.VictoryPoints:
        if (currentIncome.VictoryPoints > 0)
          currentIncome.VictoryPoints--;
        break;
    }
  }

}
