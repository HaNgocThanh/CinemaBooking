import React from 'react';

/**
 * Presentational component for movie card
 * No hooks, no API calls - pure presentational
 */
export function MovieCard({ movie, onSelect, isLoading }) {
  return (
    <div
      className="flex flex-col rounded-lg overflow-hidden bg-white shadow-md hover:shadow-lg transition-shadow cursor-pointer"
      onClick={() => !isLoading && onSelect(movie.id)}
    >
      <div className="relative h-64 bg-gray-200 overflow-hidden">
        {movie.posterUrl ? (
          <img
            src={movie.posterUrl}
            alt={movie.title}
            className="w-full h-full object-cover"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center bg-gray-300">
            <span className="text-gray-500">No image</span>
          </div>
        )}
      </div>

      <div className="flex-1 p-4 flex flex-col justify-between">
        <div>
          <h3 className="text-lg font-semibold text-gray-800 line-clamp-2">
            {movie.title}
          </h3>
          <p className="text-sm text-gray-600 mt-2 line-clamp-2">
            {movie.description}
          </p>
        </div>

        <div className="mt-4 flex items-center justify-between">
          <span className="text-sm font-medium text-gray-700">
            {movie.durationMinutes} min
          </span>
          <span className="text-xs px-2 py-1 bg-primary text-white rounded">
            {movie.rating}
          </span>
        </div>
      </div>
    </div>
  );
}
