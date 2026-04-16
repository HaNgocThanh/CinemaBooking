/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: '#1890ff',
        secondary: '#722ed1',
        danger: '#ff4d4f',
        success: '#52c41a',
        warning: '#faad14',
      },
    },
  },
  plugins: [],
  corePlugins: {
    preflight: true,
  },
}
