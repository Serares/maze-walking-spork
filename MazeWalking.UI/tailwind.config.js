/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#065f46', // emerald-800
          light: '#047857',   // emerald-700
        },
        secondary: '#1e293b', // slate-800
      },
    },
  },
  plugins: [],
}
