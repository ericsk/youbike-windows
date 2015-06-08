namespace YouBikeUAP.Common
{
  using System;
  using System.Collections.Generic;
  using Windows.Foundation.Metadata;
  using Windows.System;
  using Windows.UI.Core;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Navigation;

  [WebHostHidden]
  public class NavigationHelper : DependencyObject
  {
    private Page Page { get; set; }
    private Frame Frame { get { return this.Page.Frame; } }

    /// <summary>
    /// 初始化 <see cref="NavigationHelper"/> 類別的新執行個體。
    /// </summary>
    /// <param name="page">巡覽所使用之目前頁面的參考。
    /// 這項參考允許進行框架操作及確保鍵盤
    /// 巡覽要求只有在頁面佔用整個視窗時才會發生。</param>
    public NavigationHelper(Page page)
    {
      this.Page = page;

      // 當這個頁面是視覺化樹狀結構的一部分時，執行兩項變更: 
      // 1) 將應用程式檢視狀態對應到頁面的視覺狀態
      // 2) 處理硬體巡覽要求
      this.Page.Loaded += (sender, e) =>
      {
#if WINDOWS_PHONE_APP
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#else
        // 只有佔用整個視窗時才適用鍵盤和滑鼠巡覽
        if (this.Page.ActualHeight == Window.Current.Bounds.Height &&
            this.Page.ActualWidth == Window.Current.Bounds.Width)
        {
          // 直接接聽視窗，所以不需要焦點
          Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=
              CoreDispatcher_AcceleratorKeyActivated;
          Window.Current.CoreWindow.PointerPressed +=
              this.CoreWindow_PointerPressed;
        }
#endif
      };

      // 當頁面不再可見時，復原相同的變更
      this.Page.Unloaded += (sender, e) =>
      {
#if WINDOWS_PHONE_APP
                Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
#else
        Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -=
            CoreDispatcher_AcceleratorKeyActivated;
        Window.Current.CoreWindow.PointerPressed -=
            this.CoreWindow_PointerPressed;
#endif
      };
    }

    #region 巡覽支援

    RelayCommand _goBackCommand;
    RelayCommand _goForwardCommand;

    /// <summary>
    /// <see cref="RelayCommand"/> 用來繫結至 [上一頁] 按鈕的 Command 屬性
    /// 巡覽向後巡覽記錄中最新的項目，若 Frame
    /// 管理它自己的巡覽記錄。
    /// 
    /// <see cref="RelayCommand"/> 已設定為使用虛擬方法 <see cref="GoBack"/>
    /// 視同執行動作和 CanExecute 的 <see cref="CanGoBack"/>。
    /// </summary>
    public RelayCommand GoBackCommand
    {
      get
      {
        if (_goBackCommand == null)
        {
          _goBackCommand = new RelayCommand(
              () => this.GoBack(),
              () => this.CanGoBack());
        }
        return _goBackCommand;
      }
      set
      {
        _goBackCommand = value;
      }
    }
    /// <summary>
    /// <see cref="RelayCommand"/> 用於巡覽至其中的最新項目
    /// 向前巡覽記錄，若 Frame 管理它自己的巡覽記錄。
    /// 
    /// <see cref="RelayCommand"/> 已設定為使用虛擬方法 <see cref="GoForward"/>
    /// 視同執行動作和 CanExecute 的 <see cref="CanGoForward"/>。
    /// </summary>
    public RelayCommand GoForwardCommand
    {
      get
      {
        if (_goForwardCommand == null)
        {
          _goForwardCommand = new RelayCommand(
              () => this.GoForward(),
              () => this.CanGoForward());
        }
        return _goForwardCommand;
      }
    }

    /// <summary>
    /// <see cref="GoBackCommand"/> 屬性所使用的虛擬方法
    /// 判斷 <see cref="Frame"/> 是否可以返回。
    /// </summary>
    /// <returns>
    /// 若 <see cref="Frame"/> 至少有一個項目則為 true
    /// 在向後巡覽記錄中。
    /// </returns>
    public virtual bool CanGoBack()
    {
      return this.Frame != null && this.Frame.CanGoBack;
    }
    /// <summary>
    /// <see cref="GoForwardCommand"/> 屬性所使用的虛擬方法
    /// 判斷 <see cref="Frame"/> 是否可以向前。
    /// </summary>
    /// <returns>
    /// 若 <see cref="Frame"/> 至少有一個項目則為 true
    /// 在向前巡覽記錄中。
    /// </returns>
    public virtual bool CanGoForward()
    {
      return this.Frame != null && this.Frame.CanGoForward;
    }

    /// <summary>
    /// <see cref="GoBackCommand"/> 屬性所使用的虛擬方法
    /// 叫用 <see cref="Windows.UI.Xaml.Controls.Frame.GoBack"/> 方法。
    /// </summary>
    public virtual void GoBack()
    {
      if (this.Frame != null && this.Frame.CanGoBack) this.Frame.GoBack();
    }
    /// <summary>
    /// <see cref="GoForwardCommand"/> 屬性所使用的虛擬方法
    /// 叫用 <see cref="Windows.UI.Xaml.Controls.Frame.GoForward"/> 方法。
    /// </summary>
    public virtual void GoForward()
    {
      if (this.Frame != null && this.Frame.CanGoForward) this.Frame.GoForward();
    }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// 在按下硬體上一頁按鈕時叫用。僅限於 Windows Phone。
        /// </summary>
        /// <param name="sender">觸發事件的執行個體。</param>
        /// <param name="e">描述造成事件之狀況的事件資料。</param>
        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (this.GoBackCommand.CanExecute(null))
            {
                e.Handled = true;
                this.GoBackCommand.Execute(null);
            }
        }
#else
    /// <summary>
    /// 當這個頁面作用中且佔用整個視窗時，在按下每個按鍵時叫用，
    /// 包括如 Alt 鍵組合等系統按鍵。用來偵測頁面之間的鍵盤巡覽，
    /// 即使頁面本身沒有焦點。
    /// </summary>
    /// <param name="sender">觸發事件的執行個體。</param>
    /// <param name="e">描述造成事件之狀況的事件資料。</param>
    private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender,
        AcceleratorKeyEventArgs e)
    {
      var virtualKey = e.VirtualKey;

      // 只在按下左、右或專用的 [上一頁] 或 [下一頁] 按鍵時才
      // 進一步調查
      if ((e.EventType == CoreAcceleratorKeyEventType.SystemKeyDown ||
          e.EventType == CoreAcceleratorKeyEventType.KeyDown) &&
          (virtualKey == VirtualKey.Left || virtualKey == VirtualKey.Right ||
          (int)virtualKey == 166 || (int)virtualKey == 167))
      {
        var coreWindow = Window.Current.CoreWindow;
        var downState = CoreVirtualKeyStates.Down;
        bool menuKey = (coreWindow.GetKeyState(VirtualKey.Menu) & downState) == downState;
        bool controlKey = (coreWindow.GetKeyState(VirtualKey.Control) & downState) == downState;
        bool shiftKey = (coreWindow.GetKeyState(VirtualKey.Shift) & downState) == downState;
        bool noModifiers = !menuKey && !controlKey && !shiftKey;
        bool onlyAlt = menuKey && !controlKey && !shiftKey;

        if (((int)virtualKey == 166 && noModifiers) ||
            (virtualKey == VirtualKey.Left && onlyAlt))
        {
          // 按下上一頁按鍵或 Alt+Left 時，向後巡覽
          e.Handled = true;
          this.GoBackCommand.Execute(null);
        }
        else if (((int)virtualKey == 167 && noModifiers) ||
            (virtualKey == VirtualKey.Right && onlyAlt))
        {
          // 按下下一頁按鍵或 Alt+Right 時，向前巡覽
          e.Handled = true;
          this.GoForwardCommand.Execute(null);
        }
      }
    }

    /// <summary>
    /// 當這個頁面作用中且佔用整個視窗時，在每個滑鼠點按、觸控螢幕點選
    /// 或對等的互動時叫用。用來偵測瀏覽器的下一頁和
    /// 上一頁滑鼠按鈕點選，以在頁面之間巡覽。
    /// </summary>
    /// <param name="sender">觸發事件的執行個體。</param>
    /// <param name="e">描述造成事件之狀況的事件資料。</param>
    private void CoreWindow_PointerPressed(CoreWindow sender,
        PointerEventArgs e)
    {
      var properties = e.CurrentPoint.Properties;

      // 忽略含左、右和中間按鈕的按鈕同步選取
      if (properties.IsLeftButtonPressed || properties.IsRightButtonPressed ||
          properties.IsMiddleButtonPressed)
        return;

      // 如果按下上一頁或下一頁 (但不是兩者都按)，即適當巡覽
      bool backPressed = properties.IsXButton1Pressed;
      bool forwardPressed = properties.IsXButton2Pressed;
      if (backPressed ^ forwardPressed)
      {
        e.Handled = true;
        if (backPressed) this.GoBackCommand.Execute(null);
        if (forwardPressed) this.GoForwardCommand.Execute(null);
      }
    }
#endif

    #endregion

    #region 處理程序生命週期管理

    private String _pageKey;

    /// <summary>
    /// 在目前頁面上註冊此事件以填入頁面
    /// 以巡覽期間傳遞的內容，以及任何儲存的
    /// 從先前工作階段重新建立頁面時，也會提供狀態。
    /// </summary>
    public event LoadStateEventHandler LoadState;
    /// <summary>
    /// 在目前頁面上註冊此事件以保留
    /// 下列狀況下與目前頁面相關聯之狀態
    /// 暫停應用程式或捨棄頁面的來源
    /// 巡覽快取。
    /// </summary>
    public event SaveStateEventHandler SaveState;

    /// <summary>
    /// 在此頁面即將顯示在框架中時叫用。
    /// 這個方法會呼叫 <see cref="LoadState"/>，其中所有頁面專屬
    /// 應該放置巡覽和處理程序生命週期管理邏輯。
    /// </summary>
    /// <param name="e">描述如何到達此頁面的事件資料。Parameter
    /// 屬性會提供要顯示之群組。</param>
    public void OnNavigatedTo(NavigationEventArgs e)
    {
      var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
      this._pageKey = "Page-" + this.Frame.BackStackDepth;

      if (e.NavigationMode == NavigationMode.New)
      {
        // 有新頁面加入巡覽堆疊時，清除向前巡覽的
        // 現有狀態
        var nextPageKey = this._pageKey;
        int nextPageIndex = this.Frame.BackStackDepth;
        while (frameState.Remove(nextPageKey))
        {
          nextPageIndex++;
          nextPageKey = "Page-" + nextPageIndex;
        }

        // 將巡覽參數傳遞給新頁面
        if (this.LoadState != null)
        {
          this.LoadState(this, new LoadStateEventArgs(e.Parameter, null));
        }
      }
      else
      {
        // 將巡覽參數和保留的頁面狀態傳遞給頁面，且使用
        // 與載入暫停狀態和根據快取重新建立捨棄的頁面
        // 一樣的策略
        if (this.LoadState != null)
        {
          this.LoadState(this, new LoadStateEventArgs(e.Parameter, (Dictionary<String, Object>)frameState[this._pageKey]));
        }
      }
    }

    /// <summary>
    /// 在框架中不再顯示這個頁面時叫用。
    /// 這個方法會呼叫 <see cref="SaveState"/>，其中所有頁面專屬
    /// 應該放置巡覽和處理程序生命週期管理邏輯。
    /// </summary>
    /// <param name="e">描述如何到達此頁面的事件資料。Parameter
    /// 屬性會提供要顯示之群組。</param>
    public void OnNavigatedFrom(NavigationEventArgs e)
    {
      var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
      var pageState = new Dictionary<String, Object>();
      if (this.SaveState != null)
      {
        this.SaveState(this, new SaveStateEventArgs(pageState));
      }
      frameState[_pageKey] = pageState;
    }

    #endregion
  }

  /// <summary>
  /// 表示方法將處理 <see cref="NavigationHelper.LoadState"/> 事件
  /// </summary>
  public delegate void LoadStateEventHandler(object sender, LoadStateEventArgs e);
  /// <summary>
  /// 表示方法將處理 <see cref="NavigationHelper.SaveState"/> 事件
  /// </summary>
  public delegate void SaveStateEventHandler(object sender, SaveStateEventArgs e);

  /// <summary>
  /// 頁面試圖載入狀態時必須有用來保存事件資料之類別。
  /// </summary>
  public class LoadStateEventArgs : EventArgs
  {
    /// <summary>
    /// 傳遞至 <see cref="Frame.Navigate(Type, Object)"/> 的參數值
    /// 的參數值。
    /// </summary>
    public Object NavigationParameter { get; private set; }
    /// <summary>
    /// 這個頁面在先前的工作階段期間保留的狀態字典
    /// 工作階段。第一次瀏覽頁面時，這一項是 null。
    /// </summary>
    public Dictionary<string, Object> PageState { get; private set; }

    /// <summary>
    /// 初始化 <see cref="LoadStateEventArgs"/> 類別的新執行個體。
    /// </summary>
    /// <param name="navigationParameter">
    /// 傳遞至 <see cref="Frame.Navigate(Type, Object)"/> 的參數值
    /// 的參數值。
    /// </param>
    /// <param name="pageState">
    /// 這個頁面在先前的工作階段期間保留的狀態字典
    /// 工作階段。第一次瀏覽頁面時，這一項是 null。
    /// </param>
    public LoadStateEventArgs(Object navigationParameter, Dictionary<string, Object> pageState)
        : base()
    {
      this.NavigationParameter = navigationParameter;
      this.PageState = pageState;
    }
  }
  /// <summary>
  /// 頁面試圖儲存狀態時必須有用來保存事件資料之類別。
  /// </summary>
  public class SaveStateEventArgs : EventArgs
  {
    /// <summary>
    /// 即將以可序列化狀態填入的空白字典。
    /// </summary>
    public Dictionary<string, Object> PageState { get; private set; }

    /// <summary>
    /// 初始化 <see cref="SaveStateEventArgs"/> 類別的新執行個體。
    /// </summary>
    /// <param name="pageState">即將以可序列化狀態填入的空白字典。</param>
    public SaveStateEventArgs(Dictionary<string, Object> pageState)
        : base()
    {
      this.PageState = pageState;
    }
  }
}
