using System.Linq;
using System.Threading.Tasks;

public class TestUnit
{
    /// <summary>總共幾個站位，會在 controller Init 時呼叫 InitializeStations 設定</summary>
    public int StationCount { get; private set; }

    /// <summary>用來記錄每個站位是否已完成</summary>
    public bool[] StationCompleted { get; private set; }

    public int CurrentStationIndex { get; set; }
    public bool IsActive { get; set; }
    public bool IsTestEnd { get; set; }
    public bool IsSkip { get; set; } = false;

    public string ShowStatus { get; set; } = string.Empty;
    // 每次旋轉需要等待的同步物件
    private TaskCompletionSource<bool> _rotationSignal = new TaskCompletionSource<bool>();

    /// <summary>初始化站位數</summary>
    public void InitializeStations(int stationCount)
    {
        StationCount = stationCount;
        StationCompleted = new bool[stationCount];
        Reset();  // 也會清空 CurrentStationIndex
    }

    /// <summary>重設為空料狀態，並清掉所有站位完成旗標</summary>
    public void Reset()
    {
        IsActive = false;
        IsTestEnd = false;
        IsSkip = false;
        CurrentStationIndex = 0;
        if (StationCompleted != null)
            for (int i = 0; i < StationCompleted.Length; i++)
                StationCompleted[i] = false;
    }

    /// <summary>標記該 DUT 在目前站位完成測試</summary>
    public void MarkCurrentStationComplete()
    {
        StationCompleted[CurrentStationIndex] = true;
    }
    public void MarkTestEnd()
    {
        // 把「目前站位之後」所有站都標為完成，免得旋轉卡住
        //for (int i = CurrentStationIndex; i < StationCount; i++)
        //{
        //    StationCompleted[i] = true;
        //}
        IsTestEnd = true;
    }
    /// <summary>標記該 DUT 為失敗，並取消後續測試</summary>
    //public void MarkAsFailed()
    //{
    //    IsFailed = true;
    //    IsActive = false;

    //    // 把「目前站位之後」所有站都標為完成，免得旋轉卡住
    //    for (int i = CurrentStationIndex; i < StationCount; i++)
    //    {
    //        StationCompleted[i] = true;
    //    }
    //}

    /// <summary>判斷「所有站位都完成」</summary>
    public bool AllStationsCompleted => StationCompleted != null
                                        && StationCompleted.All(f => f);

    /// <summary>判斷「當前站位完成」</summary>
    public bool IsCurrentStationCompleted => StationCompleted != null
                                             && StationCompleted[CurrentStationIndex];

    /// <summary>等待旋轉結束（由旋轉控制器通知）</summary>
    public Task WaitForRotationAsync() => _rotationSignal.Task;

    /// <summary>由旋轉控制器呼叫，通知旋轉已完成</summary>
    public void NotifyRotationDone()
    {
        if (!_rotationSignal.Task.IsCompleted)
            _rotationSignal.TrySetResult(true);

        _rotationSignal = new TaskCompletionSource<bool>();
    }
}
