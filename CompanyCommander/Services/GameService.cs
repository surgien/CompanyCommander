using BlazorBootstrap;
using CompanyCommander.Backend;
using CompanyCommander.DB;
using CompanyCommander.Model;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GameService {
  private readonly AppDbContext _db;

  private Dictionary<Faction, string[]> _factionGenerals = new Dictionary<Faction, string[]>() {
    {  Faction.GermanWehrmacht, new string[]
  {
    "Erwin Rommel",
    "Heinz Guderian",
    "Erich von Manstein",
    "Friedrich Paulus",
    "Albert Kesselring"
  } },
     {  Faction.OberkommandoWest, new string[]
  {

    "Wilhelm Keitel",
    "Alfred Jodl",
    "Gotthard Heinrici",
    "Walter Model",
    "Georg von Kuechler"
  } },
    { Faction.SovietUnion, new string[]
  {
    "Georgi Schukow",
    "Konstantin Rokossowski",
    "Iwan Konew",
    "Wassili Tschuikow",
    "Alexander Wassilewski"
  }},
    { Faction.British, new string[]
  {
    "Bernard L. Montgomery",
    "Miles Dempsey",
    "Alan Brooke",
    "Harold Alexander",
    "Claude Auchinleck"
  } },
    {  Faction.UnitedStates,  new string[]
  {
    "George S. Patton",
    "Douglas MacArthur",
    "Dwight D. Eisenhower",
    "Omar N. Bradley",
    "Maurice Rose"
  }  }
    };

  public GameService(AppDbContext dbContext) {
    _db = dbContext;
  }

  public async Task UpdateCurrentCountAsync(StockpileType type, IncomeModel currentCount, int currentRound, int amount) {

    var stock = _db.Stockpile.FindOne(x => x.Type == type && x.Round == currentRound);

    if (amount > 0) {
      stock.Amount = amount;
      await _db.Stockpile.UpdateAsync(stock);
      await _db.SaveDatabaseAsync();
    }
  }

  public string GetRandomPalyerName(Faction team) {
    return _factionGenerals[team][new Random().Next(0, 5)];
  }

  public async Task GainVictoryPointAsync(int currentRound, IncomeModel currentCount) {
    var stock = _db.Stockpile.FindOne(x => x.Type == StockpileType.VictoryPoints && x.Round == currentRound);

    stock.Amount++;
    currentCount.VictoryPoints++;

    await _db.SaveDatabaseAsync();
    await _db.Stockpile.UpdateAsync(stock);
    await _db.SaveDatabaseAsync();
    await _db.Fuks.InsertAsync(new Fuk() { Date = DateTime.Now, Round = currentRound });
    await _db.SaveDatabaseAsync();
  }

  public async Task BuyOneAsync(StockpileType type, IncomeModel currentCount, int currentRound) {

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

    var stock = _db.Stockpile.FindOne(x => x.Type == type && x.Round == currentRound);

    if (stock.Amount > 0) {
      stock.Amount--;
      await _db.Stockpile.UpdateAsync(stock);
      await _db.SaveDatabaseAsync();
    }
  }

  public IncomeModel GetInitialAmounts(int currentRound) {
    var result = new IncomeModel();

    foreach (var stock in _db.Stockpile.Find(x => x.Round == currentRound)) {
      switch (stock.Type) {
        case StockpileType.Manpower:
          result.Manpower = stock.InitialAmount;
          break;
        case StockpileType.Ammo:
          result.Ammo = stock.InitialAmount;
          break;
        case StockpileType.Fuel:
          result.Fuel = stock.InitialAmount;
          break;
        case StockpileType.VictoryPoints:
          result.VictoryPoints = stock.InitialAmount;
          break;
      }
    }

    return result;
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
      await _db.Stockpile.UpdateAsync(stock);
      await _db.SaveDatabaseAsync();
    }
  }

  public async Task<(IncomeModel currentIncome, IncomeModel currentCount, Game currentGame, List<int> vps)>
    InitializeGameAsync(GameEdition edition, Faction faction, string name, int targetVpPick = 15) {
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
      VictoryPoints = targetVpPick,
      PalyerName = name,
      Faction = faction
    };
    await NewRoundAsync(currentIncome, currentCount, currentRound, currentGame);

    await _db.Game.DeleteAllAsync();
    await _db.Game.InsertAsync(currentGame);
    await _db.SaveDatabaseAsync();

    return (currentIncome, currentCount, currentGame, vps);
  }

  public async Task DeleteAllAsync() {
    await _db.Income.DeleteAllAsync();
    await _db.Stockpile.DeleteAllAsync();
    await _db.Fuks.DeleteAllAsync();
    await _db.Game.DeleteAllAsync();
    await _db.SaveDatabaseAsync();
  }

  public async Task DeleteGameAsync(Guid id) {

    //await _backend.DeleteGameAsync(id);
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
    await _db.Income.InsertAsync(new CompanyCommander.DB.Income { Amount = currentIncome.Manpower, Type = StockpileType.Manpower, Round = currentRound, Date = DateTime.Now });
    await _db.Income.InsertAsync(new CompanyCommander.DB.Income { Amount = currentIncome.Ammo, Type = StockpileType.Ammo, Round = currentRound, Date = DateTime.Now });
    await _db.Income.InsertAsync(new CompanyCommander.DB.Income { Amount = currentIncome.Fuel, Type = StockpileType.Fuel, Round = currentRound, Date = DateTime.Now });
    await _db.Income.InsertAsync(new CompanyCommander.DB.Income { Amount = currentIncome.VictoryPoints, Type = StockpileType.VictoryPoints, Round = currentRound, Date = DateTime.Now });

    await _db.Stockpile.InsertAsync(new Stockpile { InitialAmount = currentCount.Manpower, Amount = currentCount.Manpower, Type = StockpileType.Manpower, Round = currentRound, Date = DateTime.Now });
    await _db.Stockpile.InsertAsync(new Stockpile { InitialAmount = currentCount.Ammo, Amount = currentCount.Ammo, Type = StockpileType.Ammo, Round = currentRound, Date = DateTime.Now });
    await _db.Stockpile.InsertAsync(new Stockpile { InitialAmount = currentCount.Fuel, Amount = currentCount.Fuel, Type = StockpileType.Fuel, Round = currentRound, Date = DateTime.Now });
    await _db.Stockpile.InsertAsync(new Stockpile { InitialAmount = currentCount.VictoryPoints, Amount = currentCount.VictoryPoints, Type = StockpileType.VictoryPoints, Round = currentRound, Date = DateTime.Now });

    //currentGame.SavedRound = currentRound;
    //_db.Game.Update(currentGame);

    //await SaveBackendAsync(currentIncome, currentCount, currentRound, currentGame);
  }

  public async Task SaveBackendAsync(int currentRound, Game currentGame, GameState state) {

    for (int i = currentGame.SavedRound; i <= currentRound; i++) {
      var inc = _db.Income.Find(x => x.Round == i);
      var stock = _db.Stockpile.Find(x => x.Round == i);
      var currentIncome = new IncomeModel();
      var currentCount = new IncomeModel();

      if (inc.Any() && stock.Any()) {
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

        if (i > 0) {
          if (i == currentRound) {
            await SaveBackendAsync(currentIncome, currentCount, i, currentGame, state);
          }
          else {
            await SaveBackendAsync(currentIncome, currentCount, i, currentGame, GameState.Open);
          }
        }
      }
      else {

      }

      currentGame.SavedRound = currentRound;
    }
    await _db.Game.UpdateAsync(currentGame);
    await _db.SaveDatabaseAsync();
  }

  public async Task<ICollection<GameInfo>> FindRoundsAsync(string term) {

    return await _backend.FindRoundsAsync(term);
  }

  private async Task SaveBackendAsync(IncomeModel currentIncome, IncomeModel currentCount, int currentRound, Game currentGame, GameState state) {
    //var backend = new CompanyCommander.Backend.BackendDataContext("https://localhost:7027/", new HttpClient());

    await _backend.CollectIncomeAsync(new CompanyCommander.Backend.Round() {
      PlayerName = currentGame.PalyerName,
      Faction = currentGame.Faction,
      CurrentState = state,

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
  private BackendDataContext _backend = new BackendDataContext("https://solarsphereapi-gybwcpf8ade9chbj.germanywestcentral-01.azurewebsites.net/", new HttpClient());


  public async Task<ICollection<Round>> GetRoundsAsync(Guid id) {
    return await _backend.GetRoundsAsync(id);
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
    await NewRoundAsync(currentIncome, currentCount, currentRound, currentGame);

    if (currentGame != null && currentCount.VictoryPoints >= currentGame.VictoryPoints) {
      await modal.ShowAsync();
      await SaveBackendAsync(currentRound, currentGame, GameState.Win);
    }
    await _db.SaveDatabaseAsync();
  }

  public async Task ClearDatabaseAsync() {
    await _db.ClearLocalStorageAsync();
    await _db.LoadDatabaseAsync();
  }

  public async Task<(List<GameEdition> editions, bool isRunning, List<Faction> factions)> LoadDatabaseAsync() {
    await _db.LoadDatabaseAsync();

    return (((GameEdition[])Enum.GetValues(typeof(GameEdition))).ToList(), _db.Game.Count() == 0, ((Faction[])Enum.GetValues(typeof(Faction))).ToList());
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
