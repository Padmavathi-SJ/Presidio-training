/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      animation: {
        'float-slow': 'floatSlow 3s ease-in-out infinite',
        'float-fast': 'floatFast 2s ease-in-out infinite',
        'spin-slow': 'spinSlow 8s linear infinite',
        'bounce-slow': 'bounceSlow 2s ease-in-out infinite',
        'pulse-fast': 'pulseFast 1.5s ease-in-out infinite',
        'wiggle': 'wiggle 3s ease-in-out infinite',
        'blink': 'blink 1s step-end infinite',
        'slide-down': 'slideDown 0.5s ease-out',
        'slide-in': 'slideIn 0.3s ease-out forwards',
      },
      keyframes: {
        floatSlow: {
          '0%, 100%': { transform: 'translateY(0px)' },
          '50%': { transform: 'translateY(-10px)' },
        },
        floatFast: {
          '0%, 100%': { transform: 'translateY(0px)' },
          '50%': { transform: 'translateY(-15px)' },
        },
        spinSlow: {
          'from': { transform: 'rotate(0deg)' },
          'to': { transform: 'rotate(360deg)' },
        },
        bounceSlow: {
          '0%, 100%': { transform: 'translateY(0)' },
          '50%': { transform: 'translateY(-10px)' },
        },
        pulseFast: {
          '0%, 100%': { transform: 'scale(1)' },
          '50%': { transform: 'scale(1.1)' },
        },
        wiggle: {
          '0%, 100%': { transform: 'rotate(0deg)' },
          '25%': { transform: 'rotate(-5deg)' },
          '75%': { transform: 'rotate(5deg)' },
        },
        blink: {
          '0%, 50%': { opacity: '1' },
          '51%, 100%': { opacity: '0' },
        },
        slideDown: {
          'from': { opacity: '0', transform: 'translateY(-50px)' },
          'to': { opacity: '1', transform: 'translateY(0)' },
        },
        slideIn: {
          'to': { opacity: '1', transform: 'translateX(0)' },
        },
      },
    },
  },
  plugins: [],
}