using System;

namespace ClaudeCop.Ads
{
    /// <summary>Quang cao co thuong. Thay FakeRewardedAd bang SDK that sau nay.</summary>
    public interface IRewardedAd
    {
        bool IsReady { get; }
        /// <summary>onRewarded: xem het, tra thuong. onFailed: loi / khong san sang / dong giua chung (khong thuong).</summary>
        void Show(Action onRewarded, Action onFailed);
    }
}
