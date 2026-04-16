// ⚠️ This file has been moved to CinemaBooking.Application.DTOs.Common.ApiSuccessResponse
// Please import from CinemaBooking.Application.DTOs.Common instead of CinemaBooking.API.Responses

using CinemaBooking.Application.DTOs.Common;

namespace CinemaBooking.API.Responses;

[Obsolete("Use CinemaBooking.Application.DTOs.Common.ApiSuccessResponse instead", error: false)]
public class ApiSuccessResponse<T> : global::CinemaBooking.Application.DTOs.Common.ApiSuccessResponse<T>
{
}
