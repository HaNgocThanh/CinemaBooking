namespace CinemaBooking.Domain.Enums;

/// <summary>
/// Trang thai phim do Admin quan ly truc tiep.
/// </summary>
public enum MovieStatus
{
    /// <summary>Phim sap chieu - chua den ngay cong chieu.</summary>
    ComingSoon = 0,

    /// <summary>Phim dang chieu - dang duoc cong chieu.</summary>
    NowShowing = 1,

    /// <summary>Phim ngung chieu - da ket thuc hoac bi huy.</summary>
    Stopped = 2
}
