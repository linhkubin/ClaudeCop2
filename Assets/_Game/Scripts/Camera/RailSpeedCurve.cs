using UnityEngine;

namespace ClaudeCop.Camera
{
    /// <summary>
    /// CAM-VC2: duong cong toc do ray kieu Virtua Cop 2 (logic thuan). Tang toc nhanh ra dau ray (smoothstep accelTime), chay deu, roi giam toc MEM
    /// tren decelFraction quang duong cuoi (toc do = v * (1 - smoothstep) nen gia toc lien tuc, khong dung dot ngot).
    /// Tong thoi gian chon de BANG thoi gian cua hinh thang cu (targetTime) nen khong lam man dai hon.
    /// </summary>
    public sealed class RailSpeedCurve
    {
        public float Length { get; private set; }
        public float Cruise { get; private set; }
        public float AccelTime { get; private set; }
        public float DecelTime { get; private set; }
        public float Total { get; private set; }
        float da, dd;

        /// <summary>Thoi gian tong cua hinh thang cu (tang toc tuyen tinh easeIn, giam easeOut) o toc do v.</summary>
        public static float TrapezoidTotal(float length, float v, float easeIn, float easeOut)
        {
            float da0 = v * easeIn * 0.5f, db0 = v * easeOut * 0.5f;
            if (da0 + db0 > length) { float k = length / (da0 + db0); easeIn *= k; easeOut *= k; da0 *= k; db0 *= k; }
            return easeIn + easeOut + (length - da0 - db0) / v;
        }

        /// <summary>
        /// targetTime = thoi gian can giu; v toi da la maxSpeed (neu vuot thi chap nhan tong dai hon chut). Khi qua ngan cho tang/giam toc thi co lai.
        /// </summary>
        public RailSpeedCurve(float length, float targetTime, float accelTime, float decelFraction, float maxSpeed)
        {
            Length = Mathf.Max(0.01f, length);
            decelFraction = Mathf.Clamp(decelFraction, 0.05f, 0.6f);
            accelTime = Mathf.Max(0.05f, accelTime);
            // total = accel/2 + (1+f)L/v  =>  v = (1+f)L / (target - accel/2)
            float denom = targetTime - accelTime * 0.5f;
            float v = denom > 0.2f ? (1f + decelFraction) * Length / denom : maxSpeed;
            Cruise = Mathf.Clamp(v, 0.1f, Mathf.Max(0.1f, maxSpeed));
            AccelTime = accelTime;
            dd = decelFraction * Length;
            DecelTime = 2f * dd / Cruise;
            da = Cruise * AccelTime * 0.5f;
            if (da + dd > Length)
            {
                // Rail qua ngan: co tang toc lai de vua
                float room = Mathf.Max(0.01f, Length - dd);
                AccelTime = 2f * room / Cruise;
                da = room;
            }
            Total = AccelTime + DecelTime + (Length - da - dd) / Cruise;
        }

        static float S(float k) { k = Mathf.Clamp01(k); return k * k * (3f - 2f * k); }
        static float SI(float k) { k = Mathf.Clamp01(k); return k * k * k - 0.5f * k * k * k * k; } // tich phan cua smoothstep 0..k

        /// <summary>Khoang cach da di va toc do tai thoi diem t (giay tu luc xuat phat).</summary>
        public void Evaluate(float t, out float dist, out float speed)
        {
            if (t <= 0f) { dist = 0f; speed = 0f; return; }
            if (t >= Total) { dist = Length; speed = 0f; return; }
            if (t < AccelTime)
            {
                float k = t / AccelTime;
                dist = Cruise * AccelTime * SI(k); speed = Cruise * S(k);
            }
            else if (t < Total - DecelTime)
            {
                dist = da + Cruise * (t - AccelTime); speed = Cruise;
            }
            else
            {
                float e = t - (Total - DecelTime);
                float k = e / DecelTime;
                float travelled = Cruise * DecelTime * (k - SI(k));
                dist = Length - dd + travelled; speed = Cruise * (1f - S(k));
            }
            dist = Mathf.Clamp(dist, 0f, Length);
        }
    }
}
