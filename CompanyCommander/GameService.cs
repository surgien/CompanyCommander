//using CompanyCommander.DB;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//public class GameService {
//  private readonly AppDbContext db;

//  public GameService(AppDbContext dbContext) {
//    db = dbContext;
//  }

//  public async Task LoadDatabaseAsync() {
//    await db.LoadDatabaseAsync();
//  }

//  public async Task ClearLocalStorageAsync() {
//    await db.ClearLocalStorageAsync();
//  }

//  public async Task SaveDatabaseAsync() {
//    await db.SaveDatabaseAsync();
//  }

//  public async Task Init(GameEdition edition, bool withDialog = true) {
//    // Implementation of Init method
//  }

//  public void Load() {
//    // Implementation of Load method
//  }

//  public void NewRound() {
//    // Implementation of NewRound method
//  }

//  public async Task NextRoundAsync() {
//    // Implementation of NextRoundAsync method
//  }

//  public async Task AppResetAsync() {
//    await ClearLocalStorageAsync();
//    await LoadDatabaseAsync();
//    await Init(GameEdition.FirstEditionProWithErrata, false);
//  }

//  public async Task NewGameAsync(GameEdition edition) {
//    // Implementation of NewGameAsync method
//  }

//  public void IncrementCount(StockpileType type, ref int currentIncomeManpower, ref int currentIncomeAmmo, ref int currentIncomeFuel, ref int currentIncomeVictoryPoints) {
//    switch (type) {
//      case StockpileType.Manpower:
//        currentIncomeManpower++;
//        break;
//      case StockpileType.Ammo:
//        currentIncomeAmmo++;
//        break;
//      case StockpileType.Fuel:
//        currentIncomeFuel++;
//        break;
//      case StockpileType.VictoryPoints:
//        currentIncomeVictoryPoints++;
//        break;
//    }
//  }

//  public void DecrementCount(StockpileType type, ref int currentIncomeManpower, ref int currentIncomeAmmo, ref int currentIncomeFuel, ref int currentIncomeVictoryPoints) {
//    switch (type) {
//      case StockpileType.Manpower:
//        if (currentIncomeManpower > 0)
//          currentIncomeManpower--;
//        break;
//      case StockpileType.Ammo:
//        if (currentIncomeAmmo > 0)
//          currentIncomeAmmo--;
//        break;
//      case StockpileType.Fuel:
//        if (currentIncomeFuel > 0)
//          currentIncomeFuel--;
//        break;
//      case StockpileType.VictoryPoints:
//        if (currentIncomeVictoryPoints > 0)
//          currentIncomeVictoryPoints--;
//        break;
//    }
//  }

//  public async Task UndoOne(StockpileType type, int currentRound, ref int currentCountManpower, ref int currentCountAmmo, ref int currentCountFuel) {
//    var stock = db.Stockpile.FindOne(x => x.Type == type && x.Round == currentRound);

//    if (stock.Amount < stock.InitialAmount) {
//      stock.Amount++;
//      db.Stockpile.Update(stock);
//      await SaveDatabaseAsync();
//    }

//    switch (type) {
//      case StockpileType.Manpower:
//        if (currentCountManpower < stock.InitialAmount)
//          currentCountManpower++;
//        break;
//      case StockpileType.Ammo:
//        if (currentCountAmmo < stock.InitialAmount)
//          currentCountAmmo++;
//        break;
//      case StockpileType.Fuel:
//        if (currentCountFuel < stock.InitialAmount)
//          currentCountFuel++;
//        break;
//    }
//  }

//  public async Task BuyOne(StockpileType type, int currentRound, ref int currentCountManpower, ref int currentCountAmmo, ref int currentCountFuel) {
//    var stock = db.Stockpile.FindOne(x => x.Type == type && x.Round == currentRound);

//    if (stock.Amount > 0) {
//      stock.Amount--;
//      db.Stockpile.Update(stock);
//      await SaveDatabaseAsync();
//    }

//    switch (type) {
//      case StockpileType.Manpower:
//        if (currentCountManpower > 0)
//          currentCountManpower--;
//        break;
//      case StockpileType.Ammo:
//        if (currentCountAmmo > 0)
//          currentCountAmmo--;
//        break;
//      case StockpileType.Fuel:
//        if (currentCountFuel > 0)
//          currentCountFuel--;
//        break;
//    }
//  }

//  public async Task GainVp(int currentRound, ref int currentCountVictoryPoints) {
//    currentCountVictoryPoints++;
//    db.Fuks.Insert(new Fuk() { Date = DateTime.Now, Round = currentRound });
//    await SaveDatabaseAsync();
//  }
//}
