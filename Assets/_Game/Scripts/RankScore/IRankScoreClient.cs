using System;

namespace ClaudeCop.RankScore
{
    /// <summary>Tay cam cho mot cuoc goi dang chay; huy duoc.</summary>
    public sealed class RankScoreCall
    {
        public bool IsDone { get; private set; }
        public bool IsCancelled { get; private set; }
        readonly Action cancelHook;

        public RankScoreCall(Action onCancel = null) { cancelHook = onCancel; }

        public void Cancel()
        {
            if (IsDone) return;
            IsCancelled = true; IsDone = true;
            cancelHook?.Invoke();
        }

        public void Complete() { IsDone = true; }
    }

    /// <summary>
    /// Client RankScore. Callback luon duoc goi tren main thread; client online phai marshal ve.
    /// Neu qua timeoutSeconds thi callback nhan RankScoreResponse.Timeout(). Bi Cancel thi KHONG goi callback.
    /// </summary>
    public interface IRankScoreClient
    {
        RankScoreCall Ask(RankScoreRequest request, float timeoutSeconds, Action<RankScoreResponse> onDone);
    }
}
