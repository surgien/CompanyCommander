using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CompanyCommander.Model {
  public class IncomeModel : INotifyPropertyChanged {
    private int manpower;
    private int ammo;
    private int fuel;
    private int victoryPoints;
    public IncomeModel() {
        
    }

    public int Manpower {
      get => manpower;
      set {
        if (manpower != value) {
          manpower = value;
          OnPropertyChanged();
        }
      }
    }

    public int Ammo {
      get => ammo;
      set {
        if (ammo != value) {
          ammo = value;
          OnPropertyChanged();
        }
      }
    }

    public int Fuel {
      get => fuel;
      set {
        if (fuel != value) {
          fuel = value;
          OnPropertyChanged();
        }
      }
    }

    public int VictoryPoints {
      get => victoryPoints;
      set {
        if (victoryPoints != value) {
          victoryPoints = value;
          OnPropertyChanged();
        }
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
