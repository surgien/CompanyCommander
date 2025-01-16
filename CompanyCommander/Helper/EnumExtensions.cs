using System.ComponentModel;
using System.Reflection;
using LiteDB;

public static class EnumExtensions {

  public static readonly SemaphoreSlim SaveSemaphore = new SemaphoreSlim(1, 1);
  
  public async static Task UpdateAsync<T>(this ILiteCollection<T> list, T entity) {
    await SaveSemaphore.WaitAsync();
    try {
      list.Update(entity);
    }
    finally {
      SaveSemaphore.Release();
    }
  }
  public async static Task InsertAsync<T>(this ILiteCollection<T> list, T entity) {
    await SaveSemaphore.WaitAsync();
    try {
      list.Insert(entity);
    }
    finally {
      SaveSemaphore.Release();
    }
  }
  public async static Task DeleteAllAsync<T>(this ILiteCollection<T> list) {
    await SaveSemaphore.WaitAsync();
    try {
      list.DeleteAll();
    }
    finally {
      SaveSemaphore.Release();
    }
  }

  public static string GetDescription(this Enum value) {
    FieldInfo field = value.GetType().GetField(value.ToString());
    DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
    return attribute == null ? value.ToString() : attribute.Description;
  }
}
