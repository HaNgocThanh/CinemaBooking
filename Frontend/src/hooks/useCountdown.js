import { useState, useEffect } from 'react';

/**
 * Custom hook for countdown timer
 * @param {number} totalSeconds - Total seconds for countdown (default 300 = 5 minutes)
 * @returns {Object} - { secondsLeft, minutes, seconds, isExpired, formattedTime }
 */
export function useCountdown(totalSeconds = 300) {
  const [secondsLeft, setSecondsLeft] = useState(totalSeconds);

  useEffect(() => {
    if (secondsLeft <= 0) return;

    const interval = setInterval(() => {
      setSecondsLeft((prev) => prev - 1);
    }, 1000);

    return () => clearInterval(interval);
  }, [secondsLeft]);

  const minutes = Math.floor(secondsLeft / 60);
  const seconds = secondsLeft % 60;

  return {
    secondsLeft,
    minutes,
    seconds,
    isExpired: secondsLeft <= 0,
    formattedTime: `${minutes}:${seconds.toString().padStart(2, '0')}`,
  };
}
