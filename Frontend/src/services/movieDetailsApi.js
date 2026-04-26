import axiosClient from './axiosClient';

/**
 * Lay chi tiet phim kem danh sach suat chieu.
 * @param {number} movieId
 * @returns {Promise}
 */
export const getMovieDetails = async (movieId) => {
  const response = await axiosClient.get(`/movies/${movieId}/showtimes`);
  return response;
};

/**
 * Lay chi tiet phim don le (khong co suat chieu).
 * @param {number} movieId
 * @returns {Promise}
 */
export const getMovieById = async (movieId) => {
  const response = await axiosClient.get(`/movies/${movieId}`);
  return response;
};
