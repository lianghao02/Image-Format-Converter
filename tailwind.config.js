/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./*.{html,js}"],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'Noto Sans TC', 'sans-serif'],
      },
      colors: {
        primary: '#2c3e50',
        accent: '#3498db',
        success: '#27ae60',
        warning: '#e67e22',
        danger: '#c0392b',
        bg: '#f4f6f9',
        'card-bg': '#ffffff',
      }
    },
  },
  plugins: [],
}
