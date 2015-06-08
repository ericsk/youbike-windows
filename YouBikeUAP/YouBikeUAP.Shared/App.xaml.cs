namespace YouBikeUAP
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Runtime.InteropServices.WindowsRuntime;
  using Windows.ApplicationModel;
  using Windows.ApplicationModel.Activation;
  using Windows.Foundation;
  using Windows.Foundation.Collections;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Controls.Primitives;
  using Windows.UI.Xaml.Data;
  using Windows.UI.Xaml.Input;
  using Windows.UI.Xaml.Media;
  using Windows.UI.Xaml.Media.Animation;
  using Windows.UI.Xaml.Navigation;
  using Common;

  // 空白應用程式範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234227

  /// <summary>
  /// 提供應用程式專屬行為以補充預設的應用程式類別。
  /// </summary>
  public sealed partial class App : Application
  {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

    /// <summary>
    /// 初始化單一應用程式物件。這是第一行執行之撰寫程式碼，
    /// 而且其邏輯相當於 main() 或 WinMain()。
    /// </summary>
    public App()
    {
      this.InitializeComponent();
      this.Suspending += this.OnSuspending;
    }

    /// <summary>
    /// 在應用程式由使用者正常啟動時叫用。其他進入點
    /// 將在啟動應用程式以開啟特定檔案時使用，以顯示
    /// 搜尋結果等。
    /// </summary>
    /// <param name="e">關於啟動要求和處理序的詳細資料。</param>
    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
#if DEBUG
      if (System.Diagnostics.Debugger.IsAttached)
      {
        this.DebugSettings.EnableFrameRateCounter = true;
      }
#endif

      Frame rootFrame = Window.Current.Content as Frame;

      // 當視窗已經有內容時，不重複應用程式初始化，
      // 只確定視窗是作用中
      if (rootFrame == null)
      {
        // 建立框架做為巡覽內容，並巡覽至第一頁
        rootFrame = new Frame();

        // TODO: 將這個值變更成適合您應用程式的快取大小
        rootFrame.CacheSize = 1;

        if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
        {
          // TODO: 從之前暫停的應用程式載入狀態
        }

        // 將框架放在目前視窗中
        Window.Current.Content = rootFrame;
      }

      if (rootFrame.Content == null)
      {
#if WINDOWS_PHONE_APP
                // 移除啟動的十字轉門式巡覽。
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

        // 在巡覽堆疊未還原時，巡覽至第一頁，
        // 設定新的頁面，方式是透過傳遞必要資訊做為巡覽
        // 參數
        if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
        {
          throw new Exception("Failed to create initial page");
        }
      }

      // 確定目前視窗是作用中
      Window.Current.Activate();
    }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// 應用程式啟動後還原內容轉換。
        /// </summary>
        /// <param name="sender">處理常式附加的物件。</param>
        /// <param name="e">有關巡覽事件的詳細資料。</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

    /// <summary>
    /// 在應用程式暫停執行時叫用。應用程式狀態會儲存起來，
    /// 但不知道應用程式即將結束或繼續，而且仍將記憶體
    /// 的內容保持不變。
    /// </summary>
    /// <param name="sender">暫停之要求的來源。</param>
    /// <param name="e">有關暫停之要求的詳細資料。</param>
    private void OnSuspending(object sender, SuspendingEventArgs e)
    {
      var deferral = e.SuspendingOperation.GetDeferral();

      // TODO: 儲存應用程式狀態，並停止任何背景活動
      deferral.Complete();
    }
  }
}