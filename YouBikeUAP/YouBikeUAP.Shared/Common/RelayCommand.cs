namespace YouBikeUAP.Common
{
  using System;
  using System.Windows.Input;

  /// <summary>
  /// 這個命令唯一的用途就是叫用委派來將其功能轉送
  /// 給其他物件。
  /// CanExecute 方法的預設傳回值為 'true'。
  /// 呼叫 <see cref="RaiseCanExecuteChanged"/> 的時機是每當
  /// <see cref="CanExecute"/> 必須傳回不同值。
  /// </summary>
  public class RelayCommand : ICommand
  {
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    /// <summary>
    /// 於呼叫 RaiseCanExecuteChanged 時引發。
    /// </summary>
    public event EventHandler CanExecuteChanged;

    /// <summary>
    /// 建立始終可以執行的新命令。
    /// </summary>
    /// <param name="execute">執行邏輯。</param>
    public RelayCommand(Action execute)
        : this(execute, null)
    {
    }

    /// <summary>
    /// 建立新命令。
    /// </summary>
    /// <param name="execute">執行邏輯。</param>
    /// <param name="canExecute">執行狀態邏輯。</param>
    public RelayCommand(Action execute, Func<bool> canExecute)
    {
      if (execute == null)
        throw new ArgumentNullException("execute");
      _execute = execute;
      _canExecute = canExecute;
    }

    /// <summary>
    /// 判斷這個 <see cref="RelayCommand"/> 能否以其目前狀態執行。
    /// </summary>
    /// <param name="parameter">
    /// 命令所使用的資料。如果不必傳遞資料給命令，可將這個物件設為 null。
    /// </param>
    /// <returns>如果可執行這個命令，即為 true，否則為 false。</returns>
    public bool CanExecute(object parameter)
    {
      return _canExecute == null ? true : _canExecute();
    }

    /// <summary>
    /// 在目前命令目標上執行 <see cref="RelayCommand"/>。
    /// </summary>
    /// <param name="parameter">
    /// 命令所使用的資料。如果不必傳遞資料給命令，可將這個物件設為 null。
    /// </param>
    public void Execute(object parameter)
    {
      _execute();
    }

    /// <summary>
    /// 這個方法用來引發 <see cref="CanExecuteChanged"/> 事件
    /// 以表示 <see cref="CanExecute"/> 方法的傳回值
    /// 已變更。
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
      var handler = CanExecuteChanged;
      if (handler != null)
      {
        handler(this, EventArgs.Empty);
      }
    }
  }
}