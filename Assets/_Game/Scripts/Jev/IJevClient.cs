using System;

namespace ClaudeCop.Jev
{
    /// <summary>Tay cam cho mot cuoc goi dang chay; huy duoc.</summary>
    public sealed class JevCall
    {
        public bool IsDone { get; private set; }
        public bool IsCancelled { get; private set; }
        readonly Action cancelHook;

        public JevCall(Action onCancel = null) { cancelHook = onCancel; }

        public void Cancel()
        {
            if (IsDone) return;
            IsCancelled = true; IsDone = true;
            cancelHook?.Invoke();
        }

        public void Complete() { IsDone = true; }
    }

    /// <summary>
    /// Client Jev. Callback luon duoc goi tren main thread; client online phai marshal ve.
    /// Neu qua timeoutSeconds thi callback nhan JevResponse.Timeout(). Bi Cancel thi KHONG goi callback.
    /// </summary>
    public interface IJevClient
    {
        JevCall Ask(JevRequest request, float timeoutSeconds, Action<JevResponse> onDone);
    }
}
