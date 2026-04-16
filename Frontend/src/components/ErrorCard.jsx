import React from 'react';
import { AlertCircle } from 'lucide-react';

export function ErrorCard({ title = 'Error', message, action }) {
  return (
    <div className="flex flex-col items-center justify-center w-full p-6 border border-danger rounded-lg bg-red-50">
      <AlertCircle className="w-10 h-10 text-danger mb-3" />
      <h3 className="text-lg font-semibold text-danger mb-2">{title}</h3>
      {message && <p className="text-center text-gray-600 mb-4">{message}</p>}
      {action && <div>{action}</div>}
    </div>
  );
}
